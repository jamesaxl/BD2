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
	[BaseDataObjectTypeIdAttribute("10ec2d31-3291-43ae-96fe-da8537b22af6", typeof(Row), "Deserialize")]
	public sealed class Row : Model.Row
	{
		object[] data;

		public object GetRawDataClone ()
		{
			return data.Clone ();
		}

		internal Row (FrontendInstanceBase  frontendInstanceBase, byte[] chunkID, Model.Table table, ColumnSet columnSet, object[] data)
			: base(frontendInstanceBase, chunkID, table, columnSet)
		{
			if (table == null)
				throw new ArgumentNullException ("table");
			if (columnSet == null)
				throw new ArgumentNullException ("columnSet");
			if (data == null)
				throw new ArgumentNullException ("data");
			this.data = data;
		}
		#region implemented abstract members of Serializable
		public static Row Deserialize (FrontendInstanceBase fib, byte[] chunkID, byte[] buffer)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (buffer)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					Table table = ((BD2.Frontend.Table.FrontendInstance)fib).GetTableByID (BR.ReadBytes (32));
					ColumnSet columnSet = ((BD2.Frontend.Table.FrontendInstance)fib).GetColumnSetByID (BR.ReadBytes (32));
					return new Row (fib, 
					                chunkID,
					                table,
					                columnSet,
					                columnSet.DeserializeObjects (BR.ReadBytes (BR.ReadInt32 ())));
				}
			}
		}

		public override void Serialize (System.IO.Stream stream)
		{
			using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (stream)) {
				BW.Write (Table.ObjectID, 0, 32);
				BW.Write (ColumnSet.ObjectID, 0, 32);
				byte[] buf = ColumnSet.SerializeObjects (data);
				BW.Write (buf.Length);
				BW.Write (buf);
			}
		}
		#endregion
		#region implemented abstract members of Row
		public override object[] GetValues ()
		{
			return data;
		}

		public override IEnumerable<KeyValuePair<BD2.Frontend.Table.Model.Column, object>> GetValuesWithColumns ()
		{
			int n = 0;
			foreach (BD2.Frontend.Table.Model.Column col in this.ColumnSet.Columns) {
				yield return new KeyValuePair<BD2.Frontend.Table.Model.Column, object> (col, data [n]);
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

		public override IEnumerable<BaseDataObject> GetDependenies ()
		{
			return base.GetDependenies ();
		}
		#endregion
	}
}
