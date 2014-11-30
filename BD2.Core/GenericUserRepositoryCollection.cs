/*
 * Copyright (c) 2014 Behrooz Amoozad
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
using System.Security.Cryptography;
using System.Net.NetworkInformation;

namespace BD2.Core
{
	public sealed class GenericUserRepositoryCollection
	{
		KeyValueStorage<byte[]> eSymmetricKeys;
		KeyValueStorage<byte[]> eMeta;
		RSAEncryptingKeyValueStorage symmetricKeys;
		RSAEncryptingKeyValueStorage meta;

		public KeyValueStorage<byte[]> ESymmetricKeys {
			get {
				return eSymmetricKeys;
			}
		}

		public KeyValueStorage<byte[]> EMeta {
			get {
				return eMeta;
			}
		}

		public RSAEncryptingKeyValueStorage SymmetricKeys {
			get {
				return symmetricKeys;
			}
		}

		public RSAEncryptingKeyValueStorage Meta {
			get {
				return meta;
			}
		}

		public byte[] DefaultRepository{ get { return meta.Get (System.Text.Encoding.Unicode.GetBytes ("Default Repository")); } }

		public KeyValueStorage<byte[]> GetSymmetricKeysRawData ()
		{
			return eSymmetricKeys;
		}


		readonly DatabasePath path;
		readonly SortedDictionary<string, KeyValueStorage<byte[]>> stores;
		RSAParameters? rsa;

		public void SetRSAParameters (RSAParameters rsa)
		{
			//if (this.rsa == null) {
			this.rsa = rsa;
			symmetricKeys = new RSAEncryptingKeyValueStorage (eSymmetricKeys, rsa);
			meta = new RSAEncryptingKeyValueStorage (eMeta, rsa);
			//} else
			//	throw new InvalidOperationException ();
		}

		public GenericUserRepositoryCollection (DatabasePath path, RSAParameters? rsa)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			this.path = path;
			stores = new SortedDictionary<string, KeyValueStorage<byte[]>> ();
			eSymmetricKeys = new LevelDBKeyValueStorage<byte[]> (path.CreatePath ("Symmetric Keys"));
			eMeta = new LevelDBKeyValueStorage<byte[]> (path.CreatePath ("Meta"));
			if (rsa.HasValue)
				SetRSAParameters (rsa.Value);
		}

		public KeyValueStorage<byte[]> GetStore (string name)
		{
			if (stores.ContainsKey (name))
				return stores [name];
			KeyValueStorage<byte[]> store = new LevelDBKeyValueStorage<byte[]> (path.CreatePath (name));
			byte[] state = meta.Get (System.Text.Encoding.Unicode.GetBytes (string.Format ("Encrypted:{0}", name)));
			if (state [0] != 0)
				store = new RSAEncryptingKeyValueStorage (store, rsa.Value);
			stores.Add (name, store);
			return store;
		}

	}
}

