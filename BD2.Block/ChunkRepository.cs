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
using System.Threading;

namespace BD2.Block
{
	public abstract class ChunkRepository
	{
		SortedSet<ChunkRepositoryCollection> chunkRepositoryCollections;

		internal void AddChunkRepositoryCollection (ChunkRepositoryCollection chunkRepositoryCollection)
		{
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
				throw new ArgumentNullException ("ChunkDescriptorCollections");
			chunkRepositoryCollections = new SortedSet<ChunkRepositoryCollection> ();
			chunkRepositoryCollections.UnionWith (chunkDescriptorCollections);
			foreach (ChunkRepositoryCollection CRC in chunkRepositoryCollections)
				CRC.AddRepository (this);
		}

		public abstract Guid ID { get; }

		public virtual void Push (IEnumerable<Tuple<byte[], byte[],  byte[][]>> chunks)
		{
			if (chunks == null)
				throw new ArgumentNullException ("Chunks");
			foreach (var chunk in chunks)
				Push (chunk.Item1, chunk.Item2, chunk.Item3);
		}

		public abstract void Push (byte[] chunkId, byte[] data, byte[][] dependencies);

		public abstract byte[] PullData (byte[] chunkID);

		public abstract byte[][] PullDependencies (byte[] chunkID);

		public virtual IEnumerator<Tuple<byte[], byte[], byte[][]>> Pull (IEnumerable<byte[]> chunkIDs)
		{
			foreach (byte[] chunkID in chunkIDs) {
				byte[] data;
				byte[][] dependencies;
				Pull (chunkID, out data, out dependencies);
				yield return new Tuple <byte[], byte[], byte[][]> (chunkID, data, dependencies); 
			}
		}

		public abstract void Pull (byte[] chunkID, out byte[] data, out byte[][] dependencies);

		public abstract IEnumerable<byte[]> Enumerate ();

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
			if (ChunkPushedEvent == null)
				return;
			ChunkPushedEvent (this, new ChunkPushedEventArgs (chunkID));
		}
	}
}
