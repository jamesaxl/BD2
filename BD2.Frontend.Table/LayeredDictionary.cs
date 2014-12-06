// /*
//  * Copyright (c) 2014 Behrooz Amoozad
//  * All rights reserved.
//  *
//  * Redistribution and use in source and binary forms, with or without
//  * modification, are permitted provided that the following conditions are met:
//  *     * Redistributions of source code must retain the above copyright
//  *       notice, this list of conditions and the following disclaimer.
//  *     * Redistributions in binary form must reproduce the above copyright
//  *       notice, this list of conditions and the following disclaimer in the
//  *       documentation and/or other materials provided with the distribution.
//  *     * Neither the name of the bd2 nor the
//  *       names of its contributors may be used to endorse or promote products
//  *       derived from this software without specific prior written permission.
//  *
//  * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//  * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//  * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//  * DISCLAIMED. IN NO EVENT SHALL Behrooz Amoozad BE LIABLE FOR ANY
//  * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//  * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//  * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//  * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//  * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//  * */
using System;
using System.Collections;
using System.Collections.Generic;

namespace BD2.Frontend.Table
{
	public class LayeredDictionary <TKey, TValue>:IDictionary<TKey,TValue>
	{
		IDictionary<TKey, TValue> baseDictionary;
		IDictionary<TKey, TValue> overlay;
		int count;

		public LayeredDictionary (IDictionary<TKey, TValue> baseDictionary, bool keepStatistics)
		{
			this.baseDictionary = baseDictionary;
			this.overlay = new SortedDictionary<TKey, TValue> ();
			if (keepStatistics)
				count = 0;
			else
				count = -1;
		}

		#region IDictionary implementation

		public void Add (TKey key, TValue value)
		{
			if (count == -1) {
				overlay.Add (key, value);
			} else {
				if (!baseDictionary.ContainsKey (key))
					count++;
				overlay.Add (key, value);
			}
		}

		public bool ContainsKey (TKey key)
		{
			return overlay.ContainsKey (key) || baseDictionary.ContainsKey (key);
		}

		public bool Remove (TKey key)
		{
			if (overlay.Remove (key)) {
				if (count != -1) {
					if (baseDictionary.ContainsKey (key)) {
						count--;
					}
				}
			}
		}

		public bool TryGetValue (TKey key, out TValue value)
		{
			if (!overlay.TryGetValue (key, out value)) {
				return baseDictionary.TryGetValue (key, out value);
			}
			return true;
		}

		public TValue this [TKey index] {
			get {
				TValue value;
				if (TryGetValue (index, out value))
					return value;
				throw new KeyNotFoundException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public ICollection<TKey> Keys {
			get {
				throw new NotImplementedException ();
			}
		}

		public ICollection<TValue> Values {
			get {
				throw new NotImplementedException ();
			}
		}

		#endregion

		#region ICollection implementation

		public void Add (KeyValuePair<TKey, TValue> item)
		{
			overlay.Add (item.Key, item.Value);
		}

		public void Clear ()
		{
			if (count != -1)
				count = 0;
			overlay.Clear ();
		}

		public bool Contains (KeyValuePair<TKey, TValue> item)
		{
			return overlay.Contains (item) || baseDictionary.Contains (item);
		}

		public void CopyTo (KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			throw new NotImplementedException ();
		}

		public bool Remove (KeyValuePair<TKey, TValue> item)
		{
			if (overlay.Remove (item)) {
				if (count != -1) {
					if (baseDictionary.ContainsKey (item)) {
						count--;
					}
				}
			}
		}

		public int Count {
			get {
				return count;
			}
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		#endregion

		#region IEnumerable implementation

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region IEnumerable implementation

		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}

