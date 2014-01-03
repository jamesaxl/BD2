//
//  TamperSafeByteArray.cs
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

namespace BD2.Core
{
	public class TamperSafeByteArray : ICloneable
	{
		byte[] buffer;

		public TamperSafeByteArray (byte[] Buffer)
		{
			buffer = Buffer;
		}

		public int this [int Index] {
			get {
				lock (buffer) {
					return buffer [Index];
				}
			}
		}

		public byte[] GetBuffer ()
		{
			byte[] BufferReference;
			lock (buffer) {
				BufferReference = buffer;
				buffer = null;
			}
			return BufferReference;
		}

		public byte[] Clone ()
		{
			lock (buffer) {
				return (byte[])buffer.Clone ();
			}
		}

		#region ICloneable implementation

		object ICloneable.Clone ()
		{
			lock (buffer) {
				return buffer.Clone ();
			}
		}

		#endregion

	}
}

