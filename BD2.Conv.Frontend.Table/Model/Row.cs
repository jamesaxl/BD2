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
			if (columnSet.Columns.Length != fields.Length)
				throw new Exception ();

		}

		public byte[] Serialize ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter (MS)) {
					bw.Write (columnSet.GetHash ());
					bw.Write (FieldCount);
					for (int n = 0; n != FieldCount; n++) {
						bw.Write ((fields [n] == null) || (fields [n] == DBNull.Value));
						if ((fields [n] != null) && (fields [n] != DBNull.Value)) {
							//Console.WriteLine ("Column {0} contains {1}", n, values [n]);
							switch (columnSet.Columns [n].TFQN) {
							case "System.Byte":
								bw.Write ((byte)fields [n]);
								break;
							case "System.Byte[]":
								bw.Write (((byte[])fields [n]).Length);
								bw.Write ((byte[])fields [n]);
								break;
							case "System.SByte":
								bw.Write ((sbyte)fields [n]);
								break;
							case "System.Int16":
								bw.Write ((short)fields [n]);
								break;
							case "System.UInt16":
								bw.Write ((ushort)fields [n]);
								break;
							case "System.Int32":
								bw.Write ((int)fields [n]);
								break;
							case "System.UInt32":
								bw.Write ((uint)fields [n]);
								break;
							case "System.Int64":
								bw.Write ((long)fields [n]);
								break;
							case "System.UInt64":
								bw.Write ((ulong)fields [n]);
								break;
							case "System.Single":
								bw.Write ((float)fields [n]);
								break;
							case "System.Double":
								bw.Write ((double)fields [n]);
								break;
							case "System.String":
								bw.Write ((string)fields [n]);
								break;
							case "System.Char":
								bw.Write ((char)fields [n]);
								break;
							case "System.Guid":
								bw.Write (((Guid)fields [n]).ToByteArray ());
								break;
							case "System.Boolean":
								bw.Write ((bool)fields [n]);
								break;
							case "System.DateTime":
								if (fields [n] is string) {
									bw.Write ((DateTime.Parse ((string)fields [n])).Ticks);
								} else {
									bw.Write (((DateTime)fields [n]).Ticks);
								}
								break;
							default:
								throw new Exception (string.Format ("Type {0} is undefined", columnSet.Columns [n].TFQN));
							}
						}
					}
				}
				return MS.ToArray ();
			}
		}

		static System.Collections.Concurrent.ConcurrentDictionary<byte[], ColumnSet> css = new System.Collections.Concurrent.ConcurrentDictionary<byte[], ColumnSet> 
			(BD2.Common.ByteSequenceComparer.Shared);

		public static void AddColumnSet (ColumnSet columnSet)
		{
			css.AddOrUpdate (columnSet.GetHash (), (hash) => columnSet, (hash,ocs) => {
				throw new Exception ();
			});
		}

		public static Row Deserialize (byte[] bytes)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (bytes, false)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					byte[] columnSet = BR.ReadBytes (32);
					int FieldCount = BR.ReadInt32 ();
					object[] fields = new object[FieldCount];
					ColumnSet cs = css [columnSet];
					if (cs.Columns.Length != fields.Length)
						throw new Exception ();
					for (int n = 0; n != fields.Length; n++) {
						bool Null = BR.ReadBoolean ();
						if (Null) {
							fields [n] = null;
							continue;
						}
						switch (cs.Columns [n].TFQN) {
						case "System.Byte[]":
							fields [n] = BR.ReadBytes (BR.ReadInt32 ());
							break;
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
						case "System.String":
							fields [n] = BR.ReadString ();
							break;
						case "System.Char":
							fields [n] = BR.ReadChar ();
							break;
						case "System.Boolean":
							fields [n] = BR.ReadBoolean ();
							break;
						case "System.DateTime":
							fields [n] = new DateTime (BR.ReadInt64 ());
							break;
						case "System.Guid":
							fields [n] = new Guid (BR.ReadBytes (16));
							break;
						}
					}
					return new Row (cs, fields);
				}
			}
		}
	}
}