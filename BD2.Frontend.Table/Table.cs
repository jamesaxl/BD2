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
 * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * */
using System;
using System.IO;
using BSO;
using System.Collections.Generic;
using BD2.Core;
using BD2.Frontend.Table.Model;

namespace BD2.Frontend.Table
{
	public class Table : Model.Table
	{
		SortedDictionary<Snapshot, Row> rows;

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

		SortedDictionary<Guid, Column> Columns = new SortedDictionary<Guid, Column> ();
		SortedDictionary<Guid, Row> Rows = new SortedDictionary<Guid, Row> ();

		internal Row GetRowByID (Guid ID)
		{
			return Rows [ID];
		}

		string name;

		public override string Name { get { return name; } }

		Snapshot snapshot;

		public Table (Snapshot Snapshot, Guid ID, string Name)
			:base()
		{
			snapshot = Snapshot;
			id = ID;
			name = Name;
		}

		internal Table (Snapshot Snapshot, System.IO.BinaryReader BR)
			:base()
		{
			snapshot = Snapshot;
			id = (Guid)BSO.Processor.DeserializeOne (BR, typeof(Guid), null);
			name = (string)BSO.Processor.DeserializeOne (BR, typeof(string), null);
		}

		public override void Serialize (System.IO.BinaryWriter BW)
		{
			base.Serialize (BW);
			BSO.Processor.SerializeOne (BW, id, typeof(Guid), null);
			BSO.Processor.SerializeOne (BW, name, typeof(string), null);
		}

		Guid id;

		public override Guid ObjectID {
			get {
				return id;
			}
		}

		internal System.Collections.Generic.SortedDictionary<Guid, System.Collections.Generic.SortedSet<Column>> ColumnSets;
		#region implemented abstract members of BaseDataObject
		public override BaseDataObject Drop ()
		{
			return new ObjectDrop (Guid.NewGuid (), this);
		}
		#endregion
		#region implemented abstract members of Table
		public override bool Alive {
			get {
				throw new System.NotImplementedException ();
			}
		}

		public override IEnumerable<Row> GetRows ()
		{
			foreach (Row Row in Rows.Values)
				yield return Row;
		}

		public override IEnumerable<Row> GetRows (Predicate<Row> Predicate)
		{
			throw new NotImplementedException ();
		}

		public override IEnumerable<Row> GetRows (IndexBase Index)
		{
			throw new System.NotImplementedException ();
		}

		public override IEnumerable<Row> GetRows (IndexBase Index, Predicate<Row> Predicate)
		{
			throw new System.NotImplementedException ();
		}

		public override IEnumerable<IndexBase> GetIndices ()
		{
			throw new NotImplementedException ();
		}

		public void AddRow (Row row);

		public void AddIndex (IndexBase Index);

		public override Snapshot Snapshot {
			get {
				return snapshot;
			}
		}
		#endregion
	}
}
