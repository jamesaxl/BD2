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
using BD2.Common;
using System.Collections.Generic;

namespace BD2.BloomFilter
{
	public class DFilter
	{
		Guid SessionID;
		byte depthBits;

		static byte ConvertBit (int value, BitDistributionAlgorithm algorithm)
		{
			value ++;
			if (value <= 0) {
				throw new ArgumentException ("value must be a positive integer.(value > 0)", "value");
			}
			switch (algorithm) {
			case BitDistributionAlgorithm.Linear:
				return (byte)Math.Min (value, 255);
			}
			throw new ArgumentOutOfRangeException ("algorithm");
		}

		SortedSet<IHashable>[] localPerBitIHashables;
		long[] localDeltaBits;
		long[] remoteDeltaBits;

		public DFilter (BasicObjectBus bus, IEnumerable<IHashable> items)
		{
			bus.RegisterPath ();
		}
		//		public DFilter (int bits, int threshhold, int estimatedSize)
		//		{
		//			this.estimatedSize = estimatedSize;
		//			if (bits < 2) {
		//				throw new Exception ("Less than two bits in a bloom filter is not even theorically possible.");
		//			}
		//			this.bits = bits;
		//		}
		void SyncInteractive ()
		{

		}
	}
}

