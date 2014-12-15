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
using System.IO;
using System.Collections.Generic;

namespace BD2.Core
{
	public static class Extensions
	{
		public static byte[] Combine (this byte[][] parts)
		{
			if (parts == null)
				return null;
			int partsLen = 0;
			for (int n = 0; n != parts.Length; n++) {
				partsLen += parts [n].Length + sizeof(int);
			}
			byte[] metadata = new byte[sizeof(int) + partsLen];
			MemoryStream metastream = new MemoryStream (metadata, true);
			BinaryWriter metawriter = new BinaryWriter (metastream);
			metawriter.Write (parts.Length);
			for (int n = 0; n != parts.Length; n++) {
				metawriter.Write (parts [n].Length);
				metawriter.Write (parts [n]);
			}
			return metadata;
		}

		public static byte[][] Expand (this byte[] array)
		{
			if (array == null)
				return null;
			MemoryStream metastream = new MemoryStream (array);
			BinaryReader metareader = new BinaryReader (metastream);
			int partCount = metareader.ReadInt32 ();
			byte[][] parts = new byte[partCount][];
			for (int n = 0; n != partCount; n++) {
				parts [n] = metareader.ReadBytes (metareader.ReadInt32 ());
			}
			return parts;
		}

		static char[] hexes = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

		public static string ToHexadecimal (this byte[] bytes)
		{
			char[] Result = new char[bytes.Length << 1];
			int Offset = 0;
			for (int i = 0; i != bytes.Length; i++) {
				Result [Offset++] = hexes [bytes [i] >> 4];
				Result [Offset++] = hexes [bytes [i] & 0x0F];
			}
			return new string (Result);
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

		public static byte[] SHA256 (this string text)
		{
			return SHA256 (System.Text.Encoding.Unicode.GetBytes (text));
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

		public static T First<T> (this System.Collections.Generic.IEnumerator<T> obj)
		{
			obj.MoveNext ();
			return obj.Current;
		}

		public static SortedSet<T> ToSet<T> (this System.Collections.Generic.IEnumerator<T> obj)
		{
			SortedSet<T> s = new SortedSet<T> ();
			while (obj.MoveNext ()) {
				s.Add (obj.Current);
			}
			return s;
		}
	}
}

