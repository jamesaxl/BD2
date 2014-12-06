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
using BD2.Frontend.Table.Model;
using BD2.Core;

namespace BD2.Frontend.Table
{
	public class DataContext : BD2.Core.DataContext
	{
		SortedDictionary<Table, SortedDictionary<byte[], Row>> perTableRows = new SortedDictionary<Table, SortedDictionary<byte[], Row>> ();
		SortedDictionary<byte[], Row> rows = new SortedDictionary<byte[], Row> (ByteSequenceComparer.Shared);
		SortedDictionary<byte[], Table> tables = new SortedDictionary<byte[], Table> (ByteSequenceComparer.Shared);
		SortedDictionary<byte[], Relation> relations = new SortedDictionary<byte[], Relation> (ByteSequenceComparer.Shared);
		SortedDictionary<byte[], Column> columns = new SortedDictionary<byte[], Column> (ByteSequenceComparer.Shared);
		SortedDictionary<byte[], ColumnSet> columnSets = new SortedDictionary<byte[], ColumnSet> (ByteSequenceComparer.Shared);
		SortedDictionary<byte[], Index> indices = new SortedDictionary<byte[], Index> (ByteSequenceComparer.Shared);
		SortedDictionary<byte[], RowDrop> rowDrops = new SortedDictionary<byte[], RowDrop> (ByteSequenceComparer.Shared);

		SortedSet<byte[]> deletedObjects = new SortedSet<byte[]> (ByteSequenceComparer.Shared);

		public DataContext (DataContext baseContext)
			: base (baseContext)
		{

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

		public  IEnumerable<BD2.Frontend.Table.Model.Row> GetRows (BD2.Frontend.Table.Model.Table table)
		{
			return perTableRows [(Table)table].Values;
		}

		public  ColumnSet GetColumnSetByID (byte[] id)
		{
			if (!columnSets.ContainsKey (id))
				System.Diagnostics.Debugger.Break ();
			return columnSets [id];
		}

		public  BD2.Frontend.Table.Model.Table GetTableByID (byte[] id)
		{
			if (!tables.ContainsKey (id))
				System.Diagnostics.Debugger.Break ();
			return tables [id];
		}

		public  BD2.Frontend.Table.Model.Column GetColumnByID (byte[] id)
		{
			if (!columns.ContainsKey (id))
				System.Diagnostics.Debugger.Break ();
			return columns [id];
		}

		public  BD2.Frontend.Table.Model.Row GetRowByID (byte[] id)
		{
			if (!rows.ContainsKey (id))
				System.Diagnostics.Debugger.Break ();
			return rows [id];
		}

		public  BD2.Frontend.Table.Model.Relation GetRelationByID (byte[] id)
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

	}
}
//
//public System.Collections.Generic.IEnumerable<Row> GetRows (BD2.Frontend.Table.Table table)
//{
//	foreach (var t in ((ITransactionSource)TransactionSource).GetRows (table)) {
//		if (!dropedRows.Contains (t.ObjectID))
//			yield return t;
//	}
//	foreach (var t in rows) {
//		if (!dropedRows.Contains (t.Key))
//		if (t.Value.Table == table)
//			yield return t.Value;
//	}
//}
//
//public System.Collections.Generic.IEnumerable<Row> GetRows ()
//{
//	foreach (var t in ((ITransactionSource)TransactionSource).GetRows ()) {
//		if (!dropedRows.Contains (t.ObjectID))
//			yield return t;
//	}
//	foreach (var t in rows) {
//		if (!dropedRows.Contains (t.Key))
//			yield return t.Value;
//	}
//}
//
//public System.Collections.Generic.IEnumerable<Column> GetColumns ()
//{
//	foreach (var t in ((ITransactionSource)TransactionSource).GetColumns ()) {
//		yield return t;
//	}
//	foreach (var t in columns) {
//		yield return t.Value;
//	}
//}
//
//public System.Collections.Generic.IEnumerable<BD2.Frontend.Table.Model.ColumnSet> GetColumnSets ()
//{
//	foreach (var t in ((ITransactionSource)TransactionSource).GetColumnSets ()) {
//		yield return t;
//	}
//	foreach (var t in columnSets) {
//		yield return t.Value;
//	}
//}
//
//public Table GetTable (string name)
//{
//	Table t = ((ITransactionSource)TransactionSource).GetTable (name);
//	if (t != null)
//		return t;
//	foreach (var tup in tables)
//		if (tup.Value.Name == name) {
//			return tup.Value;
//		}
//	return null;
//}
//
//public System.Collections.Generic.IEnumerable<Table> GetTables ()
//{
//	foreach (var t in ((ITransactionSource)TransactionSource).GetTables ()) {
//		yield return t;
//	}
//	foreach (var t in tables) {
//		yield return t.Value;
//	}
//}
//
//public System.Collections.Generic.IEnumerable<Relation> GetRelations ()
//{
//	foreach (var t in ((ITransactionSource)TransactionSource).GetRelations ()) {
//		yield return t;
//	}
//	foreach (var t in relations) {
//		yield return t.Value;
//	}
//}
//
//
