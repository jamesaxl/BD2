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

namespace BD2.Core
{
	public sealed class AESEncryptingKeyValueStorage : KeyValueStorage<byte[]>
	{
		readonly KeyValueStorage<byte[]> baseStorage;

		public KeyValueStorage<byte[]> BaseStorage {
			get {
				return baseStorage;
			}
		}

		byte[] key;
		Aes aes;
		
		ICryptoTransform decryptor;
		ICryptoTransform encryptor;

		public AESEncryptingKeyValueStorage (KeyValueStorage<byte[]> baseStorage, byte[] key)
		{
			if (baseStorage == null)
				throw new ArgumentNullException ("baseStorage");
			if (key == null)
				throw new ArgumentNullException ("key");
			this.baseStorage = baseStorage;
			this.key = key;
			Initialize ();
		}

		#region implemented abstract members of KeyValueStorage

		public override IEnumerator<byte[]> EnumerateKeys ()
		{
			return baseStorage.EnumerateKeys ();
		}

		public override void Initialize ()
		{
			aes = Aes.Create ();
			aes.Key = key;
			decryptor = aes.CreateDecryptor ();
			encryptor = aes.CreateEncryptor ();
		}

		public override void Dispose ()
		{
			aes.Clear ();
		}

		byte[] Decrypt (byte[] value)
		{
			System.IO.MemoryStream input = new System.IO.MemoryStream (value, false);
			System.IO.MemoryStream output = new System.IO.MemoryStream (value.Length);
			CryptoStream dcs = new CryptoStream (input, decryptor, CryptoStreamMode.Read);
			var buffer = new byte[1024];
			int read;
			do {
				read = dcs.Read (buffer, 0, buffer.Length);
				output.Write (buffer, 0, read);
			} while (read > 0);
			return output.ToArray ();
		}

		byte[] Encrypt (byte[] value)
		{
			System.IO.MemoryStream input = new System.IO.MemoryStream (value, false);
			System.IO.MemoryStream output = new System.IO.MemoryStream (value.Length + 32);
			CryptoStream dcs = new CryptoStream (output, encryptor, CryptoStreamMode.Write);
			var buffer = new byte[1024];
			int read;
			do {
				read = input.Read (buffer, 0, buffer.Length);
				dcs.Write (buffer, 0, read);
			} while (read > 0);
			return output.ToArray ();
		}


		public override IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator ()
		{
			foreach (var t in baseStorage) {
				yield return new KeyValuePair<byte[], byte[]> (t.Key, Decrypt (t.Value));
			}
		}

		public override void Put (byte[] key, byte[] value)
		{
			baseStorage.Put (key, Encrypt (value));
		}

		public override byte[] Get (byte[] key)
		{
			return Decrypt (baseStorage.Get (key));
		}

		public override void Delete (byte[] key)
		{
			baseStorage.Delete (key);
		}

		#endregion

		#region implemented abstract members of KeyValueStorage

		public override IEnumerable<KeyValuePair<byte[], byte[]>> EnumerateFrom (byte[] start)
		{
			foreach (var t in baseStorage.EnumerateFrom (start)) {
				yield return new KeyValuePair<byte[], byte[]> (t.Key, Decrypt (t.Value));
			}
		}

		public override IEnumerable<KeyValuePair<byte[], byte[]>> EnumerateRange (byte[] start, byte[] end)
		{
			foreach (var t in baseStorage.EnumerateRange (start, end)) {
				yield return new KeyValuePair<byte[], byte[]> (t.Key, Decrypt (t.Value));
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
