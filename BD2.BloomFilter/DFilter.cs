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

namespace BD2.BloomFilter
{
	public class DFilter:IFilter
	{
		long count;

		public long Count {
			get {
				return count;
			}
		}

		int bits;
		int threshhold;
		System.Collections.Generic.SortedSet<IHashable> FCC;

		public DFilter (int bits, int threshhold)
		{
			if (bits < 2) {
				throw new Exception ("Less than two bits in a bloom filter is not even theoretically possible.");
			}
			this.bits = bits;
		}

		public void AddItem (IHashable item)
		{
			count ++;
			if (count < threshhold) {
				FCC.Add (item);
			}

		}

		float Contains (IHashable instance)
		{
			return 0;
		}

		readonly float averageFalsePositiveRate;

		float AverageFalsePositiveRate {
			get {
				return averageFalsePositiveRate;
			}
		}

		readonly int nextFalsePasitiveRatioCalculationPoint;

		int NextFalsePasitiveRatioCalculationPoint {
			get {
				return nextFalsePasitiveRatioCalculationPoint;
			}
		}
		#region IFilter implementation
		public float ContainsObject (IHashable hashable)
		{
			throw new NotImplementedException ();
		}

		public void Add (IHashable hashable)
		{
			throw new NotImplementedException ();
		}

		public bool HasFalsePositive {
			get {
				throw new NotImplementedException ();
			}
		}

		public float FalsePositiveProbability {
			get {
				throw new NotImplementedException ();
			}
		}
		#endregion
	}
}

