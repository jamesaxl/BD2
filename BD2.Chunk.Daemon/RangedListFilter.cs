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
 * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * */
using System;
using System.Collections.Generic;
using BD2.Common;

namespace BD2.Chunk.Daemon
{
	public class RangedListFilter : IRangedFilter
	{
		byte[] first;
		byte[] last;
		int skipBytes;
		SortedSet<byte[]> items = new SortedSet<byte[]> ();

		public SortedSet<byte[]> Items {
			get {
				return new SortedSet<byte[]> (items);
			}
		}

		public RangedListFilter (IEnumerable<IEnumerable<byte[]>> buckets)
		{
			if (items == null)
				throw new ArgumentNullException ("items");
			this.items = new SortedSet<byte[]> (items);
			byte[] min = null;
			byte[] max = null;
			foreach (IEnumerable<byte[]> bucket in buckets) {
				items.UnionWith (bucket);
			}
			int maxItemLength = int.MaxValue;
			//TODO: this procedure needs a hell lot of optimization and security fixes
			foreach (byte[] item in items) {
				if (min == null) {
					min = item;
					max = item;
				}
				if (min.CompareTo (item) < 0)
					min = item;
				if (max.CompareTo (item) > 0)
					max = item;
				if (item.Length > maxItemLength)
					maxItemLength = item.Length;
			}
			first = min;
			last = max;
			skipBytes = Math.Min (min.IdenticalBytesWith (max), maxItemLength);
		}

		public static RangedListFilter Deserialize (byte[] bytes)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (bytes, false)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					int itemCount = BR.ReadInt32 ();
					int skipBytes = BR.ReadInt32 ();
					byte[] skippedBytes = BR.ReadBytes (skipBytes);
					byte[][] items = new byte[itemCount][];
					for (int n = 0; n != itemCount; n++) {
						int itemLength = BR.ReadInt32 ();
						byte[] item = new byte[skipBytes + itemLength];
						System.Buffer.BlockCopy (skippedBytes, 0, item, 0, skipBytes);
						BR.Read (item, skipBytes, itemLength);
						items [n] = item;
					}
					return new RangedListFilter (new [] { items });
				}
			}
		}
		#region IRangedFilter implementation
		public float Contains (byte[] chunkID)
		{
			return items.Contains (chunkID) ? 1 : 0;
		}

		public byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					lock (items) {
						BW.Write (items.Count);
						BW.Write (skipBytes);
						BW.Write (first, 0, skipBytes);
						foreach (byte[] item in items) {
							BW.Write (item.Length - skipBytes);
							BW.Write (item, skipBytes, item.Length - skipBytes);
						}
					}
					return MS.GetBuffer ();
				}
			}
		}

		public string FilterTypeName {
			get {
				return "RangeList";
			}
		}

		public byte[] FirstChunk {
			get {
				return first;
			}
		}

		public byte[] LastChunk {
			get {
				return last;
			}
		}
		#endregion
		#region IComparable implementation
		int IComparable.CompareTo (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			IRangedFilter IRF = (IRangedFilter)obj;
			return first.CompareTo (IRF.FirstChunk);
		}
		#endregion
	}
}

