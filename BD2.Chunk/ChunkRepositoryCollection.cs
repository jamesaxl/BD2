/*
 * Copyright (c) 2013-2014 Behrooz Amoozad
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the bd2 nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL Behrooz Amoozad BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * */
using System;
using System.Collections.Generic;
using System.IO;
using BD2.Common;
using System.Security.Cryptography;

namespace BD2.Chunk
{
	/// <summary>
	/// All methods af this class are guaranteed to be thread-safe.
	/// </summary>
	public sealed class ChunkRepositoryCollection : ChunkRepository
	{
		SortedSet<ChunkRepository> repositories = new SortedSet<ChunkRepository> ();

		public void AddRepository (ChunkRepository repository)
		{
			if (repository == null)
				throw new ArgumentNullException ("repository");
			if (repositories.Contains (repository))
				throw new InvalidOperationException ("Cannot add repository to collection more than once.");
			repositories.Add (repository);
		}

		public SortedSet<ChunkRepository> GetRepositories ()
		{
			lock (repositories) {
				return new SortedSet<ChunkRepository> (repositories);
			}
		}

		#region implemented abstract members of ChunkRepository

		public override SortedDictionary<byte[], string> GetUsers ()
		{
			SortedDictionary<byte[], string> users = new SortedDictionary<byte[], string> (ByteSequenceComparer.Shared);
			foreach (var Repo in GetRepositories ()) {
				foreach (var tup in Repo.GetUsers ()) {
					if (!users.ContainsKey (tup.Key)) {
						users.Add (tup.Key, tup.Value);
					}
				}
			}
			return users;
		}

		public override void AddUser (byte[] id, string name)
		{
			foreach (var Repo in GetRepositories ()) {
				Repo.AddUser (id, name);
			}
		}

	 
		public override byte[] PullData (byte[] chunkID)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			foreach (var Repo in GetRepositories ()) {
				byte[] chunkData = Repo.PullData (chunkID);
				if (chunkData != null) {
					return chunkData;
				}
			}
			return null;
		}

		public override byte[][] PullDependencies (byte[] chunkID)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			foreach (var Repo in GetRepositories ()) {
				byte[][] chunkMeta = Repo.PullDependencies (chunkID);
				if (chunkMeta != null) {
					return chunkMeta;
				}
			}
			return null;
		}

		public override void PushIndex (byte[] index, byte[] value)
		{
			if (index == null)
				throw new ArgumentNullException ("index");
			if (value == null)
				throw new ArgumentNullException ("value");
			foreach (ChunkRepository CR in repositories)
				CR.PushIndex (index, value);
		}

		public override byte[] PullIndex (byte[] index)
		{
			if (index == null)
				throw new ArgumentNullException ("index");
			foreach (var Repo in GetRepositories ()) {
				byte[] value = Repo.PullIndex (index);
				if (value != null) {
					return value;
				}
			}
			return null;
		}

		public override void PushSegment (byte[] chunkID, byte[] value)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			if (value == null)
				throw new ArgumentNullException ("value");
			foreach (ChunkRepository CR in repositories)
				CR.PushSegment (chunkID, value);
		}

		public override byte[] PullSegment (byte[] chunkID)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			int cost;
			ChunkRepository repo;
			GetLeastCost (chunkID, out cost, out repo);
			return repo.PullIndex (chunkID);
		}

		public override void Push (byte[] chunkID, byte[] data, byte[] segment, byte[][] dependencies)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			if (data == null)
				throw new ArgumentNullException ("data");
			if (dependencies == null)
				throw new ArgumentNullException ("dependencies");
			foreach (ChunkRepository CR in repositories)
				CR.Push (chunkID, data, segment, dependencies);
		}

		public override int GetLeastCost (int currentMinimum, byte[] chunkID)
		{
			ChunkRepository repository;
			GetLeastCost (chunkID, out currentMinimum, out repository);
			return currentMinimum;
		}

		public void GetLeastCost (byte[] chunkID, out int cost, out ChunkRepository repository)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			SortedSet<ChunkRepository> CRs = GetRepositories ();
			cost = int.MaxValue;
			repository = null;
			//no remote calls, i hope
			foreach (var Repo in CRs) {
				int Current = Repo.GetMaxCostForAny ();
				if (Current < cost) {
					cost = Current;
				}
			}
			//takes a while if object is not available locally or bad heuristics have mislead the first estimation process
			foreach (var Repo in CRs) {
				int Current = Repo.GetLeastCost (cost, chunkID);
				if (Current < cost) {
					cost = Current;
					repository = Repo;
				}
			}
			if (repository == null) {
				Console.Error.WriteLine ("BD2.Chunk.ChunkRepositoryCollection.GetLeastCost(): failed at first attempt.going fail-safe...");
				cost = int.MaxValue;
				foreach (var Repo in CRs) {
					int Current = Repo.GetLeastCost (cost, chunkID);
					if (Current < cost) {
						cost = Current;
						repository = Repo;
					}
				}
			}
		}

		public override IEnumerable<byte[]> Enumerate ()
		{
			SortedSet<byte[]> temp = null;
			foreach (ChunkRepository CR in GetRepositories()) {
				IEnumerable<byte[]> TEnumerate = CR.Enumerate ();
				SortedSet<byte[]> Chunks;
				if (TEnumerate is SortedSet<byte[]>)
					Chunks = (SortedSet<byte[]>)TEnumerate;
				else
					Chunks = new SortedSet<byte[]> (TEnumerate, BD2.Common.ByteSequenceComparer.Shared);
				if (temp != null)
					temp.UnionWith (Chunks);
				else {
					temp = Chunks;
				}
			}
			return temp;
		}

		public override IEnumerable<KeyValuePair<byte[], byte[]>> EnumerateData ()
		{
			SortedSet<byte[]> temp = null;
			foreach (ChunkRepository CR in GetRepositories())
				foreach (var tup in CR.EnumerateData ())
					if (!temp.Contains (tup.Key)) {
						temp.Add (tup.Key);
						yield return tup;
					}
		}

		public override int GetMaxCostForAny ()
		{
			int MaxCost = int.MaxValue;
			foreach (ChunkRepository repo in GetRepositories ())
				MaxCost = Math.Min (MaxCost, repo.GetMaxCostForAny ());
			return MaxCost;
		}

		public override void Pull (byte[] chunkID, out byte[] data, out byte[] segment, out byte[][] dependencies)
		{
			int cost;
			ChunkRepository repo;
			GetLeastCost (chunkID, out cost, out repo);
			repo.Pull (chunkID, out data, out segment, out dependencies);
		}

		public override IEnumerable<byte[]> EnumerateTopLevels ()
		{
			SortedSet<byte[]> temp = null;
			foreach (ChunkRepository CR in GetRepositories()) {
				foreach (var tup in CR.EnumerateTopLevels ()) {
					if (!temp.Contains (tup)) {
						temp.Add (tup);
						yield return tup;
					}
				}
			}
		}

		public override IEnumerable<Tuple<byte[], byte[][]>> EnumerateDependencies ()
		{
			SortedSet<byte[]> temp = null;
			foreach (ChunkRepository CR in GetRepositories())
				foreach (var tup in CR.EnumerateDependencies ())
					if (!temp.Contains (tup.Item1)) {
						temp.Add (tup.Item1);
						yield return tup;
					}
		}

		public override IEnumerable<Tuple<byte[], byte[][]>> EnumerateTopLevelDependencies ()
		{
			SortedSet<byte[]> temp = null;
			foreach (ChunkRepository CR in GetRepositories())
				foreach (var tup in CR.EnumerateTopLevelDependencies ())
					if (!temp.Contains (tup.Item1)) {
						temp.Add (tup.Item1);
						yield return tup;
					}
		}

		Guid id = Guid.NewGuid ();

		public override Guid ID {
			get {
				return id;
			}
		}

		public override void PushRawProxyData (byte[] index, byte[] value)
		{
			if (index == null)
				throw new ArgumentNullException ("index");
			if (value == null)
				throw new ArgumentNullException ("value");
			foreach (ChunkRepository CR in repositories)
				CR.PushRawProxyData (index, value);
		}

		public override byte[] PullRawProxyData (byte[] index)
		{
			if (index == null)
				throw new ArgumentNullException ("index");
			foreach (var Repo in GetRepositories ()) {
				byte[] value = Repo.PullRawProxyData (index);
				if (value != null) {
					return value;
				}
			}
			return null;
		}

		public override IEnumerable<KeyValuePair<byte[], byte[]>> EnumerateRawProxyData ()
		{
			SortedSet<byte[]> temp = null;
			foreach (ChunkRepository CR in GetRepositories()) {
				foreach (var tup in CR.EnumerateRawProxyData ()) {
					if (!temp.Contains (tup.Key)) {
						temp.Add (tup.Key);
						yield return tup;
					}
				}
			}
		}

		public override void PushSignatures (byte[] chunkID, SortedDictionary<byte[], byte[]> value)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			if (value == null)
				throw new ArgumentNullException ("value");
			foreach (ChunkRepository CR in repositories)
				CR.PushSignatures (chunkID, value);
		}

		public override SortedDictionary<byte[], byte[]> PullSignatures (byte[] chunkID)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			foreach (var Repo in GetRepositories ()) {
				SortedDictionary<byte[], byte[]> value = Repo.PullSignatures (chunkID);
				if (value != null) {
					return value;
				}
			}
			return null;
		}

		public override void PushKey (byte[] keyID, byte[] value)
		{
			if (keyID == null)
				throw new ArgumentNullException ("keyID");
			if (value == null)
				throw new ArgumentNullException ("value");
			foreach (ChunkRepository CR in repositories)
				CR.PushKey (keyID, value);
		}

		public override byte[] PullKey (byte[] keyID)
		{
			if (keyID == null)
				throw new ArgumentNullException ("keyID");
			foreach (var Repo in GetRepositories ()) {
				byte[] value = Repo.PullKey (keyID);
				if (value != null) {
					return value;
				}
			}
			return null;
		}

		public override void PushPrivateKey (byte[] keyID, byte[] value)
		{
			if (keyID == null)
				throw new ArgumentNullException ("keyID");
			if (value == null)
				throw new ArgumentNullException ("value");
			foreach (ChunkRepository CR in repositories)
				CR.PushPrivateKey (keyID, value);
		}

		public override byte[] PullPrivateKey (byte[] keyID)
		{
			if (keyID == null)
				throw new ArgumentNullException ("keyID");
			foreach (var Repo in GetRepositories ()) {
				byte[] value = Repo.PullPrivateKey (keyID);
				if (value != null) {
					return value;
				}
			}
			return null;
		}

		#endregion
	}
}
