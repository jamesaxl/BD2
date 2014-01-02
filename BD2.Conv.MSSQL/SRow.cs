//
//  STable.cs
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
using System.Collections.Generic;

namespace BD2.Conv.MSSQL
{
	internal class SRow : SRowBase
	{
		int mandatorySet = 0;

		public int MandatorySet {
			get {
				return mandatorySet;
			}
		}

		public int MandatoryRemaining{ get { return table.MandatoryColumns - mandatorySet; } }

		readonly SortedDictionary<SColumn, object> values = new  SortedDictionary<SColumn, object> ();
		readonly STable table;

		public override STableBase Table {
			get {
				return table;
			}
		}

		internal void SetValue (SColumn column, object value)
		{
			if (column == null)
				throw new ArgumentNullException ("column");
			if (!table.HasColumn (column))
				throw new ArgumentException ("the column must belong to the table.");
			if (column.Mandatory) {
				if ((value == null) || (value is DBNull)) {
					throw new ArgumentNullException ("value", "argument is mandatory");
				}
			} else {
				if (value == null) {
					throw new ArgumentNullException ("value", "Consider using DBNull.Value if this is desired.");
				}
			}
			lock (values) {
				if (values.ContainsKey (column)) {
					throw new InvalidOperationException ("Cannot add multiple values for a single column.");
				}
				values.Add (column, value);
				if (column.Mandatory)
					mandatorySet++;
			}
		}

		public override object GetValue (SColumnBase column)
		{
			if (column == null)
				throw new ArgumentNullException ("column");
			if (!(column is SColumn))
				throw new ArgumentException ("column must be fo type BD2.Conv.MSSQL.SColumn");
			lock (values) {
				object rv;
				if (values.TryGetValue ((SColumn)column, out rv))
					return rv;
				throw new InvalidOperationException (string.Format ("This row has no data associated with column {0}", column));
			}
		}

		public SRow (STable table)
		{
			if (table == null)
				throw new ArgumentNullException ("table");
			this.table = table;
		}

		public override string ToString ()
		{
			return string.Format ("[SRow: MandatorySet={0}, MandatoryRemaining={1}, Table={2}]", MandatorySet, MandatoryRemaining, Table);
		}
	}
}

