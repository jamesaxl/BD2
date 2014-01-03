//
//  ByteSequenceComparer.cs
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

namespace BD2.Core
{
	public sealed class  ByteSequenceComparer : IComparer<byte[]>
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
	}
}

