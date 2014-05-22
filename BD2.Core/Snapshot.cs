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
using BD2.Common;

namespace BD2.Core
{
	public sealed class Snapshot : Serializable, IComparable<Snapshot>
	{

		public override void Serialize (System.IO.Stream stream)
		{
			using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (stream)) {
				BW.Write (name);
				BW.Write (objects.Count);
				foreach (byte[] objectID in objects)
					BW.Write (objectID);
			}
		}

		public static Snapshot Deserialize (Database database, byte[] buffer)
		{
			string name;
			SortedSet<byte[]> objects = new SortedSet<byte[]> ();
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (buffer,false)) {
				using (System.IO.BinaryReader BR =  new System.IO.BinaryReader (MS)) {
					name = BR.ReadString ();
					int objectCount = BR.ReadInt32 ();
					for (int n = 0; n != objectCount; n++) {
						objects.Add (BR.ReadBytes (BR.ReadInt32 ()));
					}
				}
				return new Snapshot (database, name, objects);
			}
		}

		SortedSet<byte[]> objects = new SortedSet<byte[]> ();
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

		public SortedSet<byte[]> GetChunks ()
		{
			lock (objects) {
				return new SortedSet<byte[]> (objects);
			}
		}

		public Snapshot (Database database, string name, IEnumerable<byte[]> objects)
		{
			if (database == null)
				throw new ArgumentNullException ("database");
			if (name == null)
				throw new ArgumentNullException ("name");
			this.database = database;
			this.name = name;
			this.objects = new SortedSet<byte[]> (objects, BD2.Common.ByteSequenceComparer.Shared);
		}
		#region IComparable implementation
		public int CompareTo (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			Snapshot snapshot = obj as Snapshot;
			return snapshot.name.CompareTo (name);

		}

		int IComparable<Snapshot>.CompareTo (Snapshot other)
		{
			if (other == null)
				throw new ArgumentNullException ("other");
			return other.name.CompareTo (name);
		}
		#endregion
		public void PutObjects (IEnumerable<BaseDataObject> objects)
		{
			System.IO.MemoryStream MS = new System.IO.MemoryStream ();
			System.IO.MemoryStream MSID = new System.IO.MemoryStream ();
			SortedSet<byte[]> dependencies = new SortedSet<byte[]> (BD2.Common.ByteSequenceComparer.Shared);
			foreach (BaseDataObject bdo in objects) {
				foreach (BaseDataObject dependency in bdo.GetDependenies ()) {
					if (!dependencies.Contains (dependency.GetPersistentUniqueObjectID ()))
						dependencies.Add (dependency.GetPersistentUniqueObjectID ());
				}
				MS.Write (bdo.ObjectType.ToByteArray (), 0, 16);
				System.IO.MemoryStream MST = new System.IO.MemoryStream ();
				bdo.Serialize (MST);
				byte[] bytes = MST.ToArray ();
				MS.Write (bytes, 0, bytes.Length);
				MSID.Write (bdo.ObjectID, 0, 32);
			}
			System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create ();
			List<byte[]> deps = new List<byte[]> (dependencies);
			Console.WriteLine ("Writing {0} bytes to backend", MS.Length);
			database.Backends.Push (sha.ComputeHash (MSID.ToArray ()), MS.ToArray (), deps.ToArray ());
		}

		public void PutObject (BaseDataObject obj)
		{
			System.IO.MemoryStream MS = new System.IO.MemoryStream ();
			List<byte[]> dependencies = new List<byte[]> ();
			foreach (BaseDataObject dependency in obj.GetDependenies ()) {
				dependencies.Add (dependency.GetPersistentUniqueObjectID ());
			}
			MS.Write (obj.ObjectType.ToByteArray (), 0, 16);
			obj.Serialize (MS);
			database.Backends.Push (obj.GetPersistentUniqueObjectID (), MS.ToArray (), dependencies.ToArray ());
		}
	}
}
