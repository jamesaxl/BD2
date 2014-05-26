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

namespace BD2.Core
{
	public sealed class Database
	{
		string name;
		object snapState = new object ();
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

		public Database (IEnumerable<ChunkRepository> backends, IEnumerable<Frontend> frontends)
		{
			this.backends = new ChunkRepositoryCollection ();
			foreach (ChunkRepository CR in backends)
				this.backends.AddRepository (CR);
			primary = CreateSnapshot ("Primary");
			foreach (Frontend Frontend in frontends) {
				this.frontends.Add (Frontend.Name, Frontend);
				Frontend.Database = this;
				this.frontendInstances.Add (Frontend.CreateInstanse (primary));
			}
			Load ();
		}

		public void SaveAllSnapshots ()
		{
			SortedDictionary<Snapshot,SortedSet<BaseDataObject>> bdos = new SortedDictionary<Snapshot, SortedSet<BaseDataObject>> ();
			foreach (Snapshot snapshot in snapshots) {
				bdos.Add (snapshot, snapshot.GetVolatileData ());
			}
			System.IO.MemoryStream MS = new System.IO.MemoryStream ();
			System.IO.MemoryStream MSID = new System.IO.MemoryStream ();
			SortedSet<byte[]> dependencies = new SortedSet<byte[]> (BD2.Common.ByteSequenceComparer.Shared);
			foreach (var tup in bdos) {
				MS.WriteByte (1);
				RawProxy.RawProxyCollection rpc = tup.Key.GetRawProxies ();
				System.IO.MemoryStream MSRP = new System.IO.MemoryStream ();
				foreach (BaseDataObject bdo in tup.Value) {
					foreach (BaseDataObject dependency in bdo.GetDependenies ()) {
						if (!dependencies.Contains (dependency.GetPersistentUniqueObjectID ()))
							dependencies.Add (dependency.GetPersistentUniqueObjectID ());
					}
					MSRP.Write (bdo.ObjectType.ToByteArray (), 0, 16);
					System.IO.MemoryStream MST = new System.IO.MemoryStream ();
					bdo.Serialize (MST);
					byte[] bytes = MST.ToArray ();
					MSRP.Write (bytes, 0, bytes.Length);
					MSID.Write (bdo.ObjectID, 0, 32);
				}
				byte[] encoded = rpc.ChainEncode (MSRP.ToArray ());
				MS.Write (encoded, 0, encoded.Length);
			}
			System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create ();
			List<byte[]> deps = new List<byte[]> (dependencies);
			Console.WriteLine ("Writing {0} bytes to backend", MS.Length);
			Backends.Push (sha.ComputeHash (MSID.ToArray ()), MS.ToArray (), deps.ToArray ());
		}

		void Load ()
		{
			SortedSet<byte[]> pendingData = new SortedSet<byte[]> (backends.Enumerate (), BD2.Common.ByteSequenceComparer.Shared);
			SortedSet<byte[]> loaded = new SortedSet<byte[]> (BD2.Common.ByteSequenceComparer.Shared);
			RawProxy.RawProxyCollection rpc = new BD2.RawProxy.RawProxyCollection ();
			foreach (var rp in backends.EnumerateRawProxyData()) {
				rpc.Add (RawProxy.RawProxyAttribute.DeserializeFromRawData (rp.Value));
			}
			foreach (var tup in new SortedSet<byte[]>(pendingData, BD2.Common.ByteSequenceComparer.Shared)) {
				byte[][] deps = backends.PullDependencies (tup);
				foreach (byte[] dep in deps) {
					if (!loaded.Contains (dep)) {
						deps = null;
						break;
					}
				}
				if (deps == null)
					continue;
				byte[] chunkData = backends.PullData (tup);
				System.IO.MemoryStream MS = new System.IO.MemoryStream (chunkData);
				System.IO.BinaryReader BR = new System.IO.BinaryReader (MS);
				int chunkVersion = BR.ReadInt32 ();
				switch (chunkVersion) {
				case 1:
					while (MS.Position < MS.Length) {
						int proxyCount = BR.ReadInt32 ();
						System.Collections.Generic.List<byte[]> proxies = new List<byte[]> ();
						for (int n = 0; n != proxyCount; n++) {
							proxies.Add (BR.ReadBytes (20));
						}
						int payloadLength = BR.ReadInt32 ();
						byte[] payload = BR.ReadBytes (payloadLength);
						byte[] decodedPayload = rpc.ChainDecode (payload, proxies.ToArray ());
						foreach (FrontendInstanceBase fib in frontendInstances)
							fib.CreateObjects (decodedPayload);
					}
					break;
				default:
					throw new Exception ("This version of BD2 does not support the version of data provided.");
				}
				pendingData.Remove (tup);
			}
		}

		public Database (IEnumerable<ChunkRepository> backends, IEnumerable<Frontend> frontends, string name)
			:this(backends, frontends)
		{
			this.name = name;
		}

		public Database (DatabaseConfiguration databaseConfiguration)
		{
			if (databaseConfiguration == null)
				throw new ArgumentNullException ("databaseConfiguration");
			foreach (var Tuple in databaseConfiguration.Backends) {
				ChunkRepository repo = (ChunkRepository)(Type.GetType (Tuple.Item1).Assembly.GetType ("Repository").GetConstructor (new Type[] { typeof(string) }).Invoke (null, new object[] { Tuple.Item2 }));
				this.backends.AddRepository (repo);
			}
			primary = CreateSnapshot ("Primary");
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
			}
			return new Snapshot (this, snapshotName, backends.Enumerate ());
		}

		public Snapshot CreateSnapshot (string snapshotName)
		{
			if (snapshotName == null)
				throw new ArgumentNullException ("name");
			Snapshot snap;
			lock (snapState) {
				snap = new Snapshot (this, snapshotName, backends.Enumerate ());
				lock (snapshots) {
					snapshots.Add (snap);
				}
			}
			return snap;
		}
	}
}
