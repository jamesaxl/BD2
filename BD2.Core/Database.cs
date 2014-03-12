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
 * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
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
		SortedDictionary<SnapshotDescriptor, SortedSet<byte[]>> savedSnapshots = new SortedDictionary<SnapshotDescriptor, SortedSet<byte[]>> ();
		object snapState = new object ();
		SortedSet<Snapshot> snapshots = new SortedSet<Snapshot> ();
		SortedSet<Snapshot> aliveSnapshots = new SortedSet<Snapshot> ();
		ChunkRepositoryCollection backends;
		FrontendCollection frontends;

		public ChunkRepositoryCollection Backends {
			get {
				return backends;
			}
		}

		internal FrontendCollection Frontends {
			get {
				return frontends;
			}
		}

		public Database ()
		{
			backends = new ChunkRepositoryCollection ();
			frontends = new FrontendCollection (this);
		}

		public Database (IEnumerable<ChunkRepository> backends, IEnumerable<Frontend> frontends)
		{
			this.backends = new ChunkRepositoryCollection ();
			foreach (ChunkRepository CR in backends)
				this.backends.AddRepository (CR);
			this.frontends = new FrontendCollection (this);
			foreach (Frontend FR in frontends)
				this.frontends.AddFrontend (FR);
		}

		internal void HandleChunkInserted (ChunkData CD)
		{
			//frontends.
			foreach (Frontend frontend in frontends) {
				frontend.HandleTransaction (CD);
			}
		}

		public string Name {
			get {
				return name;
			}
		}

		internal void CommitTransaction (Transaction transaction)
		{
			ChunkData CD = ChunkData.FromTransaction (transaction);
			lock (transaction) {
				lock (transaction.Objects) {
					foreach (Snapshot snap in aliveSnapshots)
						if (snap != transaction.Snapshot) {
							snap.AddTransaction (transaction);
						}

				}
			}
		}

		public SortedSet<SnapshotDescriptor> GetSnapshots ()
		{
			lock (snapshots) {
				return new SortedSet<SnapshotDescriptor> (snapshots);
			}
		}

		public Snapshot CreateSnapshot (string Name, bool Static)
		{
			int SyncStatus;
			lock (snapState) {
				backends.Sync (out SyncStatus);
				Snapshot snap = new Snapshot (this, Name, this.backends.Enumerate (), Static);
				lock (snapshots) {
					lock (aliveSnapshots) {
						aliveSnapshots.Add (snap);
					}
					snapshots.Add (snap);
					if (!Static) {
						snap.GoneStatic += snap_GoneStatic;
					}
				}
				return snap;
			}
		}

		void snap_GoneStatic (object sender, EventArgs e)
		{
			Snapshot snap = (Snapshot)sender;
			lock (snapState) {
				aliveSnapshots.Remove (snap);
				snap.GoneStatic -= snap_GoneStatic;
			}
		}
	}
}
