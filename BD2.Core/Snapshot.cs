//
//  Snapshot.cs
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
using System.IO;
using BSO;

namespace BD2.Core
{
	public sealed class Snapshot
	{
		bool startedDynamic;
		bool dynamic;

		public bool Dynamic {
			get {
				return dynamic;
			}
		}

		public void GoStatic ()
		{
			if (dynamic == false)
				throw new InvalidOperationException ("Snapshot is already in static mode.");
			lock (ChunkObjects) {
				dynamic = false;
				if (GoneStatic != null)
					GoneStatic (this, EventArgs.Empty);
			}
		}

		public byte[] Serialize ()
		{

		}

		public static Snapshot Deserialize (Database database, byte[] Buffer)
		{
			return new Snapshot ();
		}

		internal EventHandler GoneStatic;
		SortedDictionary<ChunkData, SortedSet<BaseDataObjectDescriptor>> ChunkObjectDescriptors = new SortedDictionary<byte[], SortedSet<BaseDataObjectDescriptor>> ();
		Database database;

		public Database Database {
			get {
				return database;
			}
		}

		string name;

		public string Name {
			get {
				return name;
			}
		}

		Guid uniqueIdentifier;

		public Guid UniqueIdentifier {
			get {
				return uniqueIdentifier;
			}
		}

		internal void AddChunk (ChunkData Chunk)
		{
			if (Chunk == null)
				throw new ArgumentNullException ("Chunk");
			if (!dynamic)
				throw new InvalidOperationException ("Cannot modify static snapshot.");
		}

		public SortedSet<ChunkData> GetChunks ()
		{
			lock (chunks) {
				return new SortedSet<ChunkData> (chunks);
			}
		}

		public IEnumerable<BaseDataObject> GetObjects (ObjectDropEnumerationMode ObjectDropEnumerationMode)
		{
			SortedDictionary<ChunkData, SortedSet<BaseDataObjectDescriptor>> CBDODs = new  SortedDictionary<ChunkData, SortedSet<BaseDataObjectDescriptor>> ();
			SortedSet<ChunkData> CDs = GetChunks ();
			SortedSet<BaseDataObjectDescriptor> BDODs;
			foreach (ChunkData CD in CDs)
				foreach (BaseDataObjectDescriptor BDOD in CD.ObjectDescriptors) {
					switch (ObjectDropEnumerationMode) {
					case  ObjectDropEnumerationMode.Ignore:

						break;
					case ObjectDropEnumerationMode.Amend:

						break;
					case ObjectDropEnumerationMode.Yield:

						break;
					}
				}
			yield break;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BD2.Core.Snapshot"/> class.
		/// </summary>
		/// <param name='Database'>
		/// Database to make an snopshot of.
		/// </param>
		/// <param name="Chunks">
		/// chunks to have
		/// </param>
		/// <param name='Name'>
		/// for convinience/debug porpuses only.
		/// </param>
		/// <param name='Dynamic'>
		/// Whether to touch data or not.cannot be set to true once set to false.
		/// </param>
		public Snapshot (Database Database, string Name, IEnumerable<ChunkData> chunks, bool Dynamic)
		{
			database = Database;
			name = Name;
			dynamic = Dynamic;
			startedDynamic = Dynamic;
			foreach (byte[] CD in chunks) {
				ChunkObjects.Add (CD, new SortedSet<BaseDataObjectDescriptor> ());
				ChunkObjectDrops.Add (CD, new SortedSet<ObjectDrop> ());
			}
		}

		SortedSet<FrontendInstanceBase> aliveInstances;

		internal FrontendInstanceBase GetInstance (Frontend frontend)
		{
			if (frontend == null)
				throw new ArgumentNullException ("frontend");
			if (!database.Frontends.Contains (frontend)) {
				throw new Exception ("Frontend must be registered before use.nothing may go wrong.it's just a pracaution");
			}
			return frontend.GetInstance (this);
		}
	}
}
