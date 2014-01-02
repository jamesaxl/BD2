//
//  SColumn.cs
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

namespace BD2.Conv.MSSQL
{
	public class SColumn : SColumnBase, IComparable
	{
		readonly string name;
		readonly bool mandatory;

		public override string Name {
			get {
				return name;
			}
		}

		public override bool Mandatory {
			get {
				return mandatory;
			}
		}

		public SColumn (string name, bool mandatory)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			this.name = name;
			this.mandatory = mandatory;

		}

		int IComparable.CompareTo (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			SColumn sc = obj as SColumn;
			if (sc == null)
				throw new InvalidOperationException ("obj must be of type SColumn.");
			return string.Compare (sc.name, name, StringComparison.Ordinal);
		}
	}
}
