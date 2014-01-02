//
//  Extensions.cs
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

namespace BD2.Common
{
	public static class Extensions
	{
		public static void WriteArray<T> (this System.IO.BinaryWriter BinaryWriter, object[] Params)
		{
			BinaryWriter.Write (Params.Length);
			foreach (object obj in Params) {
				Serializable.WritePrimitive(BinaryWriter, obj);
			}
		}
		public static void WriteVersion (this System.IO.BinaryWriter BinaryWriter, short Major, short Minor)
		{
			BinaryWriter.Write (Major);
			BinaryWriter.Write (Minor);
		}
		public static void Write (this System.IO.BinaryWriter BinaryWriter, short[] Params)
		{
			BinaryWriter.Write(Params.Length);
			foreach (short Param in Params) {
				BinaryWriter.Write (Param);
			}
		}
		public static void Write (this System.IO.BinaryWriter BinaryWriter, int[] Params)
		{
			BinaryWriter.Write(Params.Length);
			foreach (int Param in Params) {
				BinaryWriter.Write (Param);
			}
		}
		public static void Write (this System.IO.BinaryWriter BinaryWriter, long[] Params)
		{
			BinaryWriter.Write(Params.Length);
			foreach (long Param in Params) {
				BinaryWriter.Write (Param);
			}
		}
		public static void Write (this System.IO.BinaryWriter BinaryWriter, Guid Param)
		{
			BinaryWriter.Write(Param.ToByteArray());
		}
		public static void Write (this System.IO.BinaryWriter BinaryWriter, Guid[] Params)
		{
			BinaryWriter.Write(Params.Length);
			foreach (Guid Param in Params) {
				BinaryWriter.Write (Param.ToByteArray ());
			}
		}
		public static void Write (this System.IO.BinaryWriter BinaryWriter, System.Collections.Generic.List<Guid> Params)
		{
			BinaryWriter.Write(Params.Count);
			foreach (Guid Param in Params) {
				BinaryWriter.Write (Param.ToByteArray ());
			}
		}
	}
}

