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
		SortedDictionary<Table, SortedDictionary<byte[], Row>> perTableRows = new SortedDictionary<Table, SortedDictionary<byte[], Row>> ();
		SortedDictionary<byte[], Row> rows = new SortedDictionary<byte[], Row> (BD2.Common.ByteSequenceComparer.Shared);
		SortedDictionary<byte[], Table> tables = new SortedDictionary<byte[], Table> (BD2.Common.ByteSequenceComparer.Shared);
		SortedDictionary<byte[], Relation> relations = new SortedDictionary<byte[], Relation> (BD2.Common.ByteSequenceComparer.Shared);
		SortedDictionary<byte[], Column> columns = new SortedDictionary<byte[], Column> (BD2.Common.ByteSequenceComparer.Shared);
		SortedDictionary<byte[], ColumnSet> columnSets = new SortedDictionary<byte[], ColumnSet> (BD2.Common.ByteSequenceComparer.Shared);
		SortedDictionary<byte[], Index> indices = new SortedDictionary<byte[], Index> (BD2.Common.ByteSequenceComparer.Shared);
		ValueSerializerBase valueSerializer;
		//SortedDictionary<byte[], BaseDataObject> volatileData = new SortedDictionary<byte[], BaseDataObject> (BD2.Common.ByteSequenceComparer.Shared);
		public override ValueSerializerBase ValueSerializer {
			get {
				return valueSerializer;
			}
		}

		public FrontendInstance (Snapshot snapshot, Frontend frontend, ValueSerializerBase valueSerializer) :
			base (snapshot, frontend)
		{
			if (valueSerializer == null)
				throw new ArgumentNullException ("valueSerializer");
			this.valueSerializer = valueSerializer;
		}

		#region implemented abstract members of FrontendInstance

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
						InsertObject (typeDescriptor.Deserialize (this, chunkID, BR.ReadBytes (objectLengeth - 16)));
					}
				}
			}
		}

		void InsertObject (BaseDataObject bdo)
		{
			if (bdo is Table) {
				tables.Add (bdo.ObjectID, (Table)bdo);
				perTableRows.Add ((Table)bdo, new SortedDictionary<byte[], Row> (BD2.Common.ByteSequenceComparer.Shared));
			} else if (bdo is Column) {
				columns.Add (bdo.ObjectID, (Column)bdo);
			} else if (bdo is ColumnSet) {
				columnSets.Add (bdo.ObjectID, (ColumnSet)bdo);
			} else if (bdo is Row) {
				rows.Add (bdo.ObjectID, (Row)bdo);
				RemovePreviousVersions ((Row)bdo);
			}
		}

		protected override BaseDataObject GetObjectWithID (byte[] objectID)
		{
			//todo:use TryGetValue
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
			if (indices.ContainsKey (objectID))
				return indices [objectID];
			return null;
		}

		//to avoid duplicates
		public override BD2.Frontend.Table.Model.Column GetColumn (string name, Type type, bool allowNull, long length)
		{
			Column nc = new Column (this, null, name, type, allowNull, length);
			byte[] hash = nc.ObjectID;
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

		public BD2.Frontend.Table.Row CreateRow (BD2.Frontend.Table.Model.Table table, BD2.Frontend.Table.Model.ColumnSet columnSet, byte[][] previousID, object[] objects)
		{
			Row r = new Row (this, null, table, columnSet, previousID, objects);
			rows.Add (r.ObjectID, r);
			perTableRows [(Table)table].Add (r.ObjectID, r);
			Snapshot.AddVolatileData (r);
			RemovePreviousVersions (r);
			return r;
		}

		public void Flush ()
		{
			Snapshot.Database.SaveSnapshots (new Snapshot[] { Snapshot });
		}

	
		public override BD2.Frontend.Table.Model.Table GetTable (string name)
		{
			Table temp = new Table (this, null, name);
			if (tables.ContainsKey (temp.ObjectID)) {
				return tables [temp.ObjectID];
			}
			Snapshot.AddVolatileData (temp);
			tables.Add (temp.ObjectID, temp);
			perTableRows.Add (temp, new SortedDictionary<byte[], Row> (BD2.Common.ByteSequenceComparer.Shared));
			return temp;
		}

		public override IEnumerable<BD2.Frontend.Table.Model.Row> GetRows (BD2.Frontend.Table.Model.Table table)
		{
			return perTableRows [(Table)table].Values;
		}

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

		public override BD2.Frontend.Table.Model.Relation GetRelationByID (byte[] id)
		{
			if (!relations.ContainsKey (id))
				System.Diagnostics.Debugger.Break ();
			return relations [id];
		}

		public override IndexBase GetIndexByID (byte[] id)
		{
			if (!indices.ContainsKey (id))
				System.Diagnostics.Debugger.Break ();
			return indices [id];
		}

		public override IEnumerable<BD2.Frontend.Table.Model.Table> GetTables ()
		{
			return new SortedSet<BD2.Frontend.Table.Model.Table> (tables.Values);
		}

		public override IEnumerable<ColumnSet> GetColumnSets ()
		{
			return new SortedSet<ColumnSet> (columnSets.Values);
		}

		public override IEnumerable<BD2.Frontend.Table.Model.Row> GetRows ()
		{
			return new SortedSet<BD2.Frontend.Table.Model.Row> (rows.Values);
		}

		SortedDictionary<byte[], SortedSet<Row>> removedRows = new SortedDictionary<byte[], SortedSet<Row>> (BD2.Common.ByteSequenceComparer.Shared);

		void RemovePreviousVersions (Row row)
		{
			foreach (byte[] id in row.PreviousVersionID) {
				if (!rows.ContainsKey (id)) {
					//multiple updates to one row
					foreach (Row r in removedRows [id]) {
						r.SetCurrentVersion (row);
						row.SetCurrentVersion (r);
					}
				} else {
					Row pr = rows [id];
					row.SetPreviousVersion (pr);
					rows.Remove (id);
					if (removedRows.ContainsKey (pr.ObjectID))
						removedRows [pr.ObjectID].Add (row);
					else {
						SortedSet<Row> newSS = new SortedSet<Row> ();
						removedRows.Add (pr.ObjectID, newSS);
						newSS.Add (row);
					}
				}
			}
		}

		void FallbackToPreviousVersions (Row row)
		{
			foreach (byte[] pid in row.PreviousVersionID) {
				foreach (Row r in removedRows[pid]) {
					rows.Add (r.ObjectID, r);
				}
				removedRows.Remove (pid);
			}
		}

	
		public override void Purge (BaseDataObject bdo)
		{
			//TODO:make sure we remove all the related objects first, like the rows in a columnset or table
			if (!bdo.IsVolatile)
				throw new Exception ("BD2 does not and never will support purging non-volatile data.");
			if (bdo is Table) {
				tables.Remove (bdo.ObjectID);
			} else if (bdo is Column) {
				columns.Remove (bdo.ObjectID);
			} else if (bdo is ColumnSet) {
				columnSets.Remove (bdo.ObjectID);
			} else if (bdo is Row) {
				rows.Remove (bdo.ObjectID);
				FallbackToPreviousVersions ((Row)bdo);
			} else if (bdo is RowDrop) {
				Row R = (Row)((RowDrop)bdo).Row;
				rows.Add (R.ObjectID, R);
			}

		}


		public override BD2.Core.Transaction CreateTransaction ()
		{
			return new Transaction (this);
		}


		public override IEnumerable<BD2.Frontend.Table.Model.Row> GetRows (BD2.Frontend.Table.Model.Table table,
		                                                                   BD2.Frontend.Table.Model.ColumnSet columnSet,
		                                                                   BD2.Frontend.Table.Model.Column[] columns, object[] match)
		{
			int[] columnIndices = new int[columns.Length];
			for (int n = 0; n != columns.Length; n++) {
				columnIndices [n] = columnSet.IndexOf (columns [n]);
			}
			foreach (BD2.Frontend.Table.Model.Row r in GetRows (table)) {
				object[] fields = r.GetValues (columnSet);
				bool isMatch = true;
				for (int n = 0; n != columns.Length; n++) {
					if (fields [columnIndices [n]] != match [n]) {
						isMatch = false;
						continue;
					}
				}
				if (isMatch)
					yield return r;
			}
		}

		public override IEnumerable<BD2.Frontend.Table.Model.Relation> GetParentRelations (BD2.Frontend.Table.Model.Table table)
		{
			foreach (Relation rel in relations) {
				if (rel.ChildTable == table)
					yield return rel;
			}
		}

		readonly System.Collections.Generic.SortedDictionary<BD2.Frontend.Table.Model.Table, Func<BaseDataObject, byte[]>> tableSegmentSelectors
		= new SortedDictionary<BD2.Frontend.Table.Model.Table, Func<BaseDataObject, byte[]>> ();

		public void SetTableSegmentSelector (BD2.Frontend.Table.Model.Table table, Func<BaseDataObject, byte[]> function)
		{
			tableSegmentSelectors.Add (table, function);
		}

		public override byte[] GetObjectSegment (BaseDataObject baseDataObject)
		{
			var row = baseDataObject as Row;
			if (row != null) {
				return tableSegmentSelectors [row.Table] (baseDataObject);
			} else {
				var rowDrop = baseDataObject as RowDrop;
				if (rowDrop != null) {
					return tableSegmentSelectors [rowDrop.Row.Table] (baseDataObject);
				}
			}
			return base.GetObjectSegment (baseDataObject);
		}

		#endregion
	}
}
