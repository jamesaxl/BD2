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
	public class Row : Model.Row
	{
		public override bool Alive {
			get {
				throw new System.NotImplementedException ();
			}
		}

		public override BaseDataObject Drop ()
		{
			throw new ObjectDrop (Guid.NewGuid (), this);
		}

		byte[] rawData;

		object GetRawDataClone ()
		{
			return rawData.Clone ();
		}

		Model.ColumnSet columnSet;

		public override Model.ColumnSet DefaultColumnSet { get { return columnSet; } }

		string name;

		public string Name { get { return name; } }

		long? length;

		public long Length { get { return length.Value; } }

		private Row (ColumnSet ColumnSet, Guid ID)
			: base()
		{
			if (ColumnSet == null)
				throw new ArgumentNullException ("Table");
			columnSet = ColumnSet;
			objectID = ID;
			length = null;
		}

		internal Row (ColumnSet ColumnSet, Guid ID, byte[] RawData)
			: base()
		{
			if (RawData == null)
				throw new ArgumentNullException ("RawData");
			if (ColumnSet == null)
				throw new ArgumentNullException ("Table");
			columnSet = ColumnSet;
			objectID = ID;
			rawData = RawData;
		}

		internal Row (ColumnSet ColumnSet, System.IO.BinaryReader BR)
		: base ()
		{
			if (BR == null)
				throw new ArgumentNullException ("BR");
			if (ColumnSet == null)
				throw new ArgumentNullException ("Table");
			objectID = (Guid)BSO.Processor.DeserializeOne (BR, typeof(Guid), null);
			name = (string)BSO.Processor.DeserializeOne (BR, typeof(string), null);
			columnSet = ColumnSet;
		}

		Guid objectID;

		public override Guid ObjectID { get { return objectID; } }

		public override void Serialize (System.IO.BinaryWriter BW)
		{
			if (BW == null)
				throw new ArgumentNullException ("BW");
			BSO.Processor.SerializeOne (BW, objectID, typeof(Guid), null);
		}
	}
}
