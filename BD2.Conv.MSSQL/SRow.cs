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

