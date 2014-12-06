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

namespace BD2.Core
{
	public sealed class UserRepository
	{
		readonly KeyValueStorage<string> usernames;
		readonly KeyValueStorage<byte[]> usercerts;
		readonly KeyValueStorage<byte[]> userkeys;
		readonly KeyValueStorage<byte[][]> userrepositories;
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

		public UserRepository (DatabasePath path, Stream configuration)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			//this.path = path;
			System.Xml.Serialization.XmlSerializer xmls = new System.Xml.Serialization.XmlSerializer (typeof(UserRepositoryConfiguration));
			UserRepositoryConfiguration config = (UserRepositoryConfiguration)xmls.Deserialize (configuration);
			usernames = new LevelDBKeyValueStorage<string> (path.CreatePath (config.UsersPath));
			usercerts = new LevelDBKeyValueStorage<byte[]> (path.CreatePath (config.UserCertsPath));
			userkeys = new LevelDBKeyValueStorage<byte[]> (path.CreatePath (config.UserKeysPath));
			userrepositories = new LevelDBKeyValueStorage<byte[][]> (path.CreatePath (config.UserRepositoresPath));
			repositories = new LevelDBKeyValueStorage<byte[]> (path.CreatePath (config.RepositoresPath));
			meta = new LevelDBKeyValueStorage<byte[]> (path.CreatePath (config.MetaPath));
			//permissions = new LevelDBKeyValueStorage<byte[]> (path.CreatePath (config.PermissionsPath));
			genericRepositories = new SortedDictionary<byte[], GenericUserRepositoryCollection> ();
			loggedInUsers = new SortedDictionary<byte[], RSAParameters> ();
			foreach (var user in usernames) {
				genericRepositories.Add (user.Key, 
					new GenericUserRepositoryCollection (path.CreatePath (user.Key.ToHexadecimal ()),
						DeserializeKey (new MemoryStream (usercerts.Get (user.Key), false))
					)
				);
			}
		}

		public IEnumerable<KeyValuePair<byte[], string>> GetUsers ()
		{
			return usernames;
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
			return userrepositories.Get (userID);
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
			byte[] privateKeyRaw = userkeys.Get (userID);

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
			userkeys.Put (userID, newKeyRaw);
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
			Rfc2898DeriveBytes aesKey = new Rfc2898DeriveBytes (passpepper, userID, 1 << 24);//16 Mibi, I don't care for cpus, EVER
			Rij.Key = aesKey.GetBytes (Rij.KeySize / 8);
			Rij.IV = aesKey.GetBytes (Rij.BlockSize / 8);
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
			System.Xml.Serialization.XmlSerializer xmls = new System.Xml.Serialization.XmlSerializer (typeof(RSAParameters));
			RSAParameters key = (RSAParameters)xmls.Deserialize (stream);
			return key;
		}

		public RSAParameters GetPublicKey (byte[] userID)
		{
			if (userID == null)
				throw new ArgumentNullException ("userID");
			byte[] pubKeyRaw = usercerts.Get (userID);
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
			byte[] privateKeyRaw = userkeys.Get (userID);
			MemoryStream memoryStream = new MemoryStream (privateKeyRaw);
			Stream cryptoStream = CreateCryptoStream (memoryStream, userID, password, pepper, CryptoStreamMode.Read);
			RSAParameters privateKey = DeserializeKey (cryptoStream);
			return privateKey;

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
	
	}
}

