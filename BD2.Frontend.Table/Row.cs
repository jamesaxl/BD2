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
using BD2.Frontend.Table;
using System.Security.Cryptography;

namespace BD2.Frontend.Table
{
	[BaseDataObjectTypeIdAttribute ("10ec2d31-3291-43ae-96fe-da8537b22af6", typeof(Row), "Deserialize")]
	public sealed class Row : BaseDataObjectVersion
	{
		readonly IDictionary<int, ColumnSet> columnSets;

		public IDictionary<int, ColumnSet> ColumnSets {
			get {
				return columnSets;
			}
		}

		readonly Table table;

		public Table Table {
			get {
				return table;
			}
		}



		public object[] GetValues (int columnSetID, ColumnSet outputColumnSet)
		{
			return ((FrontendInstance)BaseDataObject.FrontendInstanceBase).GetColumnSetConverter (columnSets [columnSetID], outputColumnSet).Convert (GetValues (columnSetID), columnSets [columnSetID], outputColumnSet);
		}

		public override IEnumerable<BaseDataObjectVersion> GetDependenies ()
		{
			yield break;
		}

		IDictionary<int, object[]> data;

		public object GetRawDataClone (int columnSetIndex)
		{
			return data [columnSetIndex].Clone ();
		}

		internal Row (byte[] id, byte[] chunkID, BaseDataObject baseDataObject, Table table, IDictionary<int, ColumnSet> columnSets, IDictionary<int, object[]> data)
			: base (id, chunkID, baseDataObject)
		{
			if (table == null)
				throw new ArgumentNullException ("table");
			if (columnSets == null)
				throw new ArgumentNullException ("columnSets");
			if (data == null)
				throw new ArgumentNullException ("data");
			this.table = table;
			this.columnSets = columnSets;
			this.data = data;
		}

		#region implemented abstract members of Serializable

		public static Row Deserialize (FrontendInstanceBase fib, byte[] chunkID, BaseDataObject baseDataObject, byte[] buffer)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (buffer)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					byte[] id = BR.ReadBytes (32);
					Table table = ((FrontendInstance)fib).GetTableByID (BR.ReadBytes (32));
					byte columnSetCount = BR.ReadByte ();
					Dictionary<int, ColumnSet> columnSets = new Dictionary<int, ColumnSet> ();
					for (int columnSetIndex = 0; columnSetIndex != columnSetCount; columnSetIndex++) {
						ColumnSet columnSet = ((FrontendInstance)fib).GetColumnSetByID (BR.ReadBytes (32));
					
					}
					int previousVersionCount = BR.ReadInt32 ();
					Dictionary<int, object[]> objs = new Dictionary<int, object[]> ();
					for (int n = 0; n != columnSetCount; n++)
						objs.Add (n, columnSets [n].DeserializeObjects (BR.ReadBytes (BR.ReadInt32 ())));
					Row R = new Row (id,
						        chunkID,
						        baseDataObject,
						        table,
						        columnSets,
						        objs);
					return R;
				}
			}
		}

		public override void Serialize (System.IO.Stream stream, EncryptedStorageManager encryptedStorageManager)
		{
			using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (stream)) {
				BW.Write (ID);
				BW.Write (Table.ObjectID, 0, 32);
				BW.Write ((byte)columnSets.Count);
				foreach (var columnSet in columnSets)
					BW.Write (columnSet.Value.ObjectID, 0, 32);
				for (int index = 0; index != columnSets.Count; index++) {
					byte[] buf = ColumnSets [index].SerializeObjects (data [index]);
					BW.Write (buf.Length);
					BW.Write (buf);
				}
			}
		}

		#endregion

		#region implemented abstract members of Row

		public object[] GetValues (int columnSetIndex)
		{
			return data [columnSetIndex];
		}

		public object GetValue (int columnSetIndex, string fieldName)
		{
			return data [columnSets [columnSetIndex].IndexOf (fieldName, StringComparison.Ordinal)];
		}

		public object GetValue (int fieldIndex)
		{
			return data [fieldIndex];
		}

		public IEnumerable<KeyValuePair<Column, object>> GetValuesWithColumns (int columnSetID)
		{
			int n = 0;
			foreach (Column col in ColumnSets[columnSetID].Columns) {
				yield return new KeyValuePair<Column, object> (col, data [n]);
				n++;
			}
		}

		#endregion

		#region implemented abstract members of BaseDataObject

		public override Guid ObjectType {
			get {
				return Guid.Parse ("10ec2d31-3291-43ae-96fe-da8537b22af6");
			}
		}

		#endregion
	}
}
