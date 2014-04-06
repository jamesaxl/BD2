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
using BD2.Frontend.Table.Model;
using System.Collections.Generic;

namespace BD2.Frontend.Table
{
	public class ValueSet : BD2.Frontend.Table.Model.ValueSet
	{
		SortedDictionary<Model.Column, object> values = new SortedDictionary<Model.Column, object> ();

		public ValueSet (Row row, byte[] rawData)
			: base(row)
		{
			System.IO.MemoryStream MS = new System.IO.MemoryStream (rawData, false);
			System.IO.BinaryReader BR = new System.IO.BinaryReader (MS);
			IValueDeserializer des = ((FrontendInstance)row.FrontendInstanceBase).ValueDeserializer;
			foreach (BD2.Frontend.Table.Model.Column col in row.ColumnSet.Columns) {
				//As bad is it can get :P
				//TODO: have the column to provide it's own length|length of it's length+a value to be added with the length read from the database, 
				//note that such value should be subtracted before serialization 
				des.Deserialize (col.TypeID, BR.ReadBytes (BR.ReadInt32 ()));
			}
		}
		#region implemented abstract members of ValueSet
		public override object GetValue (Model.Column column)
		{
			return values [column];
		}
		#endregion
	}
}

