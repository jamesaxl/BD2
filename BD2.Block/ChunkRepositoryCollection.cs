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
	public sealed class ChunkRepositoryCollection
	{
		SortedSet<ChunkRepository> repositories = new SortedSet<ChunkRepository> ();
		//		Database database;
		//		internal ChunkRepositoryCollection (Database Database)
		//		{
		//			if (database == null)
		//				throw new ArgumentNullException ("database");
		//			database = Database;
		//		}
		internal void AddRepository (ChunkRepository Repository)
		{
			if (Repository == null)
				throw new ArgumentNullException ("Repository");
			if (repositories.Contains (Repository))
				throw new InvalidOperationException ("Cannot add repository to collection more than once.");
			repositories.Add (Repository);
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

		internal byte[] Pull (byte[] Descriptor)
		{
			foreach (var Repo in GetRepositories ()) {
				byte[] CD = Repo.Pull (Descriptor);
				if (CD != null) {
					return CD;
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
		internal void Push (byte[] Hash, byte[] Data)
		{
			if (Hash == null)
				throw new ArgumentNullException ("Hash");
			if (Data == null)
				throw new ArgumentNullException ("Data");
			foreach (ChunkRepository CR in repositories)
				CR.Push (Hash, Data);
		}

		internal void GetLeastCost (byte[] ChunkDescriptor, out int Cost, out ChunkRepository Repository)
		{
			if (ChunkDescriptor == null)
				throw new ArgumentNullException ("ChunkDescriptor");
			SortedSet<ChunkRepository> CRs = GetRepositories ();
			Cost = int.MaxValue;
			Repository = null;
			//instant
			foreach (var Repo in CRs) {
				int Current = Repo.GetMaxCostForAny ();
				if (Current < Cost) {
					Cost = Current;
				}
			}
			//takes a while if object is not available locally or bad heuristics have deluded the first estimation process
			foreach (var Repo in CRs) {
				int Current = Repo.GetLeastCost (Cost, ChunkDescriptor);
				if (Current < Cost) {
					Cost = Current;
					Repository = Repo;
				}
			}
			if (Repository == null) {
				Console.Error.WriteLine ("ChunkRepositoryCollection:GetLeastCost failed at first attempt.going fail-safe...");
				Cost = int.MaxValue;
				foreach (var Repo in CRs) {
					int Current = Repo.GetLeastCost (Cost, ChunkDescriptor);
					if (Current < Cost) {
						Cost = Current;
						Repository = Repo;
					}
				}
			}
		}

		internal MemoryStream GetRawData (byte[] ChunkID, int Offset, int Count = -1)
		{
			byte[] CD = Pull (ChunkID);
			if (CD != null) {
				return new MemoryStream (CD, Offset, Count, false, false);
			}
			return null;
		}
		//		internal void Sync (out int ObjectsMoved)
		//		{
		//			//TODO: Bring back PPE sepport from experimental tree.it WAS worth it.
		//			ObjectsMoved = 0;
		//			SortedDictionary<ChunkRepository, CDRSorterhelper> RemotePPE = new SortedDictionary<ChunkRepository, CDRSorterhelper> ();
		//			SortedSet<byte[]> RemoteTotalExcept = null;
		//			SortedSet<byte[]> ExeptUnion = new SortedSet<byte[]> ();
		//			object Lock_Populate = new object ();
		//			System.Threading.Tasks.Parallel.ForEach (repositories, (Remote) => {
		//				lock (Lock_Populate) {
		//					CDRSorterhelper CSH = new CDRSorterhelper (Remote);
		//					RemotePPE.Add (Remote, CSH);
		//					if (RemoteTotalExcept == null) {
		//						RemoteTotalExcept = new  SortedSet<byte[]> (CSH.OriginalDescriptors, ByteSequenceComparer.Shared);
		//					} else {
		//						RemoteTotalExcept.ExceptWith (CSH.OriginalDescriptors);
		//					}
		//				}
		//			});
		//			System.Threading.Tasks.Parallel.ForEach (RemotePPE, (CSHT) => {
		//				CDRSorterhelper CSH = CSHT.Value;
		//				RemoteTotalExcept.ExceptWith (CSH.OriginalDescriptors);
		//			});
		//			System.Threading.Tasks.Parallel.ForEach (RemotePPE, (CSHT) => {
		//				CDRSorterhelper CSH = CSHT.Value;
		//				CSH.ExceptedDescriptors = new SortedSet<byte[]> (CSH.OriginalDescriptors, CSH.OriginalDescriptors.Comparer);
		//				CSH.ExceptedDescriptors.ExceptWith (RemoteTotalExcept);
		//				ExeptUnion.UnionWith (CSH.ExceptedDescriptors);
		//			});
		//			System.Threading.Tasks.Parallel.ForEach (RemotePPE, (CSHT) => {
		//				CDRSorterhelper CSH = CSHT.Value;
		//				ExeptUnion.UnionWith (CSH.ExceptedDescriptors);
		//			});
		//			System.Threading.Tasks.Parallel.ForEach (RemotePPE, (CSHT) => {
		//				CDRSorterhelper CSH = CSHT.Value;
		//				CSH.MissingDescriptors = new SortedSet<byte[]> (ExeptUnion, CSH.OriginalDescriptors.Comparer);
		//				CSH.MissingDescriptors.ExceptWith (CSH.ExceptedDescriptors);
		//			});
		//			System.Threading.Tasks.Parallel.ForEach (RemotePPE, (CSHT) => {
		//				CDRSorterhelper CSH = CSHT.Value;
		//				foreach (byte[] Chunk in CSH.MissingDescriptors)
		//					CSH.Remote.Push (Chunk, Pull (Chunk));
		//			});
		//		}
		internal SortedSet<byte[]> Enumerate ()
		{
			SortedSet<byte[]> cache = null;
			foreach (ChunkRepository CR in GetRepositories()) {
				SortedSet<byte[]> Chunks = new SortedSet<byte[]> (CR.Enumerate ());
				if (cache != null)
					cache.UnionWith (Chunks);
				else {
					cache = Chunks;
				}
			}
			return cache;
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
		internal int GetLeastCost (int CurrentMinimum, byte[] ChunkDescriptor)
		{
			throw new NotImplementedException ();
//			int Min = int.MaxValue;
//			foreach (ChunkRepository CR in GetRepositories ()) {			
//			}
		}

		internal int GetMaxCostForAny ()
		{
			//I really want F# here
			int MaxCost = int.MaxValue;
			foreach (ChunkRepository repo in GetRepositories ())
				MaxCost = Math.Min (MaxCost, repo.GetMaxCostForAny ());
			return MaxCost;
		}
	}
}
