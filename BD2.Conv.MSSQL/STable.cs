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
	public class STable : STableBase, IComparable, ICloneable
	{
		const int DefaultRowCount = 1024;
		const int DefaultColumnCount = 16;
		readonly List<SRow> rows = new List<SRow> (DefaultRowCount);
		readonly List<SColumn> columns = new List<SColumn> (DefaultColumnCount);
		readonly int id;
		readonly string name;
		readonly bool caseSensitive;
		int mandatoryColumns = 0;

		/// <summary>
		/// expensive
		/// </summary>
		/// <returns>Get column descriptor object by it's name.</returns>
		/// <param name="columnName">The name of the column to search for.</param>
		public override SColumnBase GetColumnByName (string columnName)
		{
			if (columnName == null)
				throw new ArgumentNullException ("columnName");
			lock (columns) {
				foreach (SColumn sc in columns) {
					if (sc.Name == columnName) {
						return sc;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// expensive, character case insensitive
		/// </summary>
		/// <returns>Get column descriptor object by it's name.</returns>
		/// <param name="columnName">The name of the column to search for.</param>
		public override SColumnBase GetColumnByNameInsensitive (string columnName)
		{
			if (columnName == null)
				throw new ArgumentNullException ("columnName");
			columnName = columnName.ToUpper ();//someone once told me it's cheaper than ToLower.
			lock (columns) {
				foreach (SColumn sc in columns) {
					if (sc.Name.ToUpper () == columnName) {
						return sc;
					}
				}
			}
			return null;
		}

		public override bool HasColumn (SColumnBase column)
		{
			if (column == null)
				throw new ArgumentNullException ("column");
			if (!(column is SColumn))
				throw new ArgumentException ("column must be of type BD2.Conv.MSSQL.SColumn");
			lock (columns) {
				return columns.Contains ((SColumn)column);
			}
		}

		public int MandatoryColumns {
			get {
				return mandatoryColumns;
			}
		}

		public int Id {
			get {
				return id;
			}
		}

		public override string Name {
			get {
				return name;
			}
		}

		public override bool CaseSensitive {
			get {
				return caseSensitive;
			}
		}

		public STable (int id, string name, bool caseSensitive)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			this.id = id;
			this.name = name;
			this.caseSensitive = caseSensitive;
		}

		public SColumn AddColumn (string name, bool mandatory)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			SColumn instance;
			lock (columns) {
				string ucaseName = name.ToUpper ();
				foreach (SColumn sc in columns) {
					if ((sc.Name == name) || (caseSensitive && (sc.Name.ToUpper () == ucaseName))) {
						throw new InvalidOperationException (string.Format ("A column named {0} already exists in the table.", sc.Name));
					}
				}
				if (mandatory)
					mandatoryColumns++;
				instance = new SColumn (name, mandatory);
				columns.Add (instance);
			}
			return instance;
		}

		internal SRow AddRow ()
		{
			SRow row = new SRow (this);
			lock (rows)
				rows.Add (row);
			return row;
		}

		public override string ToString ()
		{
			return string.Format ("[STable: MandatoryColumns={0}, Name={1}]", MandatoryColumns, Name);
		}

		public override int CompareTo (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			STable st = obj as STable;
			if (st == null)
				throw new InvalidOperationException ("obj must be of type STable.");
			return string.Compare (st.name, name, StringComparison.Ordinal);
		}

		#region ICloneable implementation

		/// <summary>
		/// REALLY DAMN EXPENSIVE
		/// </summary>
		public object Clone ()
		{
			STable clone = new  STable (id, name, caseSensitive);
			SortedDictionary<SColumn,SColumn> clonedColumns = new SortedDictionary<SColumn, SColumn> ();
			foreach (SColumn sc in columns) {
				clonedColumns.Add (sc, clone.AddColumn (sc.Name, sc.Mandatory));
			}
			foreach (SRow row in rows) {
				SRow clonedRow = clone.AddRow ();
				foreach (SColumn sc in columns) {
					clonedRow.SetValue (clonedColumns [sc], row.GetValue (sc));
				}
			}
			return clone;
		}

		#endregion

	}
}
