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
	[BaseDataObjectTypeIdAttribute("aa8d2cde-5ab2-4656-b172-510d64f831ab", typeof(Index), "Deserialize")]
	public sealed class Index : IndexBase
	{
		#region implemented abstract members of IndexBase
		public override int GetColumnCount ()
		{
			return indexColumns.Length;
		}
		#endregion
		IndexColumnBase[] indexColumns;

		public override IEnumerable<IndexColumnBase> GetIndexColumns ()
		{
			foreach (IndexColumnBase indexColumn in indexColumns)
				yield return indexColumn;
		}

		public Index (FrontendInstanceBase frontendInstanceBase, byte[] chunkID, Model.Table table, bool unique, IndexColumnBase[] indexColumns)
		:base(frontendInstanceBase, chunkID, table, unique)
		{
			if (indexColumns == null)
				throw new ArgumentNullException ("indexColumn");
			foreach (IndexColumnBase IC in indexColumns) {
				if (IC == null)
					throw new ArgumentException ("IndexColumn must not contain null enteries.", "IndexColumn");
			}
			this.indexColumns = ((IndexColumnBase[])indexColumns.Clone ());
		}
		#region implemented abstract members of Serializable
		public static Index Deserialize (FrontendInstanceBase frontendInstanceBase, byte[] chunkID, byte[] buffer)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (buffer)) {
				Model.Table table;
				bool unique;
				BD2.Frontend.Table.Model.IndexBase.Deserialize ((BD2.Frontend.Table.Model.FrontendInstance)frontendInstanceBase, MS, out table, out unique);
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					IndexColumnBase[] indexColumns = new IndexColumnBase[BR.ReadInt32 ()];
					for (int n = 0; n != indexColumns.Length; n++) {
						indexColumns [n] = IndexColumn.Deserialize ((BD2.Frontend.Table.Model.FrontendInstance)frontendInstanceBase, BR.ReadBytes (BR.ReadInt32 ()));
					}
					return new Index (frontendInstanceBase, chunkID, table, unique, indexColumns);
				}
			}
		}

		public override void Serialize (System.IO.Stream stream)
		{
			base.Serialize (stream);
			using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (stream)) {
				BW.Write (indexColumns.Length);
				for (int n = 0; n != indexColumns.Length; n++) {
					byte[] buf = indexColumns [n].Serialize ();
					BW.Write (buf.Length);
					BW.Write (buf);
				}
			}
		}
		#endregion
		#region implemented abstract members of BaseDataObject
		public override IEnumerable<BaseDataObject> GetDependenies ()
		{
			foreach (BaseDataObject bdo in base.GetDependenies ()) {
				yield return bdo;
			}
		}

		public override Guid ObjectType {
			get {
				return Guid.Parse ("aa8d2cde-5ab2-4656-b172-510d64f831ab");
			}
		}
		#endregion
	}
}
