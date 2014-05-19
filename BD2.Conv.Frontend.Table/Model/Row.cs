/*
 * Copyright (c) 2014 Behrooz Amoozad
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

namespace BD2.Conv.Frontend.Table
{
	public class Row
	{
		ColumnSet columnSet;
		object[] fields;

		public int FieldCount {
			get {
				return fields.Length;
			}
		}

		public ColumnSet ColumnSet {
			get {
				return columnSet;
			}
		}

		public object[] Fields {
			get {
				return fields;
			}
		}

		public Row (ColumnSet columnSet, object[] fields)
		{
			if (fields == null)
				throw new ArgumentNullException ("fields");
			this.columnSet = columnSet;
			this.fields = fields;
		}

		public byte[] Serialize (Func<System.IO.Stream, byte[]> createStream)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW  = new System.IO.BinaryWriter (MS)) {
					BW.Write (columnSet.ID.ToByteArray ());
					BW.Write (FieldCount);
					for (int n = 0; n != FieldCount; n++) {
						if (fields [n] == null) {
							BW.Write (1);
						} else if (fields [n] is System.IO.Stream) {
							BW.Write (2);
							BW.Write (createStream ((System.IO.Stream)fields [n]));
						} else {
							BW.Write (0);
							BW.Write (((byte[])fields [n]).Length);
							BW.Write ((byte[])fields [n]);
						}
					}
				}
				return MS.ToArray ();
			}
		}

		public static Row Deserialize (byte[] bytes, Func<Guid, ColumnSet> getColumnSet)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (bytes, false)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					Guid columnSet = new Guid (BR.ReadBytes (16));
					object[] fields = new object[BR.ReadInt32 ()];
					ColumnSet cs = getColumnSet (columnSet);
					int n = 0;
					foreach (Column col in cs.Columns) {
						switch (col.TFQN) {
						case "System.Byte":
							fields [n] = BR.ReadByte ();
							break;
						case "System.SByte":
							fields [n] = BR.ReadSByte ();
							break;
						case "System.Int16":
							fields [n] = BR.ReadInt16 ();
							break;
						case "System.UInt16":
							fields [n] = BR.ReadUInt16 ();
							break;
						case "System.Int32":
							fields [n] = BR.ReadInt32 ();
							break;
						case "System.UInt32":
							fields [n] = BR.ReadUInt32 ();
							break;
						case "System.Int64":
							fields [n] = BR.ReadInt64 ();
							break;
						case "System.UInt64":
							fields [n] = BR.ReadUInt64 ();
							break;
						case "System.Single":
							fields [n] = BR.ReadSingle ();
							break;
						case "System.Double":
							fields [n] = BR.ReadDouble ();
							break;
						case "System.Boolean":
							fields [n] = BR.ReadBoolean ();
							break;
						case "System.Char":
							fields [n] = BR.ReadChar ();
							break;
						case "System.String":
							fields [n] = BR.ReadString ();
							break;
						}
					}
					//todo: deserialize
					return new Row (cs, fields);
				}
			}
		}
	}
}

