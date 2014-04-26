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

		public ChunkRepositoryCollection Backends {
			get {
				return backends;
			}
		}

		internal IEnumerable<FrontendInstanceBase> FrontendInstances {
			get {
				return new SortedSet<FrontendInstanceBase> (FrontendInstances);
			}
		}

		public Database ()
		{
			backends = new ChunkRepositoryCollection ();
		}

		public Database (string name)
			: this()
		{
			this.name = name;
		}

		public Database (IEnumerable<ChunkRepository> backends, IEnumerable<Frontend> frontends)
		{
			this.backends = new ChunkRepositoryCollection ();
			foreach (ChunkRepository CR in backends)
				this.backends.AddRepository (CR);
			primary = CreateSnapshot ("Primary");
			foreach (Frontend Frontend in frontends)
				this.frontendInstances.Add (Frontend.CreateInstanse (primary));
		}

		public Database (IEnumerable<ChunkRepository> backends, IEnumerable<Frontend> frontends, string name)
			:this(backends, frontends)
		{
			this.name = name;
		}

		public Database (DatabaseConfiguration databaseConfiguration)
		{
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
