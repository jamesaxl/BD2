//
//  ColumnSet.cs
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

namespace BD2.Frontend.Table
{
	public class ColumnSet : Model.ColumnSet
	{
		Table table;
		public ColumnSet (Table Table)
		{
			table = Table;
		}
		#region implemented abstract members of BSO.ColumnSet
		public override Table Table {
			get {
				return table;
			}
		}
		Column[] columns;
		public override System.Collections.Generic.IEnumerable<Column> GetColumns ()
		{
			return columns.GetEnumerator();
		}
		public override object[] FromRaw (byte[] Raw)
		{
			this.OffsetHandler.GetOffsetMapFor (true, columns, null);
		}
		public override object[] FromRawStream (System.IO.Stream Raw)
		{
			throw new System.NotImplementedException ();
		}
		public override byte[] ToRaw (object[] Objects)
		{
			throw new System.NotImplementedException ();
		}
		public override int ToRawStream (object[] Objects, System.IO.Stream Stream)
		{
			throw new System.NotImplementedException ();
		}

		public override void Retrieve (Column Column)
		{
			throw new System.NotImplementedException ();
		}
		#endregion

	}
}

