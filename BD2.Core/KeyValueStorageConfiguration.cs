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

namespace BD2.Core
{
	[Serializable]
	public class KeyValueStorageConfiguration
	{
		readonly static SortedDictionary<string, Func<DatabasePath, string, Type, object>> fns = new  SortedDictionary<string, Func<DatabasePath, string, Type, object>> ();

		static KeyValueStorageConfiguration ()
		{
			fns.Add ("BPlus", (Path, Name, OutType) => {
				BPlusKeyValueStorage bpkvs = new BPlusKeyValueStorage (Path, Name);
				if (OutType == typeof(byte[]))
					return bpkvs;
				if (OutType == typeof(byte[][]))
					return new Byte2EncodingKeyValueStorage (bpkvs);
				if (OutType == typeof(string))
					return new StringEncodingKeyValueStorage (bpkvs);
				throw new NotSupportedException ();
			});
			fns.Add ("LevelDB", (Path, Name, OutType) => {
				LevelDBKeyValueStorage lkvs = new LevelDBKeyValueStorage (Path);
				if (OutType == typeof(byte[]))
					return lkvs;
				if (OutType == typeof(byte[][]))
					return new Byte2EncodingKeyValueStorage (lkvs);
				if (OutType == typeof(string))
					return new StringEncodingKeyValueStorage (lkvs);
				throw new NotSupportedException ();
			});
		}

		public string Type;
		public string Path;

		public static void AddType<T> (string type, Func<DatabasePath, string, Type, KeyValueStorage<T>> fn)
		{
			lock (fns)
				fns.Add (type, fn);
		}

		public KeyValueStorage<T> OpenStorage<T> (DatabasePath path)
		{
			lock (fns)
				return (KeyValueStorage<T>)fns [Type].Invoke (path, Path, typeof(T));
		}

		public KeyValueStorageConfiguration ()
		{
		}

		public KeyValueStorageConfiguration (string path, string type)
		{
			this.Path = path;
			this.Type = type;
		}
	}
}

