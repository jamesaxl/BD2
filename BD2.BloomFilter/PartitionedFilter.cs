//
//  PartitionedFilter.cs
//
//  Author:
//       Behrooz Amoozad <behrooz0az@gmail.com>
//
//  Copyright (c) 2013 Behrooz Amoozad
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;

namespace BD2.BloomFilter
{
	public class PartitionedFilter:IFilter
	{
		DFilter[] partitions;
		int partitionBits;

		public int PartitionBits {
			get {
				return partitionBits;
			}
		}

		int[] partiionMasks;

		public PartitionedFilter (int partitionBits)
		{
			this.partitionBits = partitionBits;
			int n = 1 << partitionBits;

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
