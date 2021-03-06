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

namespace BD2.RawProxy.crypto
{
	public class SelectiveCrypto : RawProxyv1
	{

		protected override byte[] DoSerialize ()
		{
			throw new NotImplementedException ();
		}

		System.Collections.Generic.SortedDictionary<Guid, byte[]> PublicKeys = new System.Collections.Generic.SortedDictionary<Guid, byte[]> ();
		//TODO:use tamper-safe arrays in future
		//byte[] privateKey;
		//TODO:use tamper-safe arrays in future
		//byte[] publicKey;
		//System.Security.Cryptography.Aes AES_PROVIDER;
		public byte[] CreateInitializationVector ()
		{
			throw new NotImplementedException ();

		}

		public void SetOwnKey (byte[] PublicKey, byte[] PrivateKey)
		{
			if (PublicKey == null)
				throw new ArgumentNullException ("PublicKey");
			if (PrivateKey == null)
				throw new ArgumentNullException ("PrivateKey");
//			publicKey = PublicKey;
//			privateKey = PrivateKey;
		}

		public void AddPublicKey (Guid ID, byte[] Bytes)
		{
			if (Bytes == null)
				throw new ArgumentNullException ("Bytes");
			lock (PublicKeys) {
				PublicKeys.Add (ID, Bytes);
			}
		}

		public void RequeryStorageForKeys ()
		{
			lock (PublicKeys) {
			
			}
		}

		public void SetAESStrength (int bits)
		{

		}

		private bool AsSignRequest = false;

		public void MakeAsSignRequest ()
		{
			if (AsSignRequest)
				throw new Exception ("Value already set");
			AsSignRequest = true;
		}

		public SelectiveCrypto ()
		{
		}

		public SelectiveCrypto (byte[] IV, KeyStorage keys)
		{
		}
		#region implemented abstract members of RawProxyv1
		public override byte[] Decode (byte[] Input)
		{
			throw new NotImplementedException ();
		}

		public override byte[] Encode (byte[] Input)
		{
			throw new NotImplementedException ();
		}

		public override byte[] Encode (byte[] Input, byte[] Attributes)
		{
			throw new NotImplementedException ();
		}

		public override string Name {
			get {
				throw new NotImplementedException ();
			}
		}

		public override Guid Type {
			get {
				throw new NotImplementedException ();
			}
		}
		#endregion
	}
}

