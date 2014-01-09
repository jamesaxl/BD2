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
 * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
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

namespace BD2.Block
{
	/// <summary>
	/// All methods af this class are guaranteed to be thread-safe.
	/// </summary>
	public sealed class ChunkRepositoryCollection : ChunkRepository
	{
		SortedSet<ChunkRepository> repositories = new SortedSet<ChunkRepository> ();
		//		Database database;
		//		internal ChunkRepositoryCollection (Database Database)
		//		{
		//			if (database == null)
		//				throw new ArgumentNullException ("database");
		//			database = Database;
		//		}
		internal void AddRepository (ChunkRepository repository)
		{
			if (repository == null)
				throw new ArgumentNullException ("repository");
			if (repositories.Contains (repository))
				throw new InvalidOperationException ("Cannot add repository to collection more than once.");
			repositories.Add (repository);
		}
		//		internal void HandleChunkInserted (object sender, ChunkInsertedEventArgs e)
		//		{
		//			SortedSet<ChunkRepository> repos = GetRepositories ();
		//			ChunkRepository Orig = (ChunkRepository)sender;
		//			repos.Remove (Orig);
		//			byte[] Data = Orig.Pull (e.ChunkDescriptor);
		//			foreach (ChunkRepository Repo in repos) {
		//				Repo.Push (e.ChunkDescriptor, Data);
		//			}
		//			ChunkData CD = ChunkData.GetData (new MemoryStream (Data), false);
		//			database.HandleChunkInserted (CD);
		//		}
		public SortedSet<ChunkRepository> GetRepositories ()
		{
			lock (repositories) {
				return new SortedSet<ChunkRepository> (repositories);
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
		//		internal byte[][] GetDependencies (byte[] ChunkDescriptor)
		//		{
		//			int Cost;
		//			ChunkRepository CR;
		//			GetLeastCost (ChunkDescriptor, out Cost, out CR);
		//			return CR.GetDependencies (ChunkDescriptor);
		//		}
		public override void Push (byte[] chunkID, byte[] data, byte[][] dependencies)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			if (data == null)
				throw new ArgumentNullException ("data");
			if (dependencies == null)
				throw new ArgumentNullException ("dependencies");
			foreach (ChunkRepository CR in repositories)
				CR.Push (chunkID, data, dependencies);
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
			//instant
			foreach (var Repo in CRs) {
				int Current = Repo.GetMaxCostForAny ();
				if (Current < cost) {
					cost = Current;
				}
			}
			//takes a while if object is not available locally or bad heuristics have deluded the first estimation process
			foreach (var Repo in CRs) {
				int Current = Repo.GetLeastCost (cost, chunkID);
				if (Current < cost) {
					cost = Current;
					repository = Repo;
				}
			}
			if (repository == null) {
				Console.Error.WriteLine ("BD2.Block.ChunkRepositoryCollection.GetLeastCost(): failed at first attempt.going fail-safe...");
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
				//TODO: if temp is initialized, check CR.Enumerate for being a sortedset instead of cloning it
				SortedSet<byte[]> Chunks = new SortedSet<byte[]> (CR.Enumerate ());
				if (temp != null)
					temp.UnionWith (Chunks);
				else {
					temp = Chunks;
				}
			}
			return temp;
		}
		//		internal SortedSet<byte[]> GetIndependentChunks ()
		//		{
		//			SortedSet<byte[]> cache = null;
		//			foreach (ChunkRepository CR in GetRepositories()) {
		//				SortedSet<byte[]> IndependentChunks = CR.GetIndependentChunks ();
		//				if (cache != null)
		//					cache.UnionWith (IndependentChunks);
		//				else {
		//					cache = IndependentChunks;
		//				}
		//			}
		//			return cache;
		//		}
		public override int GetMaxCostForAny ()
		{
			int MaxCost = int.MaxValue;
			foreach (ChunkRepository repo in GetRepositories ())
				MaxCost = Math.Min (MaxCost, repo.GetMaxCostForAny ());
			return MaxCost;
		}
		#region implemented abstract members of ChunkRepository
		public override void Pull (byte[] chunkID, out byte[] data, out byte[][] dependencies)
		{
			throw new NotImplementedException ();
		}

		public override IEnumerable<byte[]> EnumerateTopLevels ()
		{
			throw new NotImplementedException ();
		}

		public override IEnumerable<Tuple<byte[], byte[][]>> EnumerateDependencies ()
		{
			throw new NotImplementedException ();
		}

		public override IEnumerable<Tuple<byte[], byte[][]>> EnumerateTopLevelDependencies ()
		{
			throw new NotImplementedException ();
		}

		Guid id = Guid.NewGuid ();

		public override Guid ID {
			get {
				return id;
			}
		}
		#endregion
	}
}
