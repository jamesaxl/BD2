//
//  Row.cs
//
//  Author:
//       Behrooz Amoozad <behrooz0az@gmail.com>
//
//  Copyright (c) 2013 behrooz
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using BD2.Common;
using BSO;
namespace BD2.Frontend.Table
{
	public class Row : Model.Row
	{
		public override bool Alive {
			get
			{
				throw new System.NotImplementedException ();
			}
		}
		public override BaseDataObject Drop ()
		{
			throw new ObjectDrop(Guid.NewGuid(), this);
		}
		 
		byte[] rawData;
		object GetRawDataClone() { return rawData.Clone(); }
		Model.ColumnSet columnSet; public override Model.ColumnSet DefaultColumnSet { get { return columnSet; } }
		string name; public string Name { get { return name; } }
		long? length; public long Length { get { return length.Value; } }
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
		Guid objectID; public override Guid ObjectID { get { return objectID; } }
		public override void Serialize (System.IO.BinaryWriter BW)
		{
			if (BW == null)
				throw new ArgumentNullException ("BW");
			BSO.Processor.SerializeOne(BW, objectID, typeof(Guid), null);
		}
	}
}
