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
using System.Collections.Generic;
using BD2.Core;
using BSO;

namespace BD2.Frontend.Table
{
	public class Column : Model.Column
	{
		public override BaseDataObject Drop ()
		{
			throw new System.NotImplementedException ();
		}

		Table table;
		string name;

		public override string Name { get { return name; } }

		bool? allowNull;

		public override bool AllowNull { get { return allowNull.Value; } }

		public Column (Table Table, Guid ID, string Name, bool AllowNull)
			:base()
		{
			if (Table == null)
				throw new ArgumentNullException ("Table");
			if (Name == null)
				throw new ArgumentNullException ("Name");
			table = Table;
			objectId = ID;
			name = Name;
			allowNull = AllowNull;
		}

		internal Column (Table Table, System.IO.BinaryReader BR)
			:base()
		{
			table = Table;
			objectId = (Guid)BSO.Processor.DeserializeOne (BR, typeof(Guid), null);
			name = (string)BSO.Processor.DeserializeOne (BR, typeof(string), null);
		}

		Guid objectId;

		public override Guid ObjectID {
			get {
				return objectId;
			}
		}

		public override void Serialize (System.IO.BinaryWriter BW)
		{
			base.Serialize (BW);
			BSO.Processor.SerializeOne (BW, objectId, typeof(Guid), null);
			BSO.Processor.SerializeOne (BW, name, typeof(string), null);
		}
	}
}
