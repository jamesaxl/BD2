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
using BD2.Frontend.Table.Model;

namespace BD2.Frontend.Table
{
	public sealed class IndexValue : IComparable
	{
		public int CompareTo (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			IndexValue OtherIV = obj as IndexValue;
			if (OtherIV == null)
				throw new ArgumentException ("obj must be of type IndexValue.", "obj");
			if (!OtherIV.index.Equals (index))
				throw new ArgumentException ("obj must have the same index signature", "obj");
			Row OtherRow = OtherIV.row;
			IEnumerator<IndexColumnBase> OtherIndices = OtherIV.index.GetIndexColumns ();
			IEnumerator<IndexColumnBase> ThisIndices = this.index.GetIndexColumns ();
			while (ThisIndices.MoveNext () && OtherIndices.MoveNext ()) {
				int CompareValue;
				//TODO:HACK:XXX
				CompareValue = 0;//row.GetValue (ThisIndices.Current.Column).CompareTo (OtherRow.GetValue (OtherIndices.Current.Column));
				if (CompareValue == 0)
					continue;
				if (ThisIndices.Current.SortAscending)
					return CompareValue;
				return -CompareValue;
			}
			return 0;
		}

		IndexBase index;

		public IndexBase Index {
			get {
				return index;
			}
		}

		Row row;

		public Row Row {
			get {
				return row;
			}
		}

		public IndexValue (IndexBase index, Row row)
		{
			if (index == null)
				throw new ArgumentNullException ("Index");
			if (row == null)
				throw new ArgumentNullException ("Row");
			this.index = index;
			this.row = row;
		}
	}
}

