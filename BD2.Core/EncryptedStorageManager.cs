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
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Services;
using System.Security.Cryptography;

namespace BD2.Core
{
	public sealed class EncryptedStorageManager
	{
		readonly UserRepository userRepository;
		readonly SortedDictionary<byte, MemoryStream> tempStorage;
		readonly SortedDictionary<byte[], AESEncryptingKeyValueStorage> permanentStorage;
		byte lastID;
		//ordinal
		readonly SortedDictionary<byte[], byte> storageOIDs;
		//unique
		readonly SortedDictionary<byte, byte[]> storageUIDs;

		public byte GetStorage (byte[] keyID)
		{
			if (keyID == null)
				throw new ArgumentNullException ("keyID");
			lock (storageOIDs) {
				if (storageOIDs.ContainsKey (keyID)) {
					return storageOIDs [keyID];
				}
			}
			lock (permanentStorage)
				if (permanentStorage.ContainsKey (keyID)) {
					if (lastID == 255)
						throw new InvalidOperationException ();
					storageOIDs.Add (keyID, ++lastID);
					storageUIDs.Add (lastID, keyID);
					tempStorage.Add (lastID, new MemoryStream ());
					return lastID;
				}
			throw new KeyNotFoundException ();
		}

		public byte[] GetKeyForUsers (byte[][] userIDs)
		{
			if (userRepository.LoggedInUsersCount == 0)
				throw new InvalidOperationException ("No users are logged in cannot perform any de/encryption.");
			bool f = false;
			foreach (var u in userRepository.LoggedInUsers) {
				if (u.In (userIDs)) {
					f = true;
					break;
				}
			}
			if (!f)
				throw new InvalidOperationException ("Cannot encrypt in a way that disallows the current user from accessing the data.");//NO FALSE SENSE OF SECURITY
			SortedSet<byte[]> include, exclude, accessible;
			include = null;
			accessible = null;
			exclude = new SortedSet<byte[]> ();
			foreach (var u in userRepository.GetUsers ()) {
				SortedSet<byte[]> keys = userRepository.GetUserRepository (u.Key).ESymmetricKeys.EnumerateKeys ().ToSet ();
				if (u.Key.In (userIDs)) {
					if (include == null)
						include = new SortedSet<byte[]> (keys);
					else
						include.IntersectWith (keys);
				} else
					exclude.UnionWith (keys);
			}
			include.ExceptWith (exclude);
			//TODO:union list of keys we can use, intersect result with 'include'
			foreach (var u in userRepository.LoggedInUsers) {
				SortedSet<byte[]> keys = userRepository.GetUserRepository (u).ESymmetricKeys.EnumerateKeys ().ToSet ();
				if (accessible == null)
					accessible = new SortedSet<byte[]> (keys);
				else
					accessible.IntersectWith (keys);
			}
			SortedSet<byte[]> usable = new SortedSet<byte[]> (accessible);
			usable.IntersectWith (include);
			if (usable.Count == 0) {
				return CreateNewKey (userIDs);
			}
			return usable.GetEnumerator ().First ();
		}

		public byte[] CreateNewKey (byte[][] userIDs)
		{
			Aes aes = Aes.Create ();
			aes.GenerateKey ();
			byte[] keyBytes = aes.Key;
			byte[] keyID = keyBytes.SHA256 ();
			//TODO: create AES KEY, intern it for all logged in users; sign it with the same users; RSA encrypt it for the users
			foreach (var u in userRepository.LoggedInUsers) {
				GenericUserRepositoryCollection ur = userRepository.GetUserRepository (u);
				ur.SymmetricKeys.Put (keyID, keyBytes);
			}
			foreach (var u in userIDs) {
				if (!userRepository.LoggedInUsers.Contains (u)) {
					GenericUserRepositoryCollection ur = userRepository.GetUserRepository (u);
					ur.SymmetricKeys.Put (keyID, keyBytes);
				}
			}

			return keyID;
		}

		public void Append (byte storageID, byte[] buffer, int offset, int count)
		{
			if (storageID == 0)
				throw new ArgumentNullException ("storageID");
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			MemoryStream ms;
			lock (tempStorage)
				ms = tempStorage [storageID];
			lock (ms)
				ms.Write (buffer, offset, count);
		}
		//note on concurrency:
		//actually we don't need any checks in the commit because it should be called after we're done inserting and adding stuff, we're doing it just in case.
		public void Commit (byte[] chunkID)
		{
			lock (permanentStorage) {
				lastID = 255;
				lock (tempStorage) {
					foreach (var kv in tempStorage) {
						permanentStorage [storageUIDs [kv.Key]].Put (chunkID, kv.Value.ToArray ());
					}
				}
				lock (storageOIDs) {
					storageOIDs.Clear ();
				}
				lock (storageUIDs) {
					storageUIDs.Clear ();
				}
				lock (tempStorage) {
					tempStorage.Clear ();
				}
				permanentStorage.Clear ();
			}
		}


		public void AddUser (byte[] userID)
		{
			GenericUserRepositoryCollection userreps = userRepository.GetUserRepository (userID);
			RSAEncryptingKeyValueStorage symmetricKeys = userreps.SymmetricKeys;
			foreach (var sk in symmetricKeys) {
				if (!permanentStorage.ContainsKey (sk.Key))
					//#warning SO WRONG
					foreach (var cr in crs) {//this is sooooooooooo wrong because ih creates unnecessary KVSs in unrelated DBs
						permanentStorage.Add (sk.Key, new AESEncryptingKeyValueStorage (cr.LencryptedData [sk.Key], sk.Value, sk.Key));
					}
			}
		}

		readonly SortedSet<ChunkRepository> crs = new SortedSet<ChunkRepository> ();

		public void AddChunkRepository (ChunkRepository chunkRepository)
		{
			crs.Add (chunkRepository);
			if (chunkRepository == null)
				throw new ArgumentNullException ("chunkRepository");
			foreach (var user in userRepository.LoggedInUsers) {
				GenericUserRepositoryCollection userreps = userRepository.GetUserRepository (user);
				RSAEncryptingKeyValueStorage symmetricKeys = userreps.SymmetricKeys;
				foreach (var sk in symmetricKeys) {
					if (!permanentStorage.ContainsKey (sk.Key))
						permanentStorage.Add (sk.Key, new AESEncryptingKeyValueStorage (chunkRepository.LencryptedData [sk.Key], sk.Value, sk.Key));
				}
			}
		}


		public EncryptedStorageManager (UserRepository userRepository)
		{
			if (userRepository == null)
				throw new ArgumentNullException ("userRepository");
			this.userRepository = userRepository;
			tempStorage = new SortedDictionary<byte, MemoryStream> ();
			permanentStorage = new SortedDictionary<byte[], AESEncryptingKeyValueStorage> ();
			storageOIDs = new SortedDictionary<byte[], byte> ();
			storageUIDs = new SortedDictionary<byte, byte[]> ();
		}
	}
}

