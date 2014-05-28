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

namespace BD2.Frontend.Table.Model
{
	public abstract class IndexBase : BaseDataObject
	{
		Table table;
		bool unique;

		protected IndexBase (FrontendInstanceBase frontendInstanceBase, byte[] chunkID, Table table, bool unique)
		:base (frontendInstanceBase, chunkID)
		{
			if (table == null)
				throw new ArgumentNullException ("table");
			this.table = table;
			this.unique = unique; 
		}

		public Table Table { get { return table; } }

		public abstract IEnumerator<IndexColumnBase> GetIndexColumns ();

		public bool Unique { get { return unique; } }

		public bool SignatureEquals (IndexBase index)
		{
			if (table.Equals (index.table)) {
				IEnumerator<IndexColumnBase> othericbe = index.GetIndexColumns ();
				IEnumerator<IndexColumnBase> icbe = GetIndexColumns ();
				while (icbe.MoveNext()) {
					if (!othericbe.MoveNext ())
						return false;
					if (icbe.Current != othericbe.Current)
						return false;
				}
				return !othericbe.MoveNext ();
			}
			return false;
		}
	}
}

