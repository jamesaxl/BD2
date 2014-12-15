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
using BD2.Frontend.Table;
using BD2.Core;

namespace BD2.Frontend.Table
{
	public class DataContext : BD2.Core.DataContext
	{
		SortedDictionary<Table, SortedDictionary<byte[], Row>> perTableRows = new SortedDictionary<Table, SortedDictionary<byte[], Row>> ();
		SortedDictionary<byte[], Row> rows = new SortedDictionary<byte[], Row> (ByteSequenceComparer.Shared);
		SortedDictionary<byte[], RowDrop> rowDrops = new SortedDictionary<byte[], RowDrop> (ByteSequenceComparer.Shared);

		SortedSet<byte[]> deletedObjects = new SortedSet<byte[]> (ByteSequenceComparer.Shared);

		public DataContext (FrontendInstanceBase frontendInstanceBase, DataContext baseContext)
			: base (frontendInstanceBase, baseContext)
		{

		}

		public  Row GetRowByID (byte[] id)
		{
			if (!rows.ContainsKey (id))
				System.Diagnostics.Debugger.Break ();
			return rows [id];
		}

		protected BaseDataObjectVersion GetObjectWithID (byte[] objectID)
		{
			//todo:use TryGetValue
			if (rows.ContainsKey (objectID))
				return rows [objectID];
			return null;
		}

		public Row UpdateRow (BaseDataObject baseDataObject)
		{
			throw new NotImplementedException ();
		}

		public Row CreateRow (Table table, IDictionary<int, ColumnSet> columnSets, IDictionary<int, object[]> objects)
		{
			BaseDataObject bdo = new BaseDataObject (FrontendInstanceBase, null);
			Row r = new Row (null, null, bdo, table, columnSets, objects);
			rows.Add (r.ID, r);
			perTableRows [table].Add (r.ID, r);
			return r;
		}

		public IEnumerable<Row> GetRows (Table table,
		                                 ColumnSet columnSet, int matchColumnSetID,
		                                 Column[] columns, object[] match)
		{
			int[] columnIndices = new int[columns.Length];
			for (int n = 0; n != columns.Length; n++) {
				columnIndices [n] = columnSet.IndexOf (columns [n]);
			}
			foreach (Row r in GetRows (table)) {
				object[] fields = r.GetValues (matchColumnSetID, columnSet);
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

		public void Flush ()
		{
			//Snapshot.Database.SaveSnapshots (new Snapshot[] { Snapshot });
		}


		public  IEnumerable<Row> GetRows (Table table)
		{
			return perTableRows [(Table)table].Values;
		}


		public IEnumerable<Row> GetRows ()
		{
			return new SortedSet<BD2.Frontend.Table.Row> (rows.Values);
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
