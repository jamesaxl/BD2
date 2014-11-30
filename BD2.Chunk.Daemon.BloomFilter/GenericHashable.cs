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
 * DISCLAIMED. IN NO EVENT SHALL Behrooz Amoozad BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * */
using System;
using BD2.Chunk.Daemon;

namespace BD2.BloomFilter
{
	public class GenericHashable : IHashable, IComparable
	{
		readonly int[] availableSizes;
		readonly byte[] bytes;

		public GenericHashable (byte[] bytes, int[] availableSizes)
		{
			if (bytes == null)
				throw new ArgumentNullException ("bytes");
			this.bytes = bytes;
			this.availableSizes = availableSizes;
		}

		public GenericHashable (byte[] bytes)
			: this (bytes, null)
		{
		}

		#region IHashable implementation

		int[] IHashable.GetAvailableHashSizes ()
		{
			if (availableSizes == null) {
				availableSizes = new int[bytes.Length * 8];
				for (int n = 0; n != availableSizes.Length; n++) {
					availableSizes [n] = n + 1;
				}
			}
			return (int[])availableSizes.Clone ();
		}

		int IHashable.GetAvailableHashess (int bits)
		{
			return bytes.Length * 8 / bits;
		}

		long IHashable.GetHashValue (int bits, int index)
		{
			if ((bits > 64) || (bits < 1))
				throw new ArgumentOutOfRangeException ("bits", "argument must be in range [1-64]");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index", "argument cannot be negative");
			const int bitsInByte = 8;
			long hash = 0;
			int inOffset = index * bits;
			int outOffset = 0;
			for (int n = 0; n != bits; n++) {
				int inBitIndex = inOffset + n;
				bool bitValue = (bytes [inBitIndex / bitsInByte] & (1 << (inBitIndex % bitsInByte))) > 0;
				if (bitValue) {
					int outBitIndex = outOffset + n;
					hash |= (1L << outBitIndex);
				}
			}
			return hash;
		}

		byte[] IHashable.GetHash (int bits, int index)
		{
			const int bitsInByte = 8;
			int byteCount = (bits + (bitsInByte - 1)) / bitsInByte;
			byte[] hashBytes = new byte[byteCount];
			int inOffset = index * bits;
			int outOffset = 0;
			for (int n = 0; n != bits; n++) {
				int inBitIndex = inOffset + n;
				bool bitValue = (bytes [inBitIndex / bitsInByte] & (1 << (inBitIndex % bitsInByte))) > 0;
				if (bitValue) {
					int outBitIndex = outOffset + n;
					hashBytes [outBitIndex / bitsInByte] |= (byte)(1 << (outBitIndex % bitsInByte));
				}
			}
			return hashBytes;
		}

		#endregion

		#region IComparable implementation

		int IComparable.CompareTo (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			GenericHashable hashable = obj as GenericHashable;
			if (hashable == null) {
				throw new ArgumentException ("obj must be of type GenericHashable", "obj");
			}
			int compResult = hashable.bytes.Length.CompareTo (bytes.Length);
			if (compResult != 0)
				return compResult;
			for (int n = 0; n != hashable.bytes.Length; n++) {
				compResult = hashable.bytes [n].CompareTo (bytes [n]);
				if (compResult != 0)
					return compResult;
			}
			return 0;
		}

		#endregion
	}
}

