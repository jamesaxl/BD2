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
using System.CodeDom.Compiler;

namespace BD2.Repo.Leveldb
{
	public class Repository : ChunkRepository
	{


		string path;
		const string dependenciesDirectoryName = "Dependencies";
		const string dataDirectoryName = "Data";
		const string rawProxyDataDirectoryName = "RawProxyData";
		const string topLevelsDirectoryName = "TopLevel";
		const string metaDirectoryName = "Meta";
		const string indexDirectoryName = "Index";
		const string segmentDirectoryName = "Segment";
		const string keysDirectoryName = "Keys";
		const string privateKeysDirectoryName = "PrivateKeys";
		const string signaturesDirectoryName = "Signatures";
		const string usersDirectoryName = "Users";

		LevelDB.DB ldependencies, ldata, lrawProxyData, ltopLevels, lmeta, lindex, lsegment, lkeys, lprivateKeys, lsignatures, lusers;

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
			lrawProxyData = OpenLevelDB (rawProxyDataDirectoryName);
			ldependencies = OpenLevelDB (dependenciesDirectoryName);
			ltopLevels = OpenLevelDB (topLevelsDirectoryName);
			lindex = OpenLevelDB (indexDirectoryName);
			lsegment = OpenLevelDB (segmentDirectoryName);
			lkeys = OpenLevelDB (keysDirectoryName);
			lprivateKeys = OpenLevelDB (privateKeysDirectoryName);
			lsignatures = OpenLevelDB (signaturesDirectoryName);
			lusers = OpenLevelDB (usersDirectoryName);
			if (lmeta.GetRaw ("Guid") == null)
				lmeta.Put ("Guid", Guid.NewGuid ().ToByteArray ());
		}

		static byte[] ArrayCombine (byte[][] parts)
		{
			if (parts == null)
				return null;
			int partsLen = 0;
			for (int n = 0; n != parts.Length; n++) {
				partsLen += parts [n].Length + sizeof(int);
			}
			byte[] metadata = new byte[sizeof(int) + partsLen];
			MemoryStream metastream = new MemoryStream (metadata, true);
			BinaryWriter metawriter = new BinaryWriter (metastream);
			metawriter.Write (parts.Length);
			for (int n = 0; n != parts.Length; n++) {
				metawriter.Write (parts [n].Length);
				metawriter.Write (parts [n]);
			}
			return metadata;
		}

		static byte[][] ArrayExpand (byte[] array)
		{
			if (array == null)
				return null;
			MemoryStream metastream = new MemoryStream (array);
			BinaryReader metareader = new BinaryReader (metastream);
			int partCount = metareader.ReadInt32 ();
			byte[][] parts = new byte[partCount][];
			for (int n = 0; n != partCount; n++) {
				parts [n] = metareader.ReadBytes (metareader.ReadInt32 ());
			}
			return parts;
		}

		#region implemented abstract members of ChunkRepository

		public override void Push (byte[] chunkID, byte[] data, byte[] segment, byte[][] dependencies)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			if (data == null)
				throw new ArgumentNullException ("data");
			if (dependencies == null)
				throw new ArgumentNullException ("dependencies");
			var dependenciesArray = ArrayCombine (dependencies);
			ldependencies.Put (chunkID, dependenciesArray);
			lsegment.Put (chunkID, segment);
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
			return ArrayExpand (ldependencies.GetRaw (chunkID));
		}

		public override void Pull (byte[] chunkID, out byte[] data, out byte[] segment, out byte[][] dependencies)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			data = ldata.GetRaw (chunkID);
			segment = lsegment.GetRaw (chunkID);
			dependencies = ArrayExpand (ldependencies.GetRaw (chunkID));
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
				yield return new Tuple<byte[], byte[][]> (enumerator.Current.Key, ArrayExpand (enumerator.Current.Value));
		}

		public override IEnumerable<Tuple<byte[], byte[][]>> EnumerateTopLevelDependencies ()
		{
			IEnumerator<KeyValuePair <byte[],byte[]>> enumerator = ltopLevels.GetRawEnumerator ();
			while (enumerator.MoveNext ())
				yield return new Tuple<byte[], byte[][]> (enumerator.Current.Key, ArrayExpand (enumerator.Current.Value));
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

		public override void PushRawProxyData (byte[] index, byte[] value)
		{
			if (index == null)
				throw new ArgumentNullException ("index");
			if (value == null)
				throw new ArgumentNullException ("value");
			lrawProxyData.Put (index, value);
		}

		public override byte[] PullRawProxyData (byte[] index)
		{
			if (index == null)
				throw new ArgumentNullException ("index");
			return lrawProxyData.GetRaw (index);
		}

		public override IEnumerable<KeyValuePair<byte[], byte[]>> EnumerateRawProxyData ()
		{
			IEnumerator<KeyValuePair <byte[],byte[]>> enumerator = lrawProxyData.GetRawEnumerator ();
			while (enumerator.MoveNext ())
				yield return enumerator.Current;
		}

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

		public override void PushSegment (byte[] chunkID, byte[] value)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			if (value == null)
				throw new ArgumentNullException ("value");
			lsegment.Put (chunkID, value);
		}

		public override byte[] PullSegment (byte[] chunkID)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			return lsegment.GetRaw (chunkID);
		}

		public override void PushKey (byte[] keyID, byte[] value)
		{
			if (keyID == null)
				throw new ArgumentNullException ("keyID");
			if (value == null)
				throw new ArgumentNullException ("value");
			lkeys.Put (keyID, value);
		}

		public override byte[] PullKey (byte[] keyID)
		{
			if (keyID == null)
				throw new ArgumentNullException ("keyID");
			return lkeys.GetRaw (keyID);
		}

		public override void PushPrivateKey (byte[] keyID, byte[] value)
		{
			if (keyID == null)
				throw new ArgumentNullException ("keyID");
			if (value == null)
				throw new ArgumentNullException ("value");
			lprivateKeys.Put (keyID, value);
		}

		public override byte[] PullPrivateKey (byte[] keyID)
		{
			if (keyID == null)
				throw new ArgumentNullException ("keyID");
			return lprivateKeys.GetRaw (keyID);
		}

		public override void PushSignatures (byte[] chunkID, SortedDictionary<byte[], byte[]> sigList)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			if (sigList == null)
				throw new ArgumentNullException ("sigList");
			byte[][] kvs = new byte[sigList.Count * 2][];
			int i = 0;
			foreach (var kvp in sigList) {
				kvs [i++] = kvp.Key;
				kvs [i++] = kvp.Value;
			}
			lsignatures.Put (chunkID, ArrayCombine (kvs));
		}

		public override SortedDictionary<byte[], byte[]> PullSignatures (byte[] chunkID)
		{
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			byte[][] kvs = ArrayExpand (lsignatures.GetRaw (chunkID));
			SortedDictionary<byte[], byte[]> sigList = new SortedDictionary<byte[], byte[]> ();
			int count = kvs.Length / 2;
			int i = 0;
			for (int n = 0; n != count; n++) {
				sigList.Add (kvs [i++], kvs [i++]);
			}
			return sigList;
		}

		public override SortedDictionary<byte[], string> GetUsers ()
		{
			SortedDictionary<byte[], string> rv = new SortedDictionary<byte[], string> ();
			IEnumerator<KeyValuePair<byte[], byte[]>> E = lusers.GetRawEnumerator ();
			while (E.MoveNext ()) {
				rv.Add (E.Current.Key, System.Text.Encoding.Unicode.GetString (E.Current.Value));
			}
			return rv;
		}

		public override void AddUser (byte[] id, string name)
		{
			//We already know the database does this internally, I'm just puting it here because I've done the same in the function right above this one
			if (lusers.GetRaw (id) != null)
				throw new Exception ("The key already exists in the database.");
			lusers.Put (id, System.Text.Encoding.Unicode.GetBytes (name));
		}

		#endregion
	}
}
