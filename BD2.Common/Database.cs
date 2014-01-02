//
//  Database.cs
//
//  Author:
//       Behrooz Amoozad <behrooz0az@gmail.com>
//
//  Copyright (c) 2013 behrooz
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace BD2.Common
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
			backends = new  ChunkRepositoryCollection (this);
			frontends = new FrontendCollection (this);
		}

		public Database (IEnumerable<ChunkRepository> Backends, IEnumerable<Frontend> Frontends)
		{
			backends = new  ChunkRepositoryCollection (this);
			foreach (ChunkRepository CR in Backends)
				backends.AddRepository (CR);
			frontends = new FrontendCollection (this);
			foreach (Frontend FR in Frontends)
				frontends.AddFrontend (FR);
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
