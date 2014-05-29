/*
 * Copyright (c) 2013-2014 Behrooz Amoozad
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
using BD2.Core;
using BD2.Frontend.Table.Model;
using BD2.Common;

namespace BD2.Frontend.Table
{
	public class FrontendInstance : BD2.Frontend.Table.Model.FrontendInstance
	{
		SortedDictionary<byte[], Row> rows = new SortedDictionary<byte[], Row> (BD2.Common.ByteSequenceComparer.Shared);
		SortedDictionary<byte[], Table> tables = new SortedDictionary<byte[], Table> (BD2.Common.ByteSequenceComparer.Shared);
		SortedDictionary<byte[], Relation> relations = new SortedDictionary<byte[], Relation> (BD2.Common.ByteSequenceComparer.Shared);
		SortedDictionary<byte[], Column> columns = new SortedDictionary<byte[], Column> (BD2.Common.ByteSequenceComparer.Shared);
		SortedDictionary<byte[], ColumnSet> columnSets = new SortedDictionary<byte[], ColumnSet> (BD2.Common.ByteSequenceComparer.Shared);
		ValueSerializerBase valueSerializer;
		//SortedDictionary<byte[], BaseDataObject> volatileData = new SortedDictionary<byte[], BaseDataObject> (BD2.Common.ByteSequenceComparer.Shared);
		public override ValueSerializerBase ValueSerializer {
			get {
				return valueSerializer;
			}
		}

		public FrontendInstance (Snapshot snapshot, Frontend frontend, ValueSerializerBase valueSerializer):
			base(snapshot, frontend)
		{
			if (valueSerializer == null)
				throw new ArgumentNullException ("valueSerializer");
			this.valueSerializer = valueSerializer;
		}
		#region implemented abstract members of FrontendInstanceBase
		protected override void OnCreateObjects (byte[] chunkID, byte[] bytes)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (bytes)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					int objectCount = BR.ReadInt32 ();
					Console.WriteLine ("Deserializing {0} objects", objectCount);
					for (int n = 0; n != objectCount; n++) {
						int objectLengeth = BR.ReadInt32 ();
						//Console.WriteLine ("Length:{0}", objectLengeth);
						Guid objectTypeID = new Guid (BR.ReadBytes (16));
						BaseDataObjectTypeIdAttribute typeDescriptor = BaseDataObjectTypeIdAttribute.GetAttribFor (objectTypeID);
						BaseDataObject BDO = typeDescriptor.Deserialize (this, chunkID, BR.ReadBytes (objectLengeth - 16));
						//Console.WriteLine (BDO.GetType ().FullName);
						if (BDO is Table) {
							tables.Add (BDO.ObjectID, (Table)BDO);
						} else if (BDO is Column) {
							columns.Add (BDO.ObjectID, (Column)BDO);
						} else if (BDO is ColumnSet) {
							columnSets.Add (BDO.ObjectID, (ColumnSet)BDO);
						} else if (BDO is Row) {
							rows.Add (BDO.ObjectID, (Row)BDO);
						}
					}
				}
			}
		}

		protected override BaseDataObject GetObjectWithID (byte[] objectID)
		{
			if (rows.ContainsKey (objectID))
				return rows [objectID];
			if (columns.ContainsKey (objectID))
				return columns [objectID];
			if (columnSets.ContainsKey (objectID))
				return columnSets [objectID];
			if (tables.ContainsKey (objectID))
				return tables [objectID];
			if (relations.ContainsKey (objectID))
				return relations [objectID];
			return null;
		}
		#endregion
		//to avoid duplicates
		public override BD2.Frontend.Table.Model.Column GetColumn (string name, Type type, bool allowNull, long length)
		{
			Column nc = new Column (this, null, name, type, allowNull, length);
			byte[] hash = nc.GetPersistentUniqueObjectID ();
			if (columns.ContainsKey (hash)) 
				return columns [hash];
			Snapshot.AddVolatileData (nc);
			columns.Add (hash, nc);
			return nc;
		}

		public override ColumnSet GetColumnSet (Model.Column[] columns)
		{
			if (columns == null)
				throw new ArgumentNullException ("columns");
			ColumnSet cs = new ColumnSet (this, null, columns);
			byte[] hash = cs.ObjectID;
			if (columnSets.ContainsKey (hash)) {
				return columnSets [hash];
			}
			Snapshot.AddVolatileData (cs);
			columnSets.Add (hash, cs);
			return cs;
		}

		public BD2.Frontend.Table.Row CreateRow (BD2.Frontend.Table.Model.Table table, BD2.Frontend.Table.Model.ColumnSet columnSet, object[] objects)
		{
			Row r = new Row (this, null, table, columnSet, objects);
			rows.Add (r.ObjectID, r);
			Snapshot.AddVolatileData (r);
			return r;
		}

		public void Flush ()
		{
			Snapshot.Database.SaveAllSnapshots ();
		}
		#region implemented abstract members of FrontendInstance
		public override BD2.Frontend.Table.Model.Table GetTable (string name)
		{
			Table temp = new Table (this, null, name);
			if (tables.ContainsKey (temp.ObjectID)) {
				return tables [temp.ObjectID];
			}
			Snapshot.AddVolatileData (temp);
			tables.Add (temp.ObjectID, temp);
			return temp;
		}

		public override IEnumerable<BD2.Frontend.Table.Model.Row> GetRows (BD2.Frontend.Table.Model.Table table)
		{
			foreach (var rt in rows) {
				if (rt.Value.Table == table) {
					yield return rt.Value;
				}
			}
		}
		#endregion
		public override ColumnSet GetColumnSetByID (byte[] id)
		{
			if (!columnSets.ContainsKey (id))
				System.Diagnostics.Debugger.Break ();
			return columnSets [id];
		}

		public override BD2.Frontend.Table.Model.Table GetTableByID (byte[] id)
		{
			if (!tables.ContainsKey (id))
				System.Diagnostics.Debugger.Break ();
			return tables [id];
		}

		public override BD2.Frontend.Table.Model.Column GetColumnByID (byte[] id)
		{
			if (!columns.ContainsKey (id))
				System.Diagnostics.Debugger.Break ();
			return columns [id];
		}

		public override BD2.Frontend.Table.Model.Row GetRowByID (byte[] id)
		{
			if (!rows.ContainsKey (id))
				System.Diagnostics.Debugger.Break ();
			return rows [id];
		}

		public IEnumerable<Table> GetTables ()
		{
			return new SortedSet<Table> (tables.Values);
		}
	}
}
