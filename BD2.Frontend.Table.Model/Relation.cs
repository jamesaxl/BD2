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

namespace BD2.Frontend.Table.Model
{
	public abstract class Relation : BaseDataObjectVersion
	{
		public override IEnumerable<BaseDataObject> GetDependenies ()
		{
			yield return parentColumns;
			yield return childTable;
			foreach (Column c in childColumns) {
				yield return c;
			}
			foreach (BaseDataObject bdo in  base.GetDependenies ()) {
				yield return bdo;
			}
		}

		IndexBase parentColumns;
		Column[] childColumns;
		ColumnSet childColumnSet;
		Table childTable;
		//it's purpose is to allow for relations to cross ColumnSets without one relation being processed twice
		string name;

		public string Name {
			get {
				return name;
			}
		}

		protected Relation (FrontendInstanceBase frontendInstanceBase, byte[] chunkID, string name, IndexBase parentColumns, Table childTable, ColumnSet childColumnSet, Column[] childColumns)
			: base (frontendInstanceBase, chunkID)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (parentColumns == null)
				throw new ArgumentNullException ("parentColumns");
			if (childTable == null)
				throw new ArgumentNullException ("childTable");
			if (childColumnSet == null)
				throw new ArgumentNullException ("childColumnSet");
			if (childColumns == null)
				throw new ArgumentNullException ("childColumns");
			this.name = name;
			this.parentColumns = parentColumns;
			this.childTable = childTable;
			this.childColumnSet = childColumnSet;
			this.childColumns = childColumns;
		}

		public IndexBase ParentColumns { get { return parentColumns; } }

		public Table ChildTable { get { return childTable; } }

		public Column[] ChildColumns { get { return childColumns; } }

		public ColumnSet ChildColumnSet {
			get {
				return childColumnSet;
			}
		}

		#region implemented abstract members of Serializable

		public override void Serialize (System.IO.Stream stream)
		{
			using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (stream)) {
				BW.Write (parentColumns.ObjectID);
				BW.Write (childTable.ObjectID);
				BW.Write (childColumns.Length);
				for (int n = 0; n != childColumns.Length; n++) {
					BW.Write (childColumns [n].ObjectID);
				}
			}
		}

		#endregion

		public abstract IEnumerable<Row> GetChildRows (Row parent);

		public abstract Row GetParentRow (Row child);
	}
}
