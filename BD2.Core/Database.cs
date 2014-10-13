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
using BD2.Chunk;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Mono.Security.Cryptography;
using System.IO;
using BD2.Common;

namespace BD2.Core
{
	public sealed class Database
	{
		string name;
		SortedSet<Snapshot> snapshots = new SortedSet<Snapshot> ();
		ChunkRepositoryCollection backends;
		SortedSet<FrontendInstanceBase> frontendInstances = new SortedSet<FrontendInstanceBase> ();
		Snapshot primary;

		internal ChunkRepositoryCollection Backends {
			get {
				return backends;
			}
		}

		internal IEnumerable<FrontendInstanceBase> FrontendInstances {
			get {
				return new SortedSet<FrontendInstanceBase> (FrontendInstances);
			}
		}

		SortedDictionary<string, Frontend> frontends = new SortedDictionary<string, Frontend> ();

		public Frontend GetFrontend (string frontendName)
		{
			if (frontendName == null)
				throw new ArgumentNullException ("name");
			return frontends [frontendName];
		}

		byte[] segmentCriteria;

		public Database (IEnumerable<ChunkRepository> backends, IEnumerable<Frontend> frontends, byte[] segmentCriteria)
		{
			if (backends == null)
				throw new ArgumentNullException ("backends");
			if (frontends == null)
				throw new ArgumentNullException ("frontends");
			this.segmentCriteria = segmentCriteria;
			this.backends = new ChunkRepositoryCollection ();
			foreach (ChunkRepository CR in backends)
				this.backends.AddRepository (CR);
			primary = GetSnapshot ("Primary");
			foreach (Frontend Frontend in frontends) {
				this.frontends.Add (Frontend.Name, Frontend);
				Frontend.Database = this;
				this.frontendInstances.Add (Frontend.GetInstanse (primary));
			}
		}

		public SortedDictionary<byte[], string> GetUsers ()
		{
			return backends.GetUsers ();
		}

		SortedDictionary<byte[], RSAParameters> loggedInUsers = new SortedDictionary<byte[], RSAParameters> ();

		public bool UpdatePassword (byte[] userID, string password, string newPassword, string pepper)
		{
			byte[] privateKeyRaw = Backends.PullPrivateKey (userID);

			{//verify password
				System.IO.MemoryStream memoryStream = new System.IO.MemoryStream (privateKeyRaw);
				Stream cryptoStream = CreateCryptoStream (memoryStream, userID, password, pepper, CryptoStreamMode.Read);
				RSAParameters privateKey = DeserializeKey (cryptoStream);
				RSAParameters pubKey = GetPublicKey (userID);
				if ((ByteSequenceComparer.Shared.Compare (pubKey.Modulus, privateKey.Modulus) != 0) || (ByteSequenceComparer.Shared.Compare (pubKey.Exponent, privateKey.Exponent) != 0))
					return false;

			}

			MemoryStream newKeyStream = new MemoryStream ();
			Rijndael oldPassRij = CreateRijndael (userID, password, pepper);
			Rijndael newPassRij = CreateRijndael (userID, newPassword, pepper);
			System.Security.Cryptography.CryptoStream streamI = new CryptoStream (new System.IO.MemoryStream (privateKeyRaw), oldPassRij.CreateDecryptor (), CryptoStreamMode.Read);
			System.Security.Cryptography.CryptoStream streamO = new CryptoStream (newKeyStream, newPassRij.CreateEncryptor (), CryptoStreamMode.Write);

			var buffer = new byte[1024];
			var read = streamI.Read (buffer, 0, buffer.Length);
			while (read > 0) {
				streamO.Write (buffer, 0, read);
				read = streamI.Read (buffer, 0, buffer.Length);
			}
			streamO.FlushFinalBlock ();

			byte[] newKeyRaw = newKeyStream.ToArray ();
			Backends.PushPrivateKey (userID, newKeyRaw);
			return true;
		}

		static Rijndael CreateRijndael (byte[] userID, string password, string pepper)
		{
			string passpepper = password + pepper;
			Rijndael Rij = Rijndael.Create ();
			Rij.Padding = System.Security.Cryptography.PaddingMode.ISO10126;
			Rij.Mode = CipherMode.CBC;
			Rfc2898DeriveBytes aesKey = new Rfc2898DeriveBytes (passpepper, userID);
			Rij.Key = aesKey.GetBytes (Rij.KeySize / 8);
			Rij.IV = aesKey.GetBytes (Rij.BlockSize / 8);
			return Rij;
		}

		static CryptoStream CreateCryptoStream (Stream stream, byte[] userID, string password, string pepper, CryptoStreamMode cryptoStreamMode)
		{
			var Rij = CreateRijndael (userID, password, pepper);
			ICryptoTransform RijTrans = Rij.CreateDecryptor ();
			System.Security.Cryptography.CryptoStream cstream = new CryptoStream (stream, RijTrans, cryptoStreamMode);
			return cstream;
		}

		static RSAParameters DeserializeKey (Stream stream)
		{
			System.Xml.Serialization.XmlSerializer xmls = new System.Xml.Serialization.XmlSerializer (typeof(RSAParameters));
			RSAParameters key = (RSAParameters)xmls.Deserialize (stream);
			return key;
		}

		public RSAParameters GetPublicKey (byte[] userID)
		{
			byte[] pubKeyRaw = backends.PullKey (userID);
			System.IO.MemoryStream memoryStream = new System.IO.MemoryStream (pubKeyRaw);
			var pubKey = DeserializeKey (memoryStream);
			return pubKey;
		}

		public RSAParameters GetPrivateKey (byte[] userID, string password, string pepper)
		{
			byte[] privateKeyRaw = backends.PullPrivateKey (userID);
			System.IO.MemoryStream memoryStream = new System.IO.MemoryStream (privateKeyRaw);
			Stream cryptoStream = CreateCryptoStream (memoryStream, userID, password, pepper, CryptoStreamMode.Read);
			RSAParameters privateKey = DeserializeKey (cryptoStream);
			return privateKey;

		}

		public bool Login (byte[] userID, string password, string pepper)
		{
			RSAParameters pubKey = GetPublicKey (userID);
			try {
				RSAParameters privateKey = GetPrivateKey (userID, password, pepper);
				loggedInUsers.Add (userID, privateKey);
				if ((ByteSequenceComparer.Shared.Compare (pubKey.Modulus, privateKey.Modulus) != 0) || (ByteSequenceComparer.Shared.Compare (pubKey.Exponent, privateKey.Exponent) != 0))
					return false;
			} catch {
				return false;
			}
			return true;

		}

		public void SaveSnapshots (IEnumerable<Snapshot> snaps)
		{
			SortedDictionary<Snapshot, SortedSet<BaseDataObject>> bdos = new SortedDictionary<Snapshot, SortedSet<BaseDataObject>> ();
			foreach (Snapshot snapshot in snaps) {
				bdos.Add (snapshot, snapshot.GetAndClearVolatileData ());
			}
			System.IO.MemoryStream MS = new System.IO.MemoryStream ();
			System.IO.MemoryStream MSRP = new System.IO.MemoryStream ();
			System.IO.BinaryWriter MSBW = new System.IO.BinaryWriter (MS);
			SortedSet<byte[]> dependencies = new SortedSet<byte[]> (BD2.Common.ByteSequenceComparer.Shared);

			ChunkHeaderv1 ch = new ChunkHeaderv1 (DateTime.UtcNow, "");
			MSBW.Write (ch.Version);	
			byte[] chbytes = ch.Serialize ();
			MSBW.Write (chbytes.Length);
			MSBW.Write (chbytes);
			MSBW.Write (bdos.Count);
			Console.WriteLine ("{0} sections", bdos.Count);
			int n = 0;
			foreach (var tup in bdos) {
				MSBW.Write (1);
				Console.WriteLine ("Section version is {0}", 1);
				RawProxy.RawProxyCollection rpc = tup.Key.GetRawProxies ();
				MSBW.Write (rpc.Count);
				Console.WriteLine ("Raw proxies are {0}", rpc.Count);
				MSBW.Write (rpc.Serialize ());
				{
					System.IO.MemoryStream MSC = new System.IO.MemoryStream ();
					System.IO.BinaryWriter BWC = new System.IO.BinaryWriter (MSC);
					BWC.Write (tup.Value.Count);
					Console.WriteLine ("{0} objects", tup.Value.Count);
					MSRP.Write (MSC.ToArray (), 0, 4);
				}
				System.Collections.Generic.SortedDictionary <BaseDataObject, LinkedListNode<BaseDataObject>> ss = new  System.Collections.Generic.SortedDictionary <BaseDataObject, LinkedListNode<BaseDataObject>> ();
				System.Collections.Generic.LinkedList <BaseDataObject> ll = new LinkedList<BaseDataObject> ();
				foreach (BaseDataObject bdo in tup.Value) {
					if (!ss.ContainsKey (bdo))
						ss.Add (bdo, ll.AddLast (bdo));

					foreach (BaseDataObject dependency in bdo.GetDependenies ()) {
						byte[] dep = dependency.ChunkID;
						if (dep == null) {
							if (ss.ContainsKey (bdo)) {

							} else {

								ss.Add (dependency, ll.AddBefore (ss [bdo], dependency));

							}
						} else {
							if (!dependencies.Contains (dependency.ChunkID))
								dependencies.Add (dependency.ChunkID);
						}
					}
				}
				foreach (BaseDataObject bdo in ll) {
					n++;
					System.IO.MemoryStream MST = new System.IO.MemoryStream ();
					MST.Write (bdo.ObjectType.ToByteArray (), 0, 16);
					bdo.Serialize (MST);
					byte[] bytes = MST.ToArray ();
					//Console.WriteLine ("Object of type {0} serialized to {1} bytes.", bdo.GetType ().FullName, bytes.Length);
					{
						System.IO.MemoryStream MSC = new System.IO.MemoryStream ();
						System.IO.BinaryWriter BWC = new System.IO.BinaryWriter (MSC);
						BWC.Write (bytes.Length);
						MSRP.Write (MSC.ToArray (), 0, 4);
					}

					MSRP.Write (bytes, 0, bytes.Length);
				}
				byte[] encoded = rpc.ChainEncode (MSRP.ToArray ());
				Console.WriteLine ("{0} objects encoded in {1} bytes", tup.Value.Count, encoded.Length);
				MSBW.Write (encoded.Length);
				MSBW.Write (encoded);
			}
			if (n == 0) {
				Console.WriteLine ("No objects to save, nothing to do");
				return;
			}
			System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create ();
			Console.WriteLine ("{0} dependencies", dependencies.Count);
			List<byte[]> deps = new List<byte[]> (dependencies);
			Console.WriteLine ("Writing {0} bytes representing {1} objects to backend", MS.Length, n);
			byte[] buf = MS.ToArray ();
			byte[] chunkID = sha.ComputeHash (buf);
			SortedDictionary<byte[], byte[]> sigs = new SortedDictionary<byte[], byte[]> ();
			foreach (var tup in loggedInUsers) {
				RSA rsaa = RSA.Create ();
				rsaa.ImportParameters (tup.Value);
				byte[] signature = PKCS1.Sign_v15 (rsaa, System.Security.Cryptography.SHA256.Create (), chunkID);
				sigs.Add (tup.Key, signature);
			}
			Backends.Push (chunkID, buf, new byte[]{ }, deps.ToArray ());
			Backends.PushSignatures (chunkID, sigs);
			foreach (var tup in bdos) {
				foreach (var bdo in tup.Value) {
					bdo.SetChunkID (chunkID);
				}
			}
		}

		public void SaveAllSnapshots ()
		{
			SaveSnapshots (snapshots);
		}

		public void Load ()
		{
			SortedSet<byte[]> pendingData = new SortedSet<byte[]> (backends.Enumerate (), BD2.Common.ByteSequenceComparer.Shared);
			RawProxy.RawProxyCollection rpc = new BD2.RawProxy.RawProxyCollection ();
			foreach (var rp in backends.EnumerateRawProxyData()) {
				rpc.Add (RawProxy.RawProxyAttribute.DeserializeFromRawData (rp.Value));
			}
			while (pendingData.Count != 0)
				foreach (var tup in new SortedSet<byte[]>(pendingData, BD2.Common.ByteSequenceComparer.Shared)) {
					//Console.WriteLine ("Testing object");
					byte[] nchunk = tup;
					byte[][] deps = backends.PullDependencies (nchunk);
					Console.WriteLine ("dependency count = {0}", deps.Length);
					if (deps == null)
						continue;
					foreach (byte[] dep in deps) {
						if (pendingData.Contains (dep)) {
							Console.WriteLine ("Found an unloaded dependency, skipping...");
							nchunk = null;
						}
					}
					if (nchunk == null)
						continue;
					Console.WriteLine ("All the dependencies are loaded, proceeding...");
					byte[] chunkSegment = backends.PullSegment (nchunk);
					int mb = Math.Min (chunkSegment.Length, segmentCriteria.Length);
					bool skip = false;
					for (int n = 0; n != mb; n++) {
						if ((segmentCriteria [n] & chunkSegment [n]) != segmentCriteria [n])
							skip = true;
					}
					if (!skip) { //this is a really bad idea, I KNOW
						byte[] chunkData = backends.PullData (nchunk);
						LoadChunk (nchunk, chunkData, rpc);
					}
					pendingData.Remove (nchunk);
				}	
		}

		SortedDictionary<byte[], ChunkHeaderv1> chunkHeaders = new SortedDictionary<byte[], ChunkHeaderv1> (BD2.Common.ByteSequenceComparer.Shared);

		public ChunkHeaderv1 GetHeadersForChunk (byte[] id)
		{
			return chunkHeaders [id];
		}

		void LoadChunk (byte[] chunkID, byte[] chunkData, RawProxy.RawProxyCollection rpc)
		{
			Console.WriteLine ("Pulled a chunk with a size of {0}", chunkData.Length);
			System.IO.MemoryStream MS = new System.IO.MemoryStream (chunkData);
			System.IO.BinaryReader BR = new System.IO.BinaryReader (MS);
			int chunkVersion = BR.ReadInt32 ();
			if (chunkVersion == 1) {
				ChunkHeaderv1 ch = ChunkHeaderv1.Deserialzie (BR.ReadBytes (BR.ReadInt32 ())); 
				chunkHeaders.Add (chunkID, ch);
			} else
				throw new Exception (string.Format ("Chunk meta data version is 0x{0:x} which is not supported by this version of BD2.", chunkVersion));
			int sectionCount = BR.ReadInt32 ();
			for (int sectionID = 0; sectionID != sectionCount; sectionID++) {
				switch (chunkVersion) {
				case 1:
					int SectionVersion = BR.ReadInt32 ();
					switch (SectionVersion) {
					case 1:
						int proxyCount = BR.ReadInt32 ();
						Console.WriteLine ("Payload is encoded with {0} proxies.", proxyCount);
						System.Collections.Generic.List<byte[]> proxies = new List<byte[]> ();
						for (int n = 0; n != proxyCount; n++) {
							proxies.Add (BR.ReadBytes (20));
						}
						int payloadLength = BR.ReadInt32 ();
						byte[] payload = BR.ReadBytes (payloadLength);
						Console.WriteLine ("The payload[{0}] is {1} bytes.", sectionID, payloadLength);
						byte[] decodedPayload = rpc.ChainDecode (payload, proxies.ToArray ());
						Console.WriteLine ("Trying to deserialize in {0} frontends.", frontendInstances.Count);
						foreach (FrontendInstanceBase fib in frontendInstances)
							fib.CreateObjects (chunkID, decodedPayload);
						break;
					default:
						throw new Exception ("This version of BD2 does not support the version of data provided.");						
					}
					break;
				default:
					throw new Exception ("This version of BD2 does not support the version of data provided.");
				}
			}

		}

		public Database (IEnumerable<ChunkRepository> backends, IEnumerable<Frontend> frontends, byte[] segmentCriteria, string name)
			: this (backends, frontends, segmentCriteria)
		{
			this.name = name;
		}

		public string Name {
			get {
				return name;
			}
		}

		public SortedSet<Snapshot> GetSnapshots ()
		{
			lock (snapshots) {
				return new SortedSet<Snapshot> (snapshots);
			}
		}

		public Snapshot GetSnapshot (string snapshotName)
		{
			if (snapshotName == null)
				throw new ArgumentNullException ("snapshotName");
			lock (snapshots) {
				foreach (Snapshot ss in snapshots) {
					if (ss.Name == snapshotName) {
						return ss;
					}
				}
				Snapshot snap = new Snapshot (this, snapshotName, backends.Enumerate ());
				snapshots.Add (snap);
				return snap;
			}
		}

		public void Close ()
		{
			SaveAllSnapshots ();
			foreach (var fib in frontendInstances) {
				//fib.Close ();
			}
			foreach (var s in snapshots) {
				//s.Close ();
			}
			foreach (var f in frontends) {
				//f.Value.Close ();
			}
			//backends.Close ();
		}
	}
}
