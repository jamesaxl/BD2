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
using Mono.Security.Cryptography;
using BD2.Core;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Reflection.Emit;
using System.Globalization;

namespace BD2.Core
{
	public sealed class Database
	{

		public string Name {
			get {
				return System.Text.Encoding.Unicode.GetString (userStorage.GetMetaData (System.Text.Encoding.Unicode.GetBytes ("Name")));
			}
		}

		readonly UserRepository userStorage;
		readonly SortedDictionary<byte[], ChunkRepository> dataStorage = new SortedDictionary<byte[], ChunkRepository> ();
		readonly SortedDictionary<string, FrontendBase> frontends = new SortedDictionary<string, FrontendBase> ();
		readonly SortedSet<FrontendInstanceBase> frontendInstances = new SortedSet<FrontendInstanceBase> ();
		readonly EncryptedStorageManager encryptedStorageManager;

		public UserRepository UserStorage {
			get {
				return userStorage;
			}
		}

		public SortedDictionary<byte[], ChunkRepository> DataStorage {
			get {
				return dataStorage;
			}
		}

		byte[] DefaultStorage { get; set; }

		internal IEnumerable<FrontendInstanceBase> FrontendInstances {
			get {
				return new SortedSet<FrontendInstanceBase> (frontendInstances);
			}
		}

		public IEnumerable<KeyValuePair<byte[], string>> GetUsers ()
		{
			return userStorage.GetUsers ();
		}

		public bool VerifyPassword (byte[] userID, string password, string pepper)
		{
			return userStorage.VerifyPassword (userID, password, pepper);
		}

		DatabasePath GetRepositoryPath (byte[] ID)
		{
			return new DatabasePath (ID.ToHexadecimal ());
		}

		public bool Login (byte[] userID, string password, string pepper)
		{
			if (userStorage.LoggedInUsers.Contains (userID))
				throw new InvalidOperationException (string.Format ("User [0x{0}] is already logged in.", userID.ToHexadecimal ()));
			bool v = userStorage.Login (userID, password, pepper);
			if (v) {
				foreach (var ur in userStorage.GetUserRepositories (userID)) {
					byte[] repoInfo = userStorage.GetRawRepositoryInfo (ur);
					System.Xml.Serialization.XmlSerializer xmls = new System.Xml.Serialization.XmlSerializer (typeof(ChunkRepositoryConfiguration));
					ChunkRepositoryConfiguration cri = (ChunkRepositoryConfiguration)xmls.Deserialize (new MemoryStream (repoInfo, false));
					DatabasePath dbp = GetRepositoryPath (cri.ID);
					if (dataStorage.ContainsKey (ur)) {
						foreach (var sk in userStorage.GetUserRepository (ur).SymmetricKeys) {
							if (!dataStorage [ur].LencryptedData.ContainsKey (sk.Key)) {
								dataStorage [ur].LencryptedData.Add (sk.Key, new AESEncryptingKeyValueStorage (
									new LevelDBKeyValueStorage (dbp.CreatePath ("Encrypted").CreatePath (sk.Key.ToHexadecimal ()))
								, sk.Value));
							}
						}
					} else {
						SortedDictionary<byte[], KeyValueStorage<byte[]>> EncryptedData = new SortedDictionary<byte[], KeyValueStorage<byte[]>> ();
						foreach (var sk in userStorage.GetUserRepository (ur).SymmetricKeys) {
							KeyValueStorageConfiguration ESC = new KeyValueStorageConfiguration ();
							ESC.Type = cri.Data.Type;
							ESC.Path = sk.Key.ToHexadecimal ();
							EncryptedData.Add (sk.Key, new AESEncryptingKeyValueStorage (
								ESC.OpenStorage<byte[]> (dbp.CreatePath ("Encrypted")), sk.Value));
						}
						ChunkRepository cr = new ChunkRepository (
							                     cri.Data.OpenStorage<byte[]> (dbp),
							                     cri.TopLevels.OpenStorage<byte[][]> (dbp),
							                     cri.Dependencies.OpenStorage<byte[][]> (dbp),
							                     cri.Meta.OpenStorage<byte[]> (dbp),
							                     cri.MetaTopLevels.OpenStorage<byte[][]> (dbp),
							                     cri.MetaDependencies.OpenStorage<byte[][]> (dbp),
							                     cri.Signatures.OpenStorage<byte[][]> (dbp),
							                     cri.ChunkSymmetricKeys.OpenStorage<byte[][]> (dbp),
							                     cri.Index.OpenStorage<byte[]> (dbp),
							                     EncryptedData);
						dataStorage.Add (cr.ID, cr);
					}
					//encryptedStorageManager.Add (ur, new EncryptedStorageManager (cr, userStorage));
				}
			}
			return v;
		}

		public FrontendBase GetFrontend (string frontendName)
		{
			if (frontendName == null)
				throw new ArgumentNullException ("frontendName");
			return frontends [frontendName];
		}


		public Database (UserRepository userStorage, IEnumerable<FrontendBase> frontends)
		{
			if (userStorage == null)
				throw new ArgumentNullException ("userStorage");
			if (frontends == null)
				throw new ArgumentNullException ("frontends");
			this.userStorage = userStorage;
			this.frontends = new SortedDictionary<string, FrontendBase> ();
			DataContext dc = new NullDataContext ();
			foreach (var f in frontends) {
				if (f == null)
					throw new ArgumentException ("Has at least one null item", "frontends");
				this.frontends.Add (f.Name, f);
				frontendInstances.Add (f.GetInstanse (dc));
			}
			encryptedStorageManager = new EncryptedStorageManager (userStorage);

		}

	
		public void Load ()
		{
			//sanity check
			foreach (var crt in dataStorage) {
				byte[][] deps = crt.Value.GetRepositoryDependencies ();
				foreach (byte[] dep in deps) {
					if (!dataStorage.ContainsKey (dep)) {
						throw new Exception ("DONTPANIC Unresolved repository.");
					}
				}
			}

			SortedDictionary<byte[], ChunkRepository> repos = new SortedDictionary<byte[], ChunkRepository> ();
			while (repos.Count < dataStorage.Count)
				foreach (var crt in dataStorage) {
					byte[][] deps = crt.Value.GetRepositoryDependencies ();
					foreach (byte[] dep in deps) {
						if (!repos.ContainsKey (dep))
							continue;
					}
					LoadRepository (crt.Value);
					repos.Add (crt.Key, crt.Value);
				}

		}

		void LoadRepository (ChunkRepository cr)
		{

			SortedDictionary<byte[], byte[]> pendingData = new SortedDictionary<byte[], byte[]> (ByteSequenceComparer.Shared);
			foreach (var d in cr.Ldata) {
				pendingData.Add (d.Key, d.Value);
			}

			while (pendingData.Count != 0)
				foreach (var tup in new SortedDictionary<byte[], byte[]>(pendingData, BD2.Core.ByteSequenceComparer.Shared)) {
					//Console.WriteLine ("Testing object");
					byte[] nchunk = tup.Key;
					byte[][] deps = cr.LdataDependencies.Get (nchunk);
					if (deps == null)
						throw new InvalidDataException ();
					Console.WriteLine ("dependency count = {0}", deps.Length);
					foreach (byte[] dep in deps) {

						if (pendingData.ContainsKey (dep)) {
							Console.WriteLine ("Found an unloaded dependency, skipping...");
							nchunk = null;
						}
					}
					if (nchunk == null)
						continue;
					Console.WriteLine ("All the dependencies are loaded, proceeding...");
					byte[] chunkData = tup.Value;
					LoadChunk (nchunk, chunkData);
					pendingData.Remove (nchunk);
				}	
		}


		ChunkHeaderv1 LoadChunk (byte[] chunkID, byte[] chunkData)
		{
			ChunkHeaderv1 ch = null;
			Console.WriteLine ("Pulled a chunk with a size of {0}", chunkData.Length);
			System.IO.MemoryStream MS = new System.IO.MemoryStream (chunkData);
			System.IO.BinaryReader BR = new System.IO.BinaryReader (MS);
			int chunkVersion = BR.ReadInt32 ();
			if (chunkVersion == 1) {
				ch = ChunkHeaderv1.Deserialzie (BR.ReadBytes (BR.ReadInt32 ())); 

				//chunkHeaders.Add (chunkID, ch);
			} else
				throw new Exception (string.Format ("Chunk meta data version is 0x{0:x} which is not supported by this version of BD2.", chunkVersion));
			int sectionCount = BR.ReadInt32 ();
			for (int sectionID = 0; sectionID != sectionCount; sectionID++) {
				switch (chunkVersion) {
				case 1:
					int SectionVersion = BR.ReadInt32 ();
					switch (SectionVersion) {
					case 1:
						int payloadLength = BR.ReadInt32 ();
						byte[] payload = BR.ReadBytes (payloadLength);
						Console.WriteLine ("Trying to deserialize in {0} frontends.", frontendInstances.Count);
						foreach (FrontendInstanceBase fib in frontendInstances)
							fib.CreateObjects (chunkID, payload);
						break;
					default:
						throw new Exception ("This version of BD2 does not support the version of data provided.");						
					}
					break;
				default:
					throw new Exception ("This version of BD2 does not support the version of data provided.");
				}
			}
			return ch;
		}


		public void Close ()
		{
			foreach (var fib in frontendInstances) {
				//fib.Close ();
			}
		 
			foreach (var f in frontends) {
				//f.Value.Close ();
			}
			//backends.Close ();
		}

		#region IDataSource implementation


		public byte[] CommitTransaction (SortedSet<BaseDataObjectVersion> objects)
		{
			System.IO.MemoryStream MS = new System.IO.MemoryStream ();
			System.IO.MemoryStream MSRP = new System.IO.MemoryStream ();
			System.IO.BinaryWriter MSBW = new System.IO.BinaryWriter (MS);
			SortedSet<byte[]> dependencies = new SortedSet<byte[]> (BD2.Core.ByteSequenceComparer.Shared);
			ChunkHeaderv1 ch = new ChunkHeaderv1 (DateTime.UtcNow, "");
			MSBW.Write (ch.Version);	
			byte[] chbytes = ch.Serialize ();
			MSBW.Write (chbytes.Length);
			MSBW.Write (chbytes);
			MSBW.Write (1);
			Console.WriteLine ("{0} sections", 1);
			int n = 0;
			//foreach (var tup in data) {
			MSBW.Write (1);
			Console.WriteLine ("Section version is {0}", 1);
			System.Collections.Generic.SortedDictionary <BaseDataObjectVersion, LinkedListNode<BaseDataObjectVersion>> ss = new  System.Collections.Generic.SortedDictionary <BaseDataObjectVersion, LinkedListNode<BaseDataObjectVersion>> ();
			System.Collections.Generic.LinkedList <BaseDataObjectVersion> ll = new LinkedList<BaseDataObjectVersion> ();
			foreach (BaseDataObjectVersion bdo in objects) {
				if (!ss.ContainsKey (bdo))
					ss.Add (bdo, ll.AddLast (bdo));

				foreach (BaseDataObjectVersion dependency in bdo.GetDependenies ()) {
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
			foreach (BaseDataObjectVersion bdo in ll) {
				n++;
				System.IO.MemoryStream MST = new System.IO.MemoryStream ();
				MST.Write (bdo.ObjectType.ToByteArray (), 0, 16);
				bdo.Serialize (MST, encryptedStorageManager);
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
			byte[] encoded = MSRP.ToArray ();
			Console.WriteLine ("{0} objects encoded in {1} bytes", objects.Count, encoded.Length);
			MSBW.Write (encoded.Length);
			MSBW.Write (encoded);
			//}
			if (n == 0) {
				Console.WriteLine ("No objects to save, nothing to do");
				return null;
			}
			System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create ();
			Console.WriteLine ("{0} dependencies", dependencies.Count);
			byte[][] deps = new byte [dependencies.Count][];
			int depid = 0;
			foreach (byte[] dep in dependencies)
				deps [depid++] = dep;
			Console.WriteLine ("Writing {0} bytes representing {1} objects to backend", MS.Length, n);
			byte[] buf = MS.ToArray ();
			byte[] chunkID = sha.ComputeHash (buf);
			dataStorage [DefaultStorage].Push (chunkID, buf, deps, userStorage.Sign (chunkID));
			foreach (var bdo in objects) {
				bdo.SetChunkID (chunkID);
			}
			return chunkID;
		}

		#endregion
	}
}
