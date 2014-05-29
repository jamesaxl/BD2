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

namespace BD2.Frontend.Table
{
	[BaseDataObjectTypeIdAttribute("3be06a16-6727-4639-b702-060b522af660", typeof(Table), "Deserialize")]
	public class Table : Model.Table
	{
		public class Comparer : IComparer<Table>
		{
			public int Compare (Table x, Table y)
			{
				int hashComp = x.GetHashCode ().CompareTo (y.GetHashCode ());
				if (hashComp != 0)
					return hashComp;
				throw new NotImplementedException ();
			}
		}

		SortedDictionary<byte[], Row> Rows = new SortedDictionary<byte[], Row> (new BD2.Common.ByteSequenceComparer ());
		SortedDictionary<IndexBase, SortedDictionary<byte[], Row>> rowsByIndices = new SortedDictionary<IndexBase, SortedDictionary<byte[], Row>> ();

		internal Row GetRowByID (byte[] id)
		{
			return Rows [id];
		}

		string name;

		public override string Name { get { return name; } }

		public Table (FrontendInstanceBase frontendInstanceBase, byte[] chunkID, string name)
			:base(frontendInstanceBase, chunkID)
		{
			this.name = name;
		}

		internal void InsertColumnSet (ColumnSet columnSet)
		{
			columnSets.Add (columnSet.ObjectID, columnSet);
		}

		SortedDictionary<byte[], ColumnSet> columnSets = new SortedDictionary<byte[], ColumnSet> (new BD2.Common.ByteSequenceComparer ());

		internal SortedDictionary<byte[], ColumnSet> ColumnSets {
			get {
				return new SortedDictionary<byte[], ColumnSet> (columnSets);
			}
		}

		SortedDictionary<byte[], BD2.Frontend.Table.Model.Relation> relations = new SortedDictionary<byte[], BD2.Frontend.Table.Model.Relation> (new BD2.Common.ByteSequenceComparer ());

		public SortedDictionary<byte[], BD2.Frontend.Table.Model.Relation> Relations {
			get {
				return new SortedDictionary<byte[], BD2.Frontend.Table.Model.Relation> (relations);
			}
		}
		#region implemented abstract members of Serializable
		public override void Serialize (System.IO.Stream stream)
		{
			using (System.IO.BinaryWriter BW  = new System.IO.BinaryWriter (stream)) {
				BW.Write (name);
			}
		}
		#endregion
		#region implemented abstract members of BaseDataObject
		public override IEnumerable<BaseDataObject> GetDependenies ()
		{
			foreach (ColumnSet cs in columnSets.Values)
				yield return cs;
			yield break;
		}

		public override Guid ObjectType {
			get {
				return Guid.Parse ("3be06a16-6727-4639-b702-060b522af660");
			}
		}
		#endregion
		#region implemented abstract members of Table
		public override IEnumerable<BD2.Frontend.Table.Model.Relation> GetParentRelations ()
		{
			return Relations.Values;
		}

		public override IEnumerable<BD2.Frontend.Table.Model.Row> GetRows ()
		{
			foreach (KeyValuePair<byte[], Row> r in Rows) {
				yield return r.Value;
			}
		}

		public override IEnumerable<BD2.Frontend.Table.Model.Row> GetRows (IndexBase index)
		{
			return rowsByIndices [index].Values;
		}

		public override IEnumerable<IndexBase> GetIndices ()
		{
			return rowsByIndices.Keys;
		}

		public override IEnumerable<ColumnSet> GetColumnSets ()
		{
			return columnSets.Values;
		}
		#endregion
		#region implemented abstract members of Table
		public override IEnumerable<BD2.Frontend.Table.Model.Row> GetRows (ColumnSet columnSet)
		{
			foreach (Row r in Rows.Values) {
				if (r.ColumnSet == columnSet) {
					yield return r;
				}
			}
		}

		public override IEnumerable<BD2.Frontend.Table.Model.Row> GetRows (ColumnSet columnSet, IndexBase index)
		{
			foreach (Row r in rowsByIndices[index].Values) {
				if (r.ColumnSet == columnSet) {
					yield return r;
				}
			}
		}
		#endregion
	}
}
