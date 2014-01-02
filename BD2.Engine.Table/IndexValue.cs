//
//  IndexValue.cs
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
using BSO;
using System.Collections.Generic;

namespace BD2.Frontend.Table
{
	public sealed class IndexValue : IComparable
	{
		public int CompareTo (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			IndexValue OtherIV = obj as IndexValue;
			if (OtherIV == null)
				throw new ArgumentException ("obj must be of type IndexValue.", "obj");
			if(!OtherIV.index.Equals(index))
				throw new ArgumentException ("obj must have the same index signature", "obj");
			DCBase OtherRow = OtherIV.row;
			IEnumerator<IndexColumnBase> OtherIndices = OtherIV.index.GetIndexColumns ();
			IEnumerator<IndexColumnBase> ThisIndices = this.index.GetIndexColumns ();
			while(ThisIndices.MoveNext () && OtherIndices.MoveNext ()) {
				int CompareValue;
				CompareValue = row.GetValue (ThisIndices.Current.Column).CompareTo(OtherRow.GetValue(OtherIndices.Current.Column));
				if (CompareValue == 0) {
					continue;
				}
				if (ThisIndices.Current.SortAscending) {
					return CompareValue;
				} else {
					return -CompareValue;
				}
			}
			return 0;
		}
		IndexBase index;
		public IndexBase Index {
			get {
				return index;
			}
		}
		DCBase row;
		public DCBase Row {
			get {
				return row;
			}
		}
		public IndexValue(IndexBase Index, DCBase Row)
		{
			if (Row == null)
				throw new ArgumentNullException ("Row");
			if (Index == null)
				throw new ArgumentNullException ("Index");
			row = Row;
			index = Index;
		}
	}	
}

