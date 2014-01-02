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
 * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
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

namespace BD2.RawProxy.crypto
{
	public class cryptov1 : BD2.RawProxy.RawProxyv1
	{
		DB storage;

		public cryptov1 (string StoragePath)
		{
			storage = new DB (new Options () { Compression = CompressionType.SnappyCompression }, StoragePath, System.Text.Encoding.Unicode);
		}
		#region implemented abstract members of BD2.RawProxy.RawProxyv1
		public override byte[] Decode (byte[] Input)
		{
			if (Input == null)
				throw new ArgumentNullException ("Input");

		}

		public override byte[] Encode (byte[] Input)
		{
			Encode (Input, DefaultEncoder);
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
			MS.Write (Input.Length);
			MS.Write (Input);

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
			System.Security.Cryptography.X509Certificates.X509Certificate2 cert = GetCertificate (hash);//load certificate into memory
			defaultEncoder = hash;
		}
		#endregion
		public void AddCertificate (System.Security.Cryptography.X509Certificates.X509Certificate2 cert)
		{
			if (cert == null)
				throw new ArgumentNullException ("cert");
			storage.Put (cert.GetCertHash (),
			             cert.Export (System.Security.Cryptography.X509Certificates.X509ContentType.Pkcs12));
		}

		System.Collections.Generic.SortedDictionary<byte[], System.Security.Cryptography.X509Certificates.X509Certificate2> certs = new System.Collections.Generic.SortedDictionary<byte[], System.Security.Cryptography.X509Certificates.X509Certificate2> ();

		public System.Security.Cryptography.X509Certificates.X509Certificate2 GetCertificate (byte[] hash)
		{
			lock (certs) {
				System.Security.Cryptography.X509Certificates.X509Certificate2 ret;
				if (certs.TryGetValue (hash, out ret)) {
					return ret;
				}
				byte[] rawcert = GetRawCertificate (hash);
				try {
					ret = new System.Security.Cryptography.X509Certificates.X509Certificate2 (rawcert);
				} catch (Exception ex) {
					throw new InvalidDataException ("Certificate information is damaged/wrong and unusable.");
				}
				certs.Add (hash, rawcert);
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
	}
}

