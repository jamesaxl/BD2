//
//  Column.cs
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
	public class Column : Model.Column
	{
		public override BaseDataObject Drop ()
		{
			throw new System.NotImplementedException ();
		}
		Table table;
		string name; public override string Name { get { return name; } }
		bool? allowNull; public override bool AllowNull { get { return allowNull.Value; } }
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
			base.Serialize(BW);
			BSO.Processor.SerializeOne (BW, objectId, typeof(Guid), null);
			BSO.Processor.SerializeOne (BW, name, typeof(string), null);
		}
	}
}
