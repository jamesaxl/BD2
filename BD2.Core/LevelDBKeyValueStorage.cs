// /*
//  * Copyright (c) 2014 Behrooz Amoozad
//  * All rights reserved.
//  *
//  * Redistribution and use in source and binary forms, with or without
//  * modification, are permitted provided that the following conditions are met:
//  *     * Redistributions of source code must retain the above copyright
//  *       notice, this list of conditions and the following disclaimer.
//  *     * Redistributions in binary form must reproduce the above copyright
//  *       notice, this list of conditions and the following disclaimer in the
//  *       documentation and/or other materials provided with the distribution.
//  *     * Neither the name of the bd2 nor the
//  *       names of its contributors may be used to endorse or promote products
//  *       derived from this software without specific prior written permission.
//  *
//  * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//  * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//  * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//  * DISCLAIMED. IN NO EVENT SHALL Behrooz Amoozad BE LIABLE FOR ANY
//  * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//  * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//  * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//  * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//  * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//  * */
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using BD2.Core;

namespace BD2.Core
{
	public sealed class LevelDBKeyValueStorage : KeyValueStorage<byte[]>
	{
		string path;
		LevelDB.DB db;

		public static LevelDB.DB OpenLevelDB (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			string dependenciesPath = path;
			LevelDB.Options opts = new LevelDB.Options ();
			opts.CreateIfMissing = true;
			return new LevelDB.DB (opts, dependenciesPath, System.Text.Encoding.Unicode);
		}

		public string Path {
			get {
				return path;
			}
		}

		public LevelDBKeyValueStorage (DatabasePath path)
			: this (path.Path)
		{
		}

		public LevelDBKeyValueStorage (string path)
		{
			this.path = path;
			db = OpenLevelDB (path);
		}

		#region implemented abstract members of KeyValueStorage

		public override IEnumerator<byte[]> EnumerateKeys ()
		{
			return db.EnumerateRawKeys ();
		}

		public override void Initialize ()
		{
			db = OpenLevelDB (Path);
		}

		public override void Dispose ()
		{
			db.Dispose ();
		}

		public override IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator ()
		{
			var en = db.GetRawEnumerator ();
			while (en.MoveNext ())
				yield return new KeyValuePair<byte[], byte[]> (en.Current.Key, en.Current.Value);
		}

		public override void Put (byte[] key, byte[] value)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			if (value == null)
				throw new ArgumentNullException ("value");
			db.Put (key, value as byte[]);
		}

		public override byte[] Get (byte[] key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			return db.GetRaw (key);
		}

		public override void Delete (byte[] key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			db.Delete (key);
		}

		#endregion

		#region implemented abstract members of KeyValueStorage

		public override IEnumerable<KeyValuePair<byte[], byte[]>> EnumerateFrom (byte[] start)
		{
			throw new NotSupportedException ();
		}

		public override IEnumerable<KeyValuePair<byte[], byte[]>> EnumerateRange (byte[] start, byte[] end)
		{
			throw new NotSupportedException ();
		}

		public override int Count {
			get {
				throw new NotSupportedException ();
			}
		}

		#endregion
	}
}

