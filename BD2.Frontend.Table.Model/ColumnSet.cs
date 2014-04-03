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
	public sealed class ColumnSet : BaseDataObject
	{
		Table table;

		public Table Table {
			get {
				return table;
			}
		}

		Column[] columns;

		public Column[] Columns {
			get {
				return columns;
			}
		}

		public ColumnSet (FrontendInstanceBase frontendInstanceBase, Guid objectID, byte[] chunkID, Table table, Column[] columns)
			: base(frontendInstanceBase, objectID, chunkID)
		{
			if (table == null)
				throw new ArgumentNullException ("table");
			if (columns == null)
				throw new ArgumentNullException ("columns");
			this.table = table;
			this.columns = columns;
		}
		#region implemented abstract members of Serializable
		public override void Serialize (Stream stream)
		{
			using (BinaryWriter BW =  new BinaryWriter (stream)) {
				BW.Write (table.ObjectID.ToByteArray ());
				BW.Write (columns.Length);
				for (int n = 0; n != columns.Length; n++) {
					BW.Write (columns [n].ObjectID.ToByteArray ());
				}
			}
		}
		#endregion
		#region implemented abstract members of BaseDataObject
		public override Guid ObjectType {
			get {
				return Guid.Parse ("b7138176-bdf7-4a80-9944-c8fd2ee16e94");
			}
		}
		#endregion
		#region implemented abstract members of BaseDataObject
		public override IEnumerable<BaseDataObject> GetDependenies ()
		{
			yield return table;
			foreach (Column column in columns)
				yield return column;
		}
		#endregion
	}
}