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

		public void Undelete ()
		{
			lock (lock_data)
				deleted = false;
		}

		public void RejectChanges ()
		{
			lock (lock_data)
				newValues = null;
		}

		public bool Modified {
			get {
				lock (lock_data)
					return deleted || (newValues != null);
			}
		}
		#region implemented abstract members of BSO.DCData
		protected abstract void StoreValues ();

		System.Collections.Generic.Dictionary<Column, byte[]> newValues;

		protected byte[][] GetValues ()
		{
			return row.GetValuesFor (this.row.DefaultColumnSet);
		}

		public byte[] GetValue (Column Column)
		{
			lock (lock_data) {
				if (newValues.ContainsKey (Column))
					return newValues [Column];
				return OnGetValue (Column);
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
			row.GetValuesFor (row.DefaultColumnSet);
		}
	}
}
