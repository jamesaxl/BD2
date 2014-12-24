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
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;
using BD2.Core;
using Mono.Security.Cryptography;
using System.IO.MemoryMappedFiles;
using System.Configuration;
using System.Security.AccessControl;
using System.CodeDom.Compiler;
using System.Xml.Serialization;
using CSharpTest.Net.Crypto;

namespace BD2.Core
{
	public sealed class UserRepository
	{
		readonly KeyValueStorage<string> userNames;
		readonly KeyValueStorage<byte[]> userParents;
		readonly KeyValueStorage<byte[]> userCerts;
		readonly KeyValueStorage<byte[]> userKeys;
		readonly KeyValueStorage<byte[]> userSigningCerts;
		readonly KeyValueStorage<byte[]> userSigningKeys;
		readonly KeyValueStorage<byte[][]> userRepositories;
		readonly KeyValueStorage<byte[]> repositories;
		readonly KeyValueStorage<byte[]> meta;
		//readonly KeyValueStorage<byte[]> permissions;
		readonly SortedDictionary<byte[], GenericUserRepositoryCollection> genericRepositories;
		readonly SortedDictionary<byte[], RSAParameters> loggedInUsers = new SortedDictionary<byte[], RSAParameters> ();

		public int LoggedInUsersCount{ get { return loggedInUsers.Count; } }

		public GenericUserRepositoryCollection GetUserRepository (byte[] userID)
		{
			return genericRepositories [userID];
		}

		public SortedSet<byte[]> LoggedInUsers {
			get {
				return new SortedSet<byte[]> (loggedInUsers.Keys);
			}
		}

		//readonly DatabasePath path;

		public UserRepository (DatabasePath path, UserRepositoryConfiguration config)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			if (config == null)
				throw new ArgumentNullException ("config");

			Console.WriteLine ("Reading KeyValueStores");
			userNames = config.Users.OpenStorage<string> (path);
			userParents = config.UserParents.OpenStorage<byte[]> (path);
			userCerts = config.UserCerts.OpenStorage<byte[]> (path);
			userKeys = config.UserKeys.OpenStorage<byte[]> (path);
			userSigningCerts = config.UserSigningCerts.OpenStorage<byte[]> (path);
			userSigningKeys = config.UserSigningKeys.OpenStorage<byte[]> (path);
			userRepositories = config.UserRepositores.OpenStorage<byte[][]> (path);
			repositories = config.Repositores.OpenStorage<byte[]> (path);
			meta = config.Meta.OpenStorage<byte[]> (path);
			//permissions = new LevelDBKeyValueStorage<byte[]> (path.CreatePath (config.PermissionsPath));
			genericRepositories = new SortedDictionary<byte[], GenericUserRepositoryCollection> (ByteSequenceComparer.Shared);
			loggedInUsers = new SortedDictionary<byte[], RSAParameters> (ByteSequenceComparer.Shared);
			Console.WriteLine ("Done");
			var e = userNames.GetEnumerator ();
			while (e.MoveNext ()) {
				var user = e.Current;
				Console.WriteLine ("user: {0}:{1}", user.Key, user.Value);
				genericRepositories.Add (user.Key, 
					new GenericUserRepositoryCollection (path.CreatePath (user.Key.ToHexadecimal ()),
						DeserializeKey (new MemoryStream (userCerts.Get (user.Key), false)))
				);
			}
		}

		public IEnumerable<KeyValuePair<byte[], string>> GetUsers ()
		{
			return userNames;
		}

		public bool VerifyPassword (byte[] userID, string password, string pepper)
		{
			if (userID == null)
				throw new ArgumentNullException ("userID");
			if (password == null)
				throw new ArgumentNullException ("password");
			if (pepper == null)
				throw new ArgumentNullException ("pepper");
			RSAParameters pubKey = GetPublicKey (userID);
			try {
				RSAParameters privateKey = GetPrivateKey (userID, password, pepper);
				if ((ByteSequenceComparer.Shared.Compare (pubKey.Modulus, privateKey.Modulus) != 0) || (ByteSequenceComparer.Shared.Compare (pubKey.Exponent, privateKey.Exponent) != 0))
					return false;
			} catch {
				return false;
			}
			return true;
		}

		public bool Login (byte[] userID, string password, string pepper)
		{
			if (userID == null)
				throw new ArgumentNullException ("userID");
			if (password == null)
				throw new ArgumentNullException ("password");
			if (pepper == null)
				throw new ArgumentNullException ("pepper");
			RSAParameters pubKey = GetPublicKey (userID);
			try {
				RSAParameters privateKey = GetPrivateKey (userID, password, pepper);
				if ((ByteSequenceComparer.Shared.Compare (pubKey.Modulus, privateKey.Modulus) != 0) || (ByteSequenceComparer.Shared.Compare (pubKey.Exponent, privateKey.Exponent) != 0))
					return false;
				loggedInUsers.Add (userID, privateKey);
	
			} catch {
				return false;
			}
			return true;

		}

		public IEnumerable<byte[]> GetUserRepositories (byte[] userID)
		{
			if (userID == null)
				throw new ArgumentNullException ("userID");
			return userRepositories.Get (userID);
		}

		public byte[] GetRawRepositoryInfo (byte[] repositoryID)
		{
			if (repositoryID == null)
				throw new ArgumentNullException ("repositoryID");
			return repositories.Get (repositoryID);
		}

		public byte[] GetMetaData (byte[] id)
		{
			if (id == null)
				throw new ArgumentNullException ("id");
			return meta.Get (id);
		}

		public bool UpdatePassword (byte[] userID, string password, string newPassword, string pepper)
		{
			if (userID == null)
				throw new ArgumentNullException ("userID");
			if (password == null)
				throw new ArgumentNullException ("password");
			if (newPassword == null)
				throw new ArgumentNullException ("newPassword");
			if (pepper == null)
				throw new ArgumentNullException ("pepper");
			byte[] privateKeyRaw = userKeys.Get (userID);

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
			userKeys.Put (userID, newKeyRaw);
			return true;
		}

		static Rijndael CreateRijndael (byte[] userID, string password, string pepper)
		{
			if (userID == null)
				throw new ArgumentNullException ("userID");
			if (password == null)
				throw new ArgumentNullException ("password");
			if (pepper == null)
				throw new ArgumentNullException ("pepper");
			string passpepper = password + pepper;
			Rijndael Rij = Rijndael.Create ();
			Rij.KeySize = 256;
			Rij.Padding = PaddingMode.ISO10126;
			Rij.Mode = CipherMode.CBC;
			Rfc2898DeriveBytes aesKey = new Rfc2898DeriveBytes (passpepper, userID, 65536);
			Rij.Key = aesKey.GetBytes (Rij.KeySize / 8);
			Rij.GenerateIV ();
			return Rij;
		}

		static CryptoStream CreateCryptoStream (Stream stream, byte[] userID, string password, string pepper, CryptoStreamMode cryptoStreamMode)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			if (userID == null)
				throw new ArgumentNullException ("userID");
			if (password == null)
				throw new ArgumentNullException ("password");
			if (pepper == null)
				throw new ArgumentNullException ("pepper");
			var Rij = CreateRijndael (userID, password, pepper);
			ICryptoTransform RijTrans = Rij.CreateDecryptor ();
			CryptoStream cstream = new CryptoStream (stream, RijTrans, cryptoStreamMode);
			return cstream;
		}

		static RSAParameters DeserializeKey (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			XmlRootAttribute xRoot = new XmlRootAttribute ();
			xRoot.ElementName = "RSAKeyValue";
			XmlSerializer xmls = new XmlSerializer (typeof(RSAParameters), xRoot);
			RSAParameters key = (RSAParameters)xmls.Deserialize (stream);
			return key;
		}

		public RSAParameters GetPublicKey (byte[] userID)
		{
			if (userID == null)
				throw new ArgumentNullException ("userID");
			byte[] pubKeyRaw = userCerts.Get (userID);
			MemoryStream memoryStream = new MemoryStream (pubKeyRaw);
			var pubKey = DeserializeKey (memoryStream);
			return pubKey;
		}

		public RSAParameters GetPrivateKey (byte[] userID, string password, string pepper)
		{
			if (userID == null)
				throw new ArgumentNullException ("userID");
			if (password == null)
				throw new ArgumentNullException ("password");
			if (pepper == null)
				throw new ArgumentNullException ("pepper");
			byte[] privateKeyRaw = userKeys.Get (userID);
			MemoryStream memoryStream = new MemoryStream (privateKeyRaw);
			//try deserializing without auth// means first login
			try {
				RSAParameters rsap = DeserializeKey (new MemoryStream (privateKeyRaw));
				try {
					MemoryStream newKeyStream = new MemoryStream ();
					Rijndael newPassRij = CreateRijndael (userID, password, pepper);
					MemoryStream streamI = new MemoryStream (privateKeyRaw);
					CryptoStream streamO = new CryptoStream (newKeyStream, newPassRij.CreateEncryptor (), CryptoStreamMode.Write);
					var buffer = new byte[1024];
					var read = streamI.Read (buffer, 0, buffer.Length);
					while (read > 0) {
						streamO.Write (buffer, 0, read);
						read = streamI.Read (buffer, 0, buffer.Length);
					}
					streamO.FlushFinalBlock ();

					byte[] newKeyRaw = newKeyStream.ToArray ();
					userKeys.Put (userID, newKeyRaw);
				} catch (Exception ex) {
					Console.Error.WriteLine ("Exception trying to set user password for first time.");
					throw ex;
				}
				return rsap;
			} catch {
				Stream cryptoStream = CreateCryptoStream (memoryStream, userID, password, pepper, CryptoStreamMode.Read);
				RSAParameters privateKey = DeserializeKey (cryptoStream);
				return privateKey;
			}
		}

		public byte[] Sign (byte[] data, byte[] userID)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (userID == null)
				throw new ArgumentNullException ("userID");
			RSA rsaa = RSA.Create ();
			rsaa.ImportParameters (loggedInUsers [userID]);
			return PKCS1.Sign_v15 (rsaa, SHA256.Create (), data);
		}

		public byte[][] Sign (byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			List<byte[]> rv = new List<byte[]> ();
			foreach (var userData in loggedInUsers) {
				RSA rsaa = RSA.Create ();
				rsaa.ImportParameters (userData.Value);
				byte[] sig = PKCS1.Sign_v15 (rsaa, SHA256.Create (), data);
				rv.Add (userData.Key);
				rv.Add (sig);
			}
			return rv.ToArray ();
		}

		public void CreateUser (string name, string password, string pepper, byte[] parentID)
		{
			byte[] userID = new byte[32];
			RandomNumberGenerator.Create ().GetBytes (userID);
			Console.Write ("User ID:");
			foreach (byte b in userID)
				Console.Write (" {0:X2}", b);
			Console.WriteLine ();

			using (var rsa = new RSACryptoServiceProvider (2048)) {
				using (var rsaSign = new RSACryptoServiceProvider (2048)) {
					try {
						userNames.Put (userID, name);
						if (parentID != null) {
							userParents.Put (userID, parentID);
						}
						var Rij = CreateRijndael (userID, password, pepper);
						{
							MemoryStream streamI = new MemoryStream (System.Text.Encoding.Unicode.GetBytes (rsa.ToXmlString (true)));
							MemoryStream newKeyStream = new MemoryStream ();
							CryptoStream streamO = new CryptoStream (newKeyStream, Rij.CreateEncryptor (), CryptoStreamMode.Write);
							var buffer = new byte[1024];
							var read = streamI.Read (buffer, 0, buffer.Length);
							while (read > 0) {
								streamO.Write (buffer, 0, read);
								read = streamI.Read (buffer, 0, buffer.Length);
							}
							streamO.FlushFinalBlock ();
							userKeys.Put (userID, newKeyStream.ToArray ());
						}
						userCerts.Put (userID, System.Text.Encoding.Unicode.GetBytes (rsa.ToXmlString (false)));

						{
							MemoryStream streamI = new MemoryStream (System.Text.Encoding.Unicode.GetBytes (rsaSign.ToXmlString (true)));
							MemoryStream newKeyStream = new MemoryStream ();
							CryptoStream streamO = new CryptoStream (newKeyStream, Rij.CreateEncryptor (), CryptoStreamMode.Write);
							var buffer = new byte[1024];
							var read = streamI.Read (buffer, 0, buffer.Length);
							while (read > 0) {
								streamO.Write (buffer, 0, read);
								read = streamI.Read (buffer, 0, buffer.Length);
							}
							streamO.FlushFinalBlock ();
							userSigningKeys.Put (userID, newKeyStream.ToArray ());
						}
						userSigningCerts.Put (userID, System.Text.Encoding.Unicode.GetBytes (rsaSign.ToXmlString (false)));

					} finally {
						rsa.PersistKeyInCsp = false;
						rsaSign.PersistKeyInCsp = false;
					}
				}
			}
		}
	}
}

