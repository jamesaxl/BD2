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
using BD2.Core;

namespace BD2.Core
{
	public sealed class ChunkRepository
	{
		public ChunkRepository (
			KeyValueStorage<byte[]> data,
			KeyValueStorage<byte[][]> dataDependencies,
			KeyValueStorage<byte[][]> dataTopLevels,
			KeyValueStorage<byte[]> meta,
			KeyValueStorage<byte[][]> metaDependencies,
			KeyValueStorage<byte[][]> metaTopLevels,
			KeyValueStorage<byte[][]> signatures,
			KeyValueStorage<byte[][]> chunkSymmetricKeys,
			KeyValueStorage<byte[]> index,
			IDictionary<byte[], KeyValueStorage<byte[]>> encryptedData
		)
		{
			this.lindex = index;
			this.ldata = data;
			this.ldataDependencies = dataDependencies;
			this.ldataTopLevels = dataTopLevels;
			this.lmeta = meta;
			this.lmetaDependencies = metaDependencies;
			this.lmetaTopLevels = metaTopLevels;
			this.lsignatures = signatures;
			this.lchunkSymmetricKeys = chunkSymmetricKeys;
			this.lencryptedData = new SortedDictionary<byte[], KeyValueStorage<byte[]>> (encryptedData);
		}

		SortedDictionary<byte[], KeyValueStorage<byte[]>> lencryptedData;

		public SortedDictionary<byte[], KeyValueStorage<byte[]>> LencryptedData {
			get {
				return lencryptedData;
			}
		}

		KeyValueStorage<byte[]> ldata;

		public KeyValueStorage<byte[]> Ldata {
			get {
				return ldata;
			}
		}

		KeyValueStorage<byte[][]> ldataTopLevels;

		public KeyValueStorage<byte[][]> LdataTopLevels {
			get {
				return ldataTopLevels;
			}
		}

		KeyValueStorage<byte[]> lmeta;

		public KeyValueStorage<byte[]> Lmeta {
			get {
				return lmeta;
			}
		}

		KeyValueStorage<byte[][]> lmetaDependencies;

		public KeyValueStorage<byte[][]> LmetaDepnedencies {
			get {
				return lmetaDependencies;
			}
		}

		KeyValueStorage<byte[][]> lmetaTopLevels;

		public KeyValueStorage<byte[][]> LmetaTopLevels {
			get {
				return lmetaTopLevels;
			}
		}


		KeyValueStorage<byte[]> lindex;

		public KeyValueStorage<byte[]> Lindex {
			get {
				return lindex;
			}
		}

		KeyValueStorage<byte[][]> ldataDependencies;

		public KeyValueStorage<byte[][]> LdataDependencies {
			get {
				return ldataDependencies;
			}
		}

		KeyValueStorage<byte[][]> lsignatures;

		public KeyValueStorage<byte[][]> Lsignatures {
			get {
				return lsignatures;
			}
		}

		KeyValueStorage<byte[][]> lchunkSymmetricKeys;

		public KeyValueStorage<byte[][]> LchunkSymmetricKeys {
			get {
				return lchunkSymmetricKeys;
			}
		}

		 
		public byte[] ID { get { return lmeta.Get (System.Text.Encoding.Unicode.GetBytes ("ID")); } }

		public byte[][] GetRepositoryDependencies ()
		{
			return lmeta.Get (System.Text.Encoding.Unicode.GetBytes ("Dependencies")).Expand ();
		}

		public IEnumerator<ChunkData> Pull (IEnumerable<byte[]> chunkIDs)
		{
			if (chunkIDs == null)
				throw new ArgumentNullException ("chunkIDs");
			foreach (byte[] chunkID in chunkIDs) {
				byte[] data;
				byte[][] dependencies;
				byte[][] signatures;
				data = ldata.Get (chunkID);
				dependencies = ldataDependencies.Get (chunkID);
				signatures = lsignatures.Get (chunkID);
				yield return new ChunkData (chunkID, data, dependencies, signatures, this); 
			}
		}

		public void Push (byte[] chunkID, byte[] data, byte[][] dependencies, byte[][] signatures)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			if (data == null)
				throw new ArgumentNullException ("data");
			if (dependencies == null)
				throw new ArgumentNullException ("dependencies");
			ldataDependencies.Put (chunkID, dependencies);
			ldata.Put (chunkID, data);
			ldataTopLevels.Put (chunkID, dependencies);
			lsignatures.Put (chunkID, signatures);
			for (int n = 0; n != signatures.Length; n++) {
				if (signatures [n] == null)
					throw new ArgumentNullException (string.Format ("signatures[{0}]", n), "signature cannot be null");
			}
			for (int n = 0; n != dependencies.Length; n++) {
				byte[] dependency = dependencies [n];
				if (dependency == null)
					throw new ArgumentNullException (string.Format ("dependencies[{0}]", n), "dependency cannot be null");
				ldataTopLevels.Delete (dependency);
			}
		}

	  
		public static SortedSet<byte[]> CreateSmallestDependencyCollection (ChunkRepository repo, SortedSet<byte[]> chunks, int extraPasses = 4)
		{
			SortedSet<byte[]> removables = new SortedSet<byte[]> (ByteSequenceComparer.Shared);
			SortedSet<byte[]> resolved = new SortedSet<byte[]> (chunks, ByteSequenceComparer.Shared);
			SortedSet<byte[]> results = new SortedSet<byte[]> (chunks, ByteSequenceComparer.Shared);
			foreach (byte[] chunk in chunks) {
				removables.UnionWith (repo.ldataDependencies.Get (chunk));
			}
			for (int n = 0; n != extraPasses; n++) {
				foreach (byte[] chunk in removables) {
					if (!resolved.Contains (chunk)) {
						removables.UnionWith (repo.ldataDependencies.Get (chunk));
						resolved.Add (chunk);
					}
				}
			}
			results.ExceptWith (removables);
			return results;
		}
	}
}