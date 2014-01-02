//
//  IndexBase.cs
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

namespace BD2.Frontend.Table.Model
{
	public abstract class IndexBase : BaseDataObject
	{
		public abstract IEnumerator<IndexColumnBase> GetIndexColumns();
		public abstract bool Unique { get; }
		private int HashCode;
		public override int GetHashCode () {
			if (HashCode == 0) {
				int index = 1;
				IEnumerator <IndexColumnBase> IndexColumns = GetIndexColumns();
				while(IndexColumns.MoveNext()){
					HashCode ^= (IndexColumns.Current.GetHashCode () * (index++));
				}
			}
			return HashCode;
		}
		public override bool Equals (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			IndexBase OtherIndex = obj as IndexBase;
			if(OtherIndex == null)
			{
				throw new ArgumentException("obj must be of type Index", "obj");
			}
			if (HashCode == OtherIndex.HashCode) {
				IEnumerator<IndexColumnBase> OtherIndexColumns = OtherIndex.GetIndexColumns ();
				IEnumerator<IndexColumnBase> ThisIndexColumns = GetIndexColumns ();
				while(OtherIndexColumns.MoveNext () && ThisIndexColumns.MoveNext())
				{
					if (!ThisIndexColumns.Current.Equals (OtherIndexColumns.Current))
						return false;
				}
				return true;
			} else {
				return false;
			}
		}
	}
}

