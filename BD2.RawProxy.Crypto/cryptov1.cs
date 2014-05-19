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
using LevelDB;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace BD2.RawProxy.crypto
{
	[RawProxyAttribute(typeof(cryptov1), "1c36f5fc-e0f0-4836-8723-c4d2371c2018", "Deserialize")]
	public class cryptov1 : RawProxyv1
	{

		static System.Collections.Generic.SortedDictionary<Guid, X509Certificate2> certificates = new System.Collections.Generic.SortedDictionary<Guid, X509Certificate2> ();

		public static void ImportCertificate (Guid id, X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");
			lock (certificates) {
				certificates.Add (id, certificate);
			}
		}
		//todo:add procedure for retrieving the proxies
		#region implemented abstract members of RawProxyv1
		public override string Name {
			get {
				return "Crypto";
			}
		}

		public override Guid Type {
			get {
				return Guid.Parse ("0d1b1c2d-8eff-4167-99e5-3feccf7585e0");
			}
		}
		#endregion
		static DB storage;

		static cryptov1 ()
		{
			string StoragePath = BD2.Common.Configuration.GetConfig (null, "RawProxy.Crypto", "StorageConfiguration", new System.Type[0] { }, (A,C) => "Change this path.");
			storage = new DB (new Options () { Compression = CompressionType.SnappyCompression }, StoragePath, System.Text.Encoding.Unicode);
		}
		#region implemented abstract members of BD2.RawProxy.RawProxyv1
		public override byte[] Decode (byte[] Input)
		{
			if (Input == null)
				throw new ArgumentNullException ("Input");
			throw new NotImplementedException ();
		}

		public override byte[] Encode (byte[] Input)
		{
			return Encode (Input, DefaultEncoder);
		}

		public override byte[] Encode (byte[] Input, byte[] Attributes)
		{
			if (Input == null)
				throw new ArgumentNullException ("Input");
			if (Attributes == null)
				throw new ArgumentNullException ("Attributes");
			if (Input.Length == 0)
				throw new ArgumentException ("Input cannot be empty.", "Input");
			if (Attributes.Length == 0)
				throw new ArgumentException ("Attributes cannot be empty.", "Attributes");
			MemoryStream MS = new MemoryStream ();
			MS.Write (Input, 0, Input.Length);
			MS.Seek (0, SeekOrigin.Begin);
			System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create ();
			aes.Mode = System.Security.Cryptography.CipherMode.CBC;
			aes.Key = Attributes;
			//System.Security.Cryptography.CryptoStream cs = 
				new System.Security.Cryptography.CryptoStream (MS, aes.CreateEncryptor (), System.Security.Cryptography.CryptoStreamMode.Read);

			throw new NotImplementedException ();
		}

		byte[] defaultEncoder;

		public byte[] DefaultEncoder {
			get {
				if (defaultEncoder == null) {
					throw new InvalidOperationException ("Default Encoder is not set.");
				}
				return defaultEncoder;
			}
		}

		public void SetDefaultEncoder (byte[] hash)
		{
			if (hash == null)
				throw new ArgumentNullException ("hash");
			if (hash.Length == 0)
				throw new ArgumentException ("hash cannot be empty.", "Input");
			//X509Certificate2 cert = 
				GetCertificate (hash);//load certificate into memory
			defaultEncoder = hash;
		}
		#endregion

		public void AddCertificate (X509Certificate2 cert)
		{
			if (cert == null)
				throw new ArgumentNullException ("cert");
			storage.Put (cert.GetCertHash (),
			             cert.Export (X509ContentType.Pkcs12));
		}

		System.Collections.Generic.SortedDictionary<byte[], X509Certificate2> certs = new System.Collections.Generic.SortedDictionary<byte[], X509Certificate2> ();

		public X509Certificate2 GetCertificate (byte[] hash)
		{
			lock (certs) {
				X509Certificate2 ret;
				if (certs.TryGetValue (hash, out ret)) {
					return ret;
				}
				byte[] rawcert = GetRawCertificate (hash);
				try {
					ret = new X509Certificate2 (rawcert);
				} catch (Exception ex) {
					throw new InvalidDataException ("Certificate information is damaged/wrong and unusable.", ex);
				}
				certs.Add (hash, ret);
				return ret;
			}
		}

		public byte[] GetRawCertificate (byte[] hash)
		{
			if (hash == null)
				throw new ArgumentNullException ("hash");
			byte[] ret = storage.GetRaw (hash);
			if (ret == null)
				throw new ArgumentException ("No certificate associated with given hash", "hash");
			return ret;
		}

		#region implemented abstract members of RawProxyv1

		public override byte[] Serialize ()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}

