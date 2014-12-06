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
using BD2.Frontend.Table.Model;

namespace BD2.Frontend.Table
{
	public class FrontendInstance : BD2.Frontend.Table.Model.FrontendInstance
	{
		ValueSerializerBase valueSerializer;
		//SortedDictionary<byte[], BaseDataObject> volatileData = new SortedDictionary<byte[], BaseDataObject> (BD2.Common.ByteSequenceComparer.Shared);
		public override ValueSerializerBase ValueSerializer {
			get {
				return valueSerializer;
			}
		}

		public FrontendInstance (Snapshot snapshot, Frontend frontend, ValueSerializerBase valueSerializer) :
			base (snapshot, frontend)
		{
			if (valueSerializer == null)
				throw new ArgumentNullException ("valueSerializer");
			this.valueSerializer = valueSerializer;
		}

		#region implemented abstract members of FrontendInstance

		protected override void OnCreateObjects (byte[] chunkID, byte[] bytes)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (bytes)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					int objectCount = BR.ReadInt32 ();
					Console.WriteLine ("Deserializing {0} objects", objectCount);
					for (int n = 0; n != objectCount; n++) {
						int objectLengeth = BR.ReadInt32 ();
						//Console.WriteLine ("Length:{0}", objectLengeth);
						Guid objectTypeID = new Guid (BR.ReadBytes (16));
						BaseDataObjectTypeIdAttribute typeDescriptor = BaseDataObjectTypeIdAttribute.GetAttribFor (objectTypeID);
						InsertObject (typeDescriptor.Deserialize (this, chunkID, BR.ReadBytes (objectLengeth - 16)));
					}
				}
			}
		}

		void InsertObject (BaseDataObject bdo)
		{
			if (bdo is Table) {
				tables.Add (bdo.ObjectID, (Table)bdo);
				perTableRows.Add ((Table)bdo, new SortedDictionary<byte[], Row> (BD2.Common.ByteSequenceComparer.Shared));
			} else if (bdo is Column) {
				columns.Add (bdo.ObjectID, (Column)bdo);
			} else if (bdo is ColumnSet) {
				columnSets.Add (bdo.ObjectID, (ColumnSet)bdo);
			} else if (bdo is Row) {
				rows.Add (bdo.ObjectID, (Row)bdo);
				RemovePreviousVersions ((Row)bdo);
			}
		}

		 
		public override void Purge (BaseDataObject bdo)
		{
			//TODO:make sure we remove all the related objects first, like the rows in a columnset or table
			if (!bdo.IsVolatile)
				throw new Exception ("BD2 does not and never will support purging non-volatile data.");
			if (bdo is Table) {
				tables.Remove (bdo.ObjectID);
			} else if (bdo is Column) {
				columns.Remove (bdo.ObjectID);
			} else if (bdo is ColumnSet) {
				columnSets.Remove (bdo.ObjectID);
			} else if (bdo is Row) {
				rows.Remove (bdo.ObjectID);
				FallbackToPreviousVersions ((Row)bdo);
			} else if (bdo is RowDrop) {
				Row R = (Row)((RowDrop)bdo).Row;
				rows.Add (R.ObjectID, R);
			}

		}


		public override BD2.Core.Transaction CreateTransaction ()
		{
			return new Transaction (this);
		}


		public override IEnumerable<BD2.Frontend.Table.Model.Row> GetRows (BD2.Frontend.Table.Model.Table table,
		                                                                   BD2.Frontend.Table.Model.ColumnSet columnSet,
		                                                                   BD2.Frontend.Table.Model.Column[] columns, object[] match)
		{
			int[] columnIndices = new int[columns.Length];
			for (int n = 0; n != columns.Length; n++) {
				columnIndices [n] = columnSet.IndexOf (columns [n]);
			}
			foreach (BD2.Frontend.Table.Model.Row r in GetRows (table)) {
				object[] fields = r.GetValues (columnSet);
				bool isMatch = true;
				for (int n = 0; n != columns.Length; n++) {
					if (fields [columnIndices [n]] != match [n]) {
						isMatch = false;
						continue;
					}
				}
				if (isMatch)
					yield return r;
			}
		}

		public override IEnumerable<BD2.Frontend.Table.Model.Relation> GetParentRelations (BD2.Frontend.Table.Model.Table table)
		{
			foreach (var rel in relations) {
				if (rel.Value.ChildTable == table)
					yield return rel.Value;
			}
		}


		#endregion
	}
}
