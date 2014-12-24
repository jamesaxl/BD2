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
using System.Collections.Generic;

namespace BD2.Core
{
	public abstract class KeyValueStorage<T> : IDisposable, IEnumerable<KeyValuePair<byte[], T>>
	{


		protected KeyValueStorage ()
		{
		}

		#region IDisposable implementation

		public abstract void Dispose ();

		#endregion

		#region IEnumerable implementation

		public abstract IEnumerator<KeyValuePair<byte[], T>> GetEnumerator ();

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		#endregion

		public abstract void Initialize ();

		/// <summary>
		/// Inclusivly enumerates from start key to the end of the collection
		/// </summary>
		public abstract IEnumerable<KeyValuePair<byte[], T>> EnumerateFrom (byte[] start);

		/// <summary>
		/// Inclusivly enumerates from start key to stop key
		/// </summary>
		public abstract IEnumerable<KeyValuePair<byte[], T>> EnumerateRange (byte[] start, byte[] end);

		public abstract IEnumerator<byte[]> EnumerateKeys ();

		public abstract int Count { get ; }

		public abstract void Put (byte[] key, T value);

		public abstract T Get (byte[] key);

		public abstract void Delete (byte[] key);

	}
}

