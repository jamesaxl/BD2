/*
 * Copyright (c) 2014 Behrooz Amoozad
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
 * DISCLAIMED. IN NO EVENT SHALL Behrooz Amoozad BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * */
using System;
using System.Collections.Generic;

namespace BD2.Common
{
	public sealed class  ByteSequenceComparer : IComparer<byte[]>, IEqualityComparer<byte[]>
	{
		static ByteSequenceComparer shared = new ByteSequenceComparer ();

		public static ByteSequenceComparer Shared {
			get {
				return shared;
			}
		}
		#region IComparer implementation
		public int Compare (byte[] x, byte[] y)
		{
			if (x == null)
				throw new ArgumentNullException ("x");
			if (y == null)
				throw new ArgumentNullException ("y");
			int L = Math.Min (y.Length, x.Length);
			int R;
			for (int n = 0; n != L; n++) {
				R = y [n].CompareTo (x [n]);
				if (R != 0) {
					return R;
				}
			}
			if (x.Length == 0) {
				return 0;
			}
			if (y.Length > x.Length) {
				return 1;
			}
			return 0;
		}
		#endregion
		#region IEqualityComparer implementation
		bool IEqualityComparer<byte[]>.Equals (byte[] x, byte[] y)
		{
			return Compare (x, y) == 0;
		}

		int IEqualityComparer<byte[]>.GetHashCode (byte[] obj)
		{
			byte[] hashbytes = new byte[4]; 
			for (int n = 0; n != obj.Length; n++) {
				hashbytes [n % 4] ^= obj [n];
			}
			return (hashbytes [0]) | (hashbytes [1] << 8) | (hashbytes [2] << 16) | (hashbytes [3] << 24); 
		}
		#endregion
	}
}

