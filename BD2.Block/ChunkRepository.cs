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

		internal void AddChunkRepositoryCollection (ChunkRepositoryCollection ChunkRepositoryCollection)
		{
			lock (chunkRepositoryCollections) {
				chunkRepositoryCollections.Add (ChunkRepositoryCollection);
				ChunkRepositoryCollection.AddRepository (this);
			}
		}
		//		SortedSet<byte[]> state = new SortedSet<byte[]> ();
		protected ChunkRepository ()
		{
			chunkRepositoryCollections = new SortedSet<ChunkRepositoryCollection> ();
		}

		protected ChunkRepository (IEnumerable<ChunkRepositoryCollection> ChunkDescriptorCollections)
		{
			if (ChunkDescriptorCollections == null)
				throw new ArgumentNullException ("ChunkDescriptorCollections");
			chunkRepositoryCollections = new SortedSet<ChunkRepositoryCollection> ();
			chunkRepositoryCollections.UnionWith (ChunkDescriptorCollections);
			foreach (ChunkRepositoryCollection CRC in chunkRepositoryCollections)
				CRC.AddRepository (this);
		}

		public abstract Guid ID { get; }

		public void Push (IEnumerable<KeyValuePair<byte[], byte[]>> Chunks)
		{
			if (Chunks == null)
				throw new ArgumentNullException ("Chunks");
			foreach (var CD in Chunks)
				Push (CD.Key, CD.Value);
		}

		public abstract void Push (byte[] ChunkID, byte[] Data);

		public abstract byte[] Pull (byte[] ChunkID);

		public abstract IEnumerator<KeyValuePair<byte[], byte[]>> Pull (IEnumerator<byte[]> ChunkID);

		public abstract IEnumerable<byte[]> Enumerate ();
		//public abstract SortedSet<byte[]> GetIndependentChunks ();
		/// <summary>
		/// Gets the least cost.
		/// </summary>
		/// <returns>
		/// The least cost.
		/// </returns>
		/// <param name='CurrentMinimum'>
		/// Current determined minimum.used to skip test and return <remarks>int</remarks>.MaxValue if it's obvious that repository cannot afford better cost.
		/// </param>
		/// <param name='ChunkDescriptor'>
		/// Chunk descriptor.
		/// </param>
		public abstract int GetLeastCost (int CurrentMinimum, byte[] ChunkDescriptor);

		public abstract int GetMaxCostForAny ();
		//public abstract byte[][] GetDependencies (byte[] ChunkID);
		//		public event ChunkInsertedEventHandler ChunkInserted;
		//		protected void OnChunkInserted (byte[] ChunkDescriptor)
		//		{
		//			if (ChunkDescriptor == null)
		//				throw new ArgumentNullException ("chunkDescriptor");
		//			if (ChunkInserted == null)
		//				return;
		//			ChunkInsertedEventArgs CIEA = new ChunkInsertedEventArgs (ChunkDescriptor);
		//			lock (chunkRepositoryCollections) {
		//				foreach (ChunkRepositoryCollection CRC in chunkRepositoryCollections) {
		//					CRC.HandleChunkInserted (this, CIEA);
		//				}
		//			}
		//			ChunkInserted (this, new ChunkInsertedEventArgs (ChunkDescriptor));
		//		}
		//Single pass, too much memory footprint for this work. can be redeced by a huge amount. requires optimization.
	}
}
