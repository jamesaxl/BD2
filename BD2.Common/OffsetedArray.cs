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

namespace BD2.Common
{
	public sealed class OffsetedArray<T>
	{
		T[] aReference;
		OffsetedArray<T> pReference;
		int offset;

		public int Offset {
			get {
				return offset;
			}
			set {
				offset = value;
			}
		}

		public int Length {
			get {
				if (pReference != null)
					return pReference.Length - offset;
				return aReference.Length - offset;
			}
		}

		public OffsetedArray (T[] reference, int offset)
		{
			if (reference == null)
				throw new ArgumentNullException ("reference");
			aReference = reference;
			pReference = null;
			offset = offset;
		}

		public OffsetedArray (OffsetedArray<T> reference, int offset)
		{
			if (reference == null)
				throw new ArgumentNullException ("reference");
			aReference = null;
			pReference = reference;
			offset = offset;
		}

		public OffsetedArray (T[] reference)
		{
			if (reference == null)
				throw new ArgumentNullException ("reference");
			aReference = reference;
			pReference = null;
			offset = 0;
		}

		public OffsetedArray (OffsetedArray<T> reference)
		{
			if (reference == null)
				throw new ArgumentNullException ("reference");
			aReference = null;
			pReference = reference;
			offset = 0;
		}

		public T[] GetStrippedPart ()
		{
			T[] Ret = new T[offset];
			for (int n = 0; n != offset; n++) {
				Ret [n] = this [n];
			}
			return Ret;
		}

		public T this [int index] {
			get {
				if (pReference != null)
					return pReference [index];
				return aReference [index];
			}
			set {
				if (pReference != null)
					pReference [index] = value;
				aReference [index] = value;
			}
		}

		public static explicit operator T[] (OffsetedArray<T> obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			T[] r = new T[obj.Length];
			for (int n =0; n!= obj.Length; n++) {
				r [n] = obj [n];
			}
			return r;
		}

		public static implicit operator OffsetedArray<T> (T[] obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			return new OffsetedArray<T> (obj, 0);
		}
	}
}
