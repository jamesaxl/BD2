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
 * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * */
using System;

namespace BD2.Frontend.Table
{
	public class ColumnSet : Model.ColumnSet
	{
		Table table;

		public ColumnSet (Table Table)
		{
			table = Table;
		}
		#region implemented abstract members of BSO.ColumnSet
		public override Table Table {
			get {
				return table;
			}
		}

		Column[] columns;

		public override System.Collections.Generic.IEnumerable<Column> GetColumns ()
		{
			return columns.GetEnumerator ();
		}

		public override object[] FromRaw (byte[] Raw)
		{
			this.OffsetHandler.GetOffsetMapFor (true, columns, null);
		}

		public override object[] FromRawStream (System.IO.Stream Raw)
		{
			throw new System.NotImplementedException ();
		}

		public override byte[] ToRaw (object[] Objects)
		{
			throw new System.NotImplementedException ();
		}

		public override int ToRawStream (object[] Objects, System.IO.Stream Stream)
		{
			throw new System.NotImplementedException ();
		}

		public override void Retrieve (Column Column)
		{
			throw new System.NotImplementedException ();
		}
		#endregion
	}
}

