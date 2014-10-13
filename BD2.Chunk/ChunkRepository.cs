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
using System.Threading;

namespace BD2.Chunk
{
	public abstract class ChunkRepository
	{
		SortedSet<ChunkRepositoryCollection> chunkRepositoryCollections;

		internal void AddChunkRepositoryCollection (ChunkRepositoryCollection chunkRepositoryCollection)
		{
			if (chunkRepositoryCollection == null)
				throw new ArgumentNullException ("chunkRepositoryCollection");
			lock (chunkRepositoryCollections) {
				chunkRepositoryCollections.Add (chunkRepositoryCollection);
				chunkRepositoryCollection.AddRepository (this);
			}
		}
		//		SortedSet<byte[]> state = new SortedSet<byte[]> ();
		protected ChunkRepository ()
		{
			chunkRepositoryCollections = new SortedSet<ChunkRepositoryCollection> ();
		}

		protected ChunkRepository (IEnumerable<ChunkRepositoryCollection> chunkDescriptorCollections)
		{
			if (chunkDescriptorCollections == null)
				throw new ArgumentNullException ("chunkDescriptorCollections");
			chunkRepositoryCollections = new SortedSet<ChunkRepositoryCollection> ();
			chunkRepositoryCollections.UnionWith (chunkDescriptorCollections);
			foreach (ChunkRepositoryCollection CRC in chunkRepositoryCollections)
				CRC.AddRepository (this);
		}

		public abstract Guid ID { get; }

		public virtual void Push (IEnumerable<Tuple<byte[], byte[], byte[],  byte[][]>> chunks)
		{
			if (chunks == null)
				throw new ArgumentNullException ("chunks");
			foreach (var chunk in chunks)
				Push (chunk.Item1, chunk.Item2, chunk.Item3, chunk.Item4);
		}

		public abstract SortedDictionary<byte[], string> GetUsers ();

		public abstract void AddUser (byte[] id, string name);

		public abstract void PushIndex (byte[] index, byte[] value);

		public abstract void PushSegment (byte[] chunkID, byte[] value);

		public abstract void PushKey (byte[] keyID, byte[] value);

		public abstract void PushPrivateKey (byte[] keyID, byte[] value);

		public abstract void PushSignatures (byte[] chunkID, SortedDictionary<byte[], byte[]> sigList);

		public abstract byte[] PullIndex (byte[] index);

		public abstract void PushRawProxyData (byte[] index, byte[] value);

		public abstract byte[] PullRawProxyData (byte[] index);

		public abstract void Push (byte[] chunkID, byte[] data, byte[] segment, byte[][] dependencies);

		public abstract byte[] PullData (byte[] chunkID);

		public abstract byte[] PullSegment (byte[] chunkID);

		public abstract byte[] PullKey (byte[] keyID);

		public abstract byte[] PullPrivateKey (byte[] keyID);

		public abstract SortedDictionary<byte[], byte[]> PullSignatures (byte[] chunkID);

		public abstract byte[][] PullDependencies (byte[] chunkID);

		public virtual IEnumerator<Tuple<byte[], byte[], byte[], byte[][]>> Pull (IEnumerable<byte[]> chunkIDs)
		{
			if (chunkIDs == null)
				throw new ArgumentNullException ("chunkIDs");
			foreach (byte[] chunkID in chunkIDs) {
				byte[] data;
				byte[] section;
				byte[][] dependencies;
				Pull (chunkID, out data, out section, out dependencies);
				yield return new Tuple <byte[], byte[], byte[], byte[][]> (chunkID, data, section, dependencies); 
			}
		}

		public abstract void Pull (byte[] chunkID, out byte[] data, out byte[] segment, out byte[][] dependencies);

		public abstract IEnumerable<byte[]> Enumerate ();

		public abstract IEnumerable<KeyValuePair<byte[], byte[]>> EnumerateData ();

		public abstract IEnumerable<KeyValuePair<byte[], byte[]>> EnumerateRawProxyData ();

		public abstract IEnumerable<byte[]> EnumerateTopLevels ();

		public abstract IEnumerable<Tuple<byte[], byte[][]>> EnumerateDependencies ();

		public abstract IEnumerable<Tuple<byte[], byte[][]>> EnumerateTopLevelDependencies ();

		/// <summary>
		/// Gets the least cost.
		/// </summary>
		/// <returns>
		/// The least cost.
		/// </returns>
		/// <param name='CurrentMinimum'>
		/// Current determined minimum.used to skip test and return <remarks>int</remarks>.MaxValue if it's obvious that repository cannot provide better cost.
		/// </param>
		/// <param name='ChunkDescriptor'>
		/// Chunk descriptor.
		/// </param>
		public abstract int GetLeastCost (int currentMinimum, byte[] chunkID);

		public abstract int GetMaxCostForAny ();

		public event ChunkPushedEventHandler ChunkPushedEvent;

		protected void OnChunkPushEvent (byte[] chunkID)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			if (ChunkPushedEvent == null)
				return;
			ChunkPushedEvent (this, new ChunkPushedEventArgs (chunkID));
		}

		public static SortedSet<byte[]> CreateSmallestDependencyCollection (ChunkRepository repo, SortedSet<byte[]> chunks, int extraPasses = 4)
		{
			SortedSet<byte[]> removables = new SortedSet<byte[]> (BD2.Common.ByteSequenceComparer.Shared);
			SortedSet<byte[]> resolved = new SortedSet<byte[]> (chunks, BD2.Common.ByteSequenceComparer.Shared);
			SortedSet<byte[]> results = new SortedSet<byte[]> (chunks, BD2.Common.ByteSequenceComparer.Shared);
			foreach (byte[] chunk in chunks) {
				removables.UnionWith (repo.PullDependencies (chunk));
			}
			for (int n = 0; n != extraPasses; n++) {
				foreach (byte[] chunk in removables) {
					if (!resolved.Contains (chunk)) {
						removables.UnionWith (repo.PullDependencies (chunk));
						resolved.Add (chunk);
					}
				}
			}
			results.ExceptWith (removables);
			return results;
		}
	}
}