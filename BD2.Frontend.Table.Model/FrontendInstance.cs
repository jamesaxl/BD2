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
using System.Collections.Generic;

namespace BD2.Frontend.Table.Model
{
	public abstract class FrontendInstance : BD2.Core.FrontendInstanceBase
	{
		protected FrontendInstance (Frontend frontend)
			: base (frontend)
		{
		}

		//TODO: Implement a real SPF algorithm
		System.Collections.Generic.SortedDictionary<ColumnSet, System.Collections.Generic.SortedDictionary<ColumnSet, ColumnSetConverter>> cscs = new System.Collections.Generic.SortedDictionary<ColumnSet, System.Collections.Generic.SortedDictionary<ColumnSet, ColumnSetConverter>> ();

		public void AddColumnSetConverter (ColumnSetConverter csc)
		{
			foreach (ColumnSet ocs in csc.OutColumnSets) {
				if (!cscs.ContainsKey (ocs)) {
					cscs.Add (ocs, new System.Collections.Generic.SortedDictionary<ColumnSet, ColumnSetConverter> ());
				}
				System.Collections.Generic.SortedDictionary<ColumnSet, ColumnSetConverter> sources = cscs [ocs];
				foreach (ColumnSet ics in csc.InColumnSets) {
					sources.Add (ics, csc);
				}
			}
		}

		public ColumnSetConverter GetColumnSetConverter (ColumnSet columnSet, ColumnSet outputColumnSet)
		{
			if (cscs.ContainsKey (outputColumnSet)) {
				System.Collections.Generic.SortedDictionary<ColumnSet, ColumnSetConverter> sources = cscs [outputColumnSet];
				if (sources.ContainsKey (columnSet)) {
					return sources [columnSet];
				} else
					throw new NotSupportedException ("Conversion from source ColumnSet is not supported.");			
			} else
				throw new NotSupportedException ("Conversion to destination ColumnSet is not supported.");
		}

		public object[] ConvertColumnSet (object[] input, ColumnSet inputColumnSet, ColumnSet outputColumnSet)
		{
			if (inputColumnSet == outputColumnSet)
				return input;
			return GetColumnSetConverter (inputColumnSet, outputColumnSet).Convert (input, inputColumnSet, outputColumnSet);
		}


	}
}

