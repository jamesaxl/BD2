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
 * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
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
	public sealed class Row
	{
		Guid tableID;

		public Guid TableID {
			get {
				return tableID;
			}
		}

		byte[] fields;

		public byte[] Fields {
			get {
				return fields;
			}
		}

		bool[] nulls;

		public bool[] Nulls {
			get {
				return nulls;
			}
		}

		public Row (Guid tableID, byte[] fields, bool[] nulls)
		{
			if (fields == null)
				throw new ArgumentNullException ("fields");
			if (nulls == null)
				throw new ArgumentNullException ("nulls");
			this.tableID = tableID;
			if (fields.Length != nulls.Length)
				throw new ArgumentException ("the null state for all fileds must be specified.");
			this.fields = fields;
			this.nulls = nulls;
		}

		public static Row Deserialize (byte[] bytes)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (bytes, false)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					Guid tableID = new Guid (BR.ReadBytes (16));
					int fieldCount = BR.ReadInt32 ();
					byte[][] fields = new byte[fieldCount][];
					bool[] nulls = new bool[fieldCount];
					for (int n = 0; n != fieldCount; n++) {
						if (nulls [n] = BR.ReadBoolean (n))
							fields [n] = BR.ReadBytes (BR.ReadInt32 ());
					}
					return new Row (tableID, fields, nulls);
				}
			}
		}
	}
}

