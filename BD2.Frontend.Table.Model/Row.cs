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
using BD2.Common;

namespace BD2.Frontend.Table.Model
{
	public abstract class Row : BaseDataObject
	{
		ColumnSet columnSet;

		public ColumnSet ColumnSet {
			get {
				return columnSet;
			}
		}

		Table table;

		public Table Table {
			get {
				return table;
			}
		}

		protected Row (FrontendInstanceBase frontendInstanceBase, byte[] chunkID, Table table, ColumnSet columnSet)
			:base (frontendInstanceBase, chunkID)
		{
			if (table == null)
				throw new ArgumentNullException ("table");
			if (columnSet == null)
				throw new ArgumentNullException ("columnSet");
			this.columnSet = columnSet;
			this.table = table;
		}

		public abstract ValueSet GetValues ();

		public override IEnumerable<BaseDataObject> GetDependenies ()
		{
			yield return table;
			yield return columnSet;
			foreach (Relation rel in table.GetParentRelations ()) {
				foreach (Column relCol in rel.ChildColumns) {
					foreach (Column setCol in columnSet.Columns) {
						if (relCol == setCol)
							goto matchOne;
					}
					goto noMatch;
					matchOne:
					;
				}
				foreach (Row row in  rel.ParentColumns.Table.GetRows (rel.ParentColumns)) {
					yield return row;
				}
				noMatch:
				;
			}
		}
		#region implemented abstract members of Serializable
		public override void Serialize (System.IO.Stream stream)
		{
			using (System.IO.BinaryWriter BW  = new System.IO.BinaryWriter (stream)) {
				BW.Write (table.ObjectID);
				BW.Write (columnSet.ObjectID);
			}
		}
		#endregion
	}
}
