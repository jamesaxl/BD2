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
using System.IO;
using BD2.Core;

namespace BD2.Frontend.Table.Model
{
	[BaseDataObjectTypeIdAttribute ("bca848f2-70b8-476d-86af-77cde2fdc5fd", typeof(ColumnSet), "Deserialize")]
	public sealed class ColumnSet : BaseDataObjectVersion
	{
		readonly Column[] columns;

		public Column[] Columns {
			get {
				return columns;
			}
		}

		public ColumnSet (FrontendInstanceBase frontendInstanceBase, byte[] chunkID, Column[] columns)
			: base (frontendInstanceBase, chunkID)
		{
			if (columns == null)
				throw new ArgumentNullException ("columns");
			this.columns = columns;
		}

		#region implemented abstract members of Serializable

		public static BaseDataObject Deserialize (FrontendInstanceBase fib, byte[] chunkID, byte[] buffer)
		{
			using (System.IO.MemoryStream MS = new MemoryStream (buffer)) {
				using (System.IO.BinaryReader BR = new BinaryReader (MS)) {
					int columnCount = BR.ReadInt32 ();
					Column[] columns = new Column[columnCount];
					for (int n = 0; n != columnCount; n++) {
						columns [n] = ((FrontendInstance)fib).GetColumnByID (BR.ReadBytes (32));
					}
					return new ColumnSet (fib, chunkID, columns);
				}
			}
		}

		public override void Serialize (Stream stream)
		{
			using (BinaryWriter BW = new BinaryWriter (stream)) {
				BW.Write (columns.Length);
				for (int n = 0; n != columns.Length; n++) {
					BW.Write (columns [n].ObjectID);
				}
			}
		}

		#endregion

		#region implemented abstract members of BaseDataObject

		public override IEnumerable<BaseDataObject> GetDependenies ()
		{
			foreach (Column column in columns)
				yield return column;
		}

		#endregion

		#region implemented abstract members of BaseDataObject

		public override Guid ObjectType {
			get {
				return Guid.Parse ("bca848f2-70b8-476d-86af-77cde2fdc5fd");
			}
		}

		#endregion

		public byte[] SerializeObjects (object[] data)
		{
			return ((FrontendInstance)FrontendInstanceBase).ValueSerializer.SerializeArray (data);
		}

		public object[] DeserializeObjects (byte[] data)
		{
			return ((FrontendInstance)FrontendInstanceBase).ValueSerializer.DeserializeArray (data);
		}

		public int IndexOf (string fieldName, StringComparison comparisonType)
		{
			for (int n = 0; n != columns.Length; n++) {
				if (columns [n].Name.Equals (fieldName, comparisonType))
					return n;
			}
			return -1;
		}

		public int IndexOf (Column column)
		{
			for (int n = 0; n != columns.Length; n++) {
				if (columns [n] == column)
					return n;
			}
			return -1;
		}
	}
}