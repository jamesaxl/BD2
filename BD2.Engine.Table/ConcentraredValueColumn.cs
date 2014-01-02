//
//  ConcentraredValueColumn.cs
//
//  Author:
//       Behrooz Amoozad <Behrooz0az@gmail.com>
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
namespace BD2
{
	[BaseDataObjectType(0x0001000A, Deserialize)]
	public class ConcentraredValueColumn : Column
	{
		Guid authority;
		public Guid Authority {
			get {
				return authority;
			}
		}
		public ConcentraredValueColumn(Table Table, Guid ID, string Name,
		                               bool AllowNull, Guid Authority)
		:base(Table,ID,Name,AllowNull){
			authority = Authority;
		}
		public override void Serialize (System.IO.BinaryWriter BW)
		{
			base.Serialize (BW);
			BW.Write (authority);
		}
		private static BaseDataObject Deserialize(System.IO.BinaryReader BR)
		{
			return new ConcentraredValueColumn (new Guid(BR.ReadBytes(16)));
		}
	}
	
}
