﻿/*
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
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.AccessControl;

namespace BD2.Core
{
	public sealed class RSAEncryptingKeyValueStorage : KeyValueStorage<byte[]>
	{
		readonly KeyValueStorage<byte[]> baseStorage;

		public KeyValueStorage<byte[]> BaseStorage {
			get {
				return baseStorage;
			}
		}

		RSAParameters rsap;
		RSACryptoServiceProvider rsacsp;

		public RSAEncryptingKeyValueStorage (KeyValueStorage<byte[]> baseStorage, RSAParameters rsap)
		{
			if (baseStorage == null)
				throw new ArgumentNullException ("baseStorage");
			this.baseStorage = baseStorage;
			this.rsap = rsap;
			Initialize ();
		}

		#region implemented abstract members of KeyValueStorage


		public override void Initialize ()
		{
			rsacsp = new RSACryptoServiceProvider ();
			rsacsp.ImportParameters (rsap);
		}

		public override void Dispose ()
		{
			rsacsp.Clear ();
		}

		byte[] Decrypt (byte[] value)
		{
			return rsacsp.Decrypt (value, true);
		}

		byte[] Encrypt (byte[] value)
		{
			return rsacsp.Encrypt (value, true);
		}

		public override System.Collections.Generic.IEnumerator<byte[]> EnumerateKeys ()
		{
			return baseStorage.EnumerateKeys ();
		}

		public override System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<byte[], byte[]>> GetEnumerator ()
		{
			foreach (var t in baseStorage) {
				yield return new System.Collections.Generic.KeyValuePair<byte[], byte[]> (t.Key, Decrypt (t.Value));
			}
		}

		public override void Put (byte[] key, byte[] value)
		{
			if (value == null)
				baseStorage.Put (key, null);
			else
				baseStorage.Put (key, Encrypt (value));
		}

		public override byte[] Get (byte[] key)
		{
			byte[] bytes = baseStorage.Get (key);
			if (bytes == null)
				return null;
			return Decrypt (bytes);
		}

		public override void Delete (byte[] key)
		{
			baseStorage.Delete (key);
		}

		#endregion


		#region implemented abstract members of KeyValueStorage

		public override System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<byte[], byte[]>> EnumerateFrom (byte[] start)
		{
			foreach (var t in baseStorage.EnumerateFrom (start)) {
				yield return new System.Collections.Generic.KeyValuePair<byte[], byte[]> (t.Key, Decrypt (t.Value));
			}
		}

		public override System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<byte[], byte[]>> EnumerateRange (byte[] start, byte[] end)
		{
			foreach (var t in baseStorage.EnumerateRange (start, end)) {
				yield return new System.Collections.Generic.KeyValuePair<byte[], byte[]> (t.Key, Decrypt (t.Value));
			}
		}

		public override int Count {
			get {
				return baseStorage.Count;
			}
		}

		#endregion
	}
}

