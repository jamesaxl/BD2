//
//  BaseDataObjectTypeIdAttribute.cs
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
using BSO;
using BD2.Common;

namespace BD2
{
	internal sealed class BaseDataObjectTypeIdAttribute : Attribute
	{
		Guid id;

		public Guid Id {
			get {
				return id;
			}
		}

		public BaseDataObjectTypeIdAttribute (Guid id)
		{
			this.id = id;
		}
	}
}
