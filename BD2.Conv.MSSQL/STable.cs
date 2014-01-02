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
