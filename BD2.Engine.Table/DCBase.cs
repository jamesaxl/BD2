//
//  DCBase.cs
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
namespace BD2.Frontend.Table
{
	public abstract class DCBase
	{
		object lock_data = new object ();
		Row row;
		public Row Row {
			get {
				return row;
			}
		}
		bool deleted = false;
		//TODO:add checks to all the methods from here downwards
		public void Delete ()
		{
			lock (lock_data) {
				//Column[] AlteredCols = new List<Column>(newValues.Keys).ToArray();
				newValues = null;
				deleted = true;
				//AlteredCols//TODO: notify before and after change 
			}
		}
		public void Undelete() { lock(lock_data) deleted = false; }
		public void RejectChanges() { lock(lock_data) newValues = null; }
		public bool Modified {
			get {
				lock(lock_data)
					return deleted || (newValues != null);
			}
		}
#region implemented abstract members of BSO.DCData
		protected abstract void StoreValues();
		System.Collections.Generic.Dictionary<Column, byte[]> newValues;
		protected byte[][] GetValues()
		{
			return row.GetValuesFor(this.row.DefaultColumnSet);
		}
		public byte[] GetValue (Column Column)
		{
			lock (lock_data) {
				if (newValues.ContainsKey (Column))
					return newValues [Column];
				return OnGetValue(Column);
			}
		}
		protected abstract byte[] OnGetValue (Column Column);
		//TODO: make whatever underneath like above
		protected abstract IComparable OnGetValue (Column Column, object[] Parameters);
		protected virtual void OnSetValue (Column Column, IComparable Value)
		{
		}
		protected virtual void OnSetValue (Column Column, IComparable Value, object[] Parameters)
		{
		}
#endregion
		public virtual bool Modifiable {
			get {
				return true;
			}
		}
		public DCBase (Row Row)
			:base()
		{
			row = Row;
			row.GetValuesFor(row.DefaultColumnSet);
		}
	}
}
