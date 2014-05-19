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
using System.IO;
using System.Collections.Generic;
using BD2.Chunk;

namespace BD2.Repo.Leveldb
{
	public class Repository : ChunkRepository
	{
		string path;
		const string dependenciesDirectoryName = "Dependencies";
		const string dataDirectoryName = "Data";
		const string topLevelsDirectoryName = "TopLevel";
		const string metaDirectoryName = "Meta";
		const string indexDirectoryName = "Index";
		LevelDB.DB ldependencies, ldata, ltopLevels, lmeta, lindex;

		LevelDB.DB OpenLevelDB (string directoryName)
		{
			if (directoryName == null)
				throw new ArgumentNullException ("directoryName");
			string dependenciesPath = path + Path.DirectorySeparatorChar + directoryName;
			LevelDB.Options opts = new LevelDB.Options ();
			opts.CreateIfMissing = true;
			return new LevelDB.DB (opts, dependenciesPath, System.Text.Encoding.Unicode);
		}

		public Repository (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			this.path = path;
			lmeta = OpenLevelDB (metaDirectoryName);
			ldata = OpenLevelDB (dataDirectoryName);
			ldependencies = OpenLevelDB (dependenciesDirectoryName);
			ltopLevels = OpenLevelDB (topLevelsDirectoryName);
			lindex = OpenLevelDB (indexDirectoryName);
			if (lmeta.GetRaw ("Guid") == null)
				lmeta.Put ("Guid", Guid.NewGuid ().ToByteArray ());
		}

		static byte[] DependenciesToArray (byte[][] dependencies)
		{
			if (dependencies == null)
				return null;
			int lenOfDependencies = 0;
			for (int n = 0; n != dependencies.Length; n++) {
				lenOfDependencies += dependencies [n].Length + sizeof(int);
			}
			byte[] metadata = new byte[sizeof(int) + lenOfDependencies];
			MemoryStream metastream = new MemoryStream (metadata, true);
			BinaryWriter metawriter = new BinaryWriter (metastream);
			metawriter.Write (dependencies.Length);
			for (int n = 0; n != dependencies.Length; n++) {
				metawriter.Write (dependencies [n].Length);
				metawriter.Write (dependencies [n]);
			}
			return metadata;
		}

		static byte[][] ArrayToDependencies (byte[] array)
		{
			if (array == null)
				return null;
			MemoryStream metastream = new MemoryStream (array);
			BinaryReader metareader = new BinaryReader (metastream);
			int countOfDependencies = metareader.ReadInt32 ();
			byte[][] dependencies = new byte[countOfDependencies][];
			for (int n = 0; n != countOfDependencies; n++) {
				dependencies [n] = metareader.ReadBytes (metareader.ReadInt32 ());
			}
			return dependencies;
		}
		#region implemented abstract members of ChunkRepository
		public override void Push (byte[] chunkID, byte[] data, byte[][] dependencies)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			if (data == null)
				throw new ArgumentNullException ("data");
			if (dependencies == null)
				throw new ArgumentNullException ("dependencies");
			var dependenciesArray = DependenciesToArray (dependencies);
			ldependencies.Put (chunkID, dependenciesArray);
			ldata.Put (chunkID, data);
			ltopLevels.Put (chunkID, dependenciesArray);
			for (int n = 0; n != dependencies.Length; n++) {
				byte[] dependency = dependencies [n];
				if (dependency == null)
					throw new ArgumentNullException (string.Format ("dependency[{0}]", n), "dependency cannot be null");
				ltopLevels.Delete (dependency);
			}
			OnChunkPushEvent (chunkID);
		}

		public override byte[] PullData (byte[] chunkID)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			return ldata.GetRaw (chunkID);
		}

		public override byte[][] PullDependencies (byte[] chunkID)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			return ArrayToDependencies (ldependencies.GetRaw (chunkID));
		}

		public override void Pull (byte[] chunkID, out byte[] data, out byte[][] dependencies)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			data = ldata.GetRaw (chunkID);
			dependencies = ArrayToDependencies (ldependencies.GetRaw (chunkID));
		}

		public override IEnumerable<byte[]> Enumerate ()
		{
			IEnumerator<KeyValuePair <byte[],byte[]>> enumerator = ldependencies.GetRawEnumerator ();
			while (enumerator.MoveNext ())
				yield return enumerator.Current.Key;
		}

		public override IEnumerable<KeyValuePair<byte[], byte[]>> EnumerateData ()
		{
			IEnumerator<KeyValuePair <byte[],byte[]>> enumerator = ldependencies.GetRawEnumerator ();
			while (enumerator.MoveNext ())
				yield return enumerator.Current;
		}

		public override IEnumerable<byte[]> EnumerateTopLevels ()
		{
			IEnumerator<KeyValuePair <byte[],byte[]>> enumerator = ltopLevels.GetRawEnumerator ();
			while (enumerator.MoveNext ())
				yield return enumerator.Current.Key;
		}

		public override IEnumerable<Tuple<byte[], byte[][]>> EnumerateDependencies ()
		{
			IEnumerator<KeyValuePair <byte[],byte[]>> enumerator = ldependencies.GetRawEnumerator ();
			while (enumerator.MoveNext ())
				yield return new Tuple<byte[], byte[][]> (enumerator.Current.Key, ArrayToDependencies (enumerator.Current.Value));
		}

		public override IEnumerable<Tuple<byte[], byte[][]>> EnumerateTopLevelDependencies ()
		{
			IEnumerator<KeyValuePair <byte[],byte[]>> enumerator = ltopLevels.GetRawEnumerator ();
			while (enumerator.MoveNext ())
				yield return new Tuple<byte[], byte[][]> (enumerator.Current.Key, ArrayToDependencies (enumerator.Current.Value));
		}

		public override int GetLeastCost (int currentMinimum, byte[] chunkID)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			//leveldb doesn't support any kind of cost estimation.
			return 1024;
		}

		public override int GetMaxCostForAny ()
		{
			return 1024;
		}

		public override Guid ID {
			get {
				return new Guid (lmeta.GetRaw ("Guid"));
			}
		}
		#endregion
		#region implemented abstract members of ChunkRepository
		public override void PushIndex (byte[] index, byte[] value)
		{
			if (index == null)
				throw new ArgumentNullException ("index");
			if (value == null)
				throw new ArgumentNullException ("value");
			lindex.Put (index, value);
		}

		public override byte[] PullIndex (byte[] index)
		{
			if (index == null)
				throw new ArgumentNullException ("index");
			return lindex.GetRaw (index);
		}
		#endregion
	}
}
