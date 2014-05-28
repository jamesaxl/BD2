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

namespace BD2.Common
{
	public static class Extensions
	{
		public static void WriteArray<T> (this System.IO.BinaryWriter BinaryWriter, object[] Params)
		{
			BinaryWriter.Write (Params.Length);
			foreach (object obj in Params) {
				Serializable.WritePrimitive (BinaryWriter, obj);
			}
		}

		public static void WriteVersion (this System.IO.BinaryWriter BinaryWriter, short Major, short Minor)
		{
			BinaryWriter.Write (Major);
			BinaryWriter.Write (Minor);
		}

		public static void Write (this System.IO.BinaryWriter BinaryWriter, short[] Params)
		{
			BinaryWriter.Write (Params.Length);
			foreach (short Param in Params) {
				BinaryWriter.Write (Param);
			}
		}

		public static void Write (this System.IO.BinaryWriter BinaryWriter, int[] Params)
		{
			BinaryWriter.Write (Params.Length);
			foreach (int Param in Params) {
				BinaryWriter.Write (Param);
			}
		}

		public static void Write (this System.IO.BinaryWriter BinaryWriter, long[] Params)
		{
			BinaryWriter.Write (Params.Length);
			foreach (long Param in Params) {
				BinaryWriter.Write (Param);
			}
		}

		public static void Write (this System.IO.BinaryWriter BinaryWriter, Guid Param)
		{
			BinaryWriter.Write (Param.ToByteArray ());
		}

		public static void Write (this System.IO.BinaryWriter BinaryWriter, Guid[] Params)
		{
			BinaryWriter.Write (Params.Length);
			foreach (Guid Param in Params) {
				BinaryWriter.Write (Param.ToByteArray ());
			}
		}

		public static void Write (this System.IO.BinaryWriter BinaryWriter, System.Collections.Generic.List<Guid> Params)
		{
			BinaryWriter.Write (Params.Count);
			foreach (Guid Param in Params) {
				BinaryWriter.Write (Param.ToByteArray ());
			}
		}

		public static byte[] SHA256 (this byte[] buffer)
		{
			return System.Security.Cryptography.SHA256.Create ().ComputeHash (buffer);
		}

		public static bool In<T> (this T obj, System.Collections.Generic.IEnumerable<T> enumerable)
		{
			foreach (T tref in enumerable) {
				if (tref.Equals (obj))
					return true;
			}
			return false;
		}

		public static bool In<T> (this T obj, params T[] enumerable)
		{
			foreach (T tref in enumerable) {
				if (tref.Equals (obj))
					return true;
			}
			return false;
		}
	}
}

