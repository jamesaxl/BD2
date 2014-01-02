//
//  IndexColumnBase.cs
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
using BD2.Common;

namespace BD2.Frontend.Table.Model
{
	public abstract class IndexColumnBase : BaseDataObject
	{
		Column column;
		bool sortAscending;
		int HashCode;
		public Column Column { get { return column; } }
		public bool SortAscending { get { return sortAscending; } }
		public override int GetHashCode () { return HashCode; }
		public IndexColumnBase(Column Column, bool SortAscending = true)
		{
			if (Column == null)
				throw new ArgumentNullException ("Column");
			column = Column;
			sortAscending = SortAscending;
			HashCode = column.GetHashCode ();
			if (!sortAscending) {
				HashCode ^= 0x12345678; 
			}
		}
		public override bool Equals (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			IndexColumnBase OtherIndexColumn = obj as IndexColumnBase;
			if (OtherIndexColumn == null)
				throw new ArgumentException ("obj must be of type IndexColumn.", "obj");
			return  (OtherIndexColumn.sortAscending == this.sortAscending) && (OtherIndexColumn.column.TypeEquals(this.column));
		}
	}
}
