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
using BD2.Core;
using System.Collections.Generic;
using BD2.Frontend.Table.Model;

namespace BD2.Frontend.Table
{
	public class Transaction:BD2.Core.Transaction, ITransactionSource
	{
		SortedDictionary<byte[], Row> rows = new SortedDictionary<byte[], Row> (BD2.Common.ByteSequenceComparer.Shared);
		SortedDictionary<byte[], Table> tables = new SortedDictionary<byte[], Table> (BD2.Common.ByteSequenceComparer.Shared);
		SortedDictionary<byte[], Relation> relations = new SortedDictionary<byte[], Relation> (BD2.Common.ByteSequenceComparer.Shared);
		SortedDictionary<byte[], Column> columns = new SortedDictionary<byte[], Column> (BD2.Common.ByteSequenceComparer.Shared);
		SortedDictionary<byte[], ColumnSet> columnSets = new SortedDictionary<byte[], ColumnSet> (BD2.Common.ByteSequenceComparer.Shared);
		SortedDictionary<byte[], RowDrop> rowDrops = new SortedDictionary<byte[], RowDrop> (BD2.Common.ByteSequenceComparer.Shared);
		SortedSet<byte[]> dropedRows = new SortedSet<byte[]> (BD2.Common.ByteSequenceComparer.Shared);

		ITransactionSource TransactionSource {
			get {
				return (ITransactionSource)base.TransactionSource;
			}
		}

		public Transaction (BD2.Core.ITransactionSource transactionSource)
			: base(transactionSource)
		{
		}
		#region implemented abstract members of Transaction
		public override void Commit ()
		{
			TransactionSource.CommitObjects (GetChanges ());
			Rollback ();
		}

		public override void Rollback ()
		{
			rows.Clear ();
			tables.Clear ();
			relations.Clear ();
			columns.Clear ();
			columnSets.Clear ();

		}

		public override System.Collections.Generic.IEnumerable<BD2.Core.BaseDataObject> GetChanges ()
		{
			foreach (var t in rows) {
				yield return t.Value;
			}
			foreach (var t in columns) {
				yield return t.Value;
			}
			foreach (var t in columnSets) {
				yield return t.Value;
			}
			foreach (var t in relations) {
				yield return t.Value;
			}
			foreach (var t in tables) {
				yield return t.Value;
			}
			foreach (var t in rowDrops) {
				yield return t.Value;
			}

		}
		#endregion
		#region ITransactionSource implementation
		public System.Collections.Generic.IEnumerable<Row> GetRows ()
		{
			foreach (var t in TransactionSource.GetRows ()) {
				if (!dropedRows.Contains (t.ObjectID))
					yield return t;
			}
			foreach (var t in rows) {
				if (!dropedRows.Contains (t.Key))
					yield return t.Value;
			}
		}

		public System.Collections.Generic.IEnumerable<Column> GetColumns ()
		{
			foreach (var t in TransactionSource.GetColumns ()) {
				yield return t;
			}
			foreach (var t in columns) {
				yield return t.Value;
			}
		}

		public System.Collections.Generic.IEnumerable<BD2.Frontend.Table.Model.ColumnSet> GetColumnSets ()
		{
			foreach (var t in TransactionSource.GetColumnSets ()) {
				yield return t;
			}
			foreach (var t in columnSets) {
				yield return t.Value;
			}
		}

		public System.Collections.Generic.IEnumerable<Table> GetTables ()
		{
			foreach (var t in TransactionSource.GetTables ()) {
				yield return t;
			}
			foreach (var t in tables) {
				yield return t.Value;
			}
		}

		public System.Collections.Generic.IEnumerable<Relation> GetRelations ()
		{
			foreach (var t in TransactionSource.GetRelations ()) {
				yield return t;
			}
			foreach (var t in relations) {
				yield return t.Value;
			}
		}
		#endregion
		public Transaction CreateTransaction ()
		{
			return new Transaction (this);
		}
	}
}

