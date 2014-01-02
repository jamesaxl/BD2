//
//  Table.cs
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
using System.IO;
using BSO;
using System.Collections.Generic;
using BD2.Common;
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
				int hashComp = x.GetHashCode ().CompareTo (y.GetHashCode());
				if (hashComp != 0)
					return hashComp;
				throw new NotImplementedException ();
			}
		}
		SortedDictionary<Guid, Column> Columns = new SortedDictionary<Guid, Column> ();
		SortedDictionary<Guid, Row> Rows = new SortedDictionary<Guid, Row> ();
		internal Row GetRowByID(Guid ID)
		{
			return Rows[ID];
		}
		string name; public override string Name { get { return name; } }
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
			base.Serialize(BW);
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
			return new ObjectDrop(Guid.NewGuid(), this);
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
			foreach(Row Row in Rows.Values)
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
		public void AddIndex(IndexBase Index);

		public override Snapshot Snapshot {
			get {
				return snapshot;
			}
		}

		#endregion
	}
}
