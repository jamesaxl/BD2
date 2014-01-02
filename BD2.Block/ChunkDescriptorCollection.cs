//
//  ChunkDescriptorCollection.cs
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

namespace BD2.Block
{
	public class ChunkDescriptorCollection : IEnumerable<byte[]>
	{
		public int Count { get { return list.Count; } }

		System.Collections.Generic.SortedSet<byte[]> list;

		public ChunkDescriptorCollection ()
		{
			list = new SortedSet<byte[]> ();
		}

		public ChunkDescriptorCollection (IEnumerable<byte[]> List)
		{
			list = new SortedSet<byte[]> (List);
		}

		public bool DependsOn (byte[] Chunk)
		{
			if (Chunk == null)
				throw new ArgumentNullException ("Chunk");
			return list.Contains (Chunk);
		}

		/// <summary>
		/// Resolves the indirect dependencies.
		/// </summary>
		/// <returns>
		/// The indirect dependencies.
		/// </returns>
		/// <param name='Depth'>
		/// Dpendency resolution depth.negative for infinity(almost). use with caution.
		/// </param>
		//		internal bool DependsOnIndirect (ChunkRepositoryCollection CRC, byte[] Chunk, int Depth = 16)
		//		{
		//			if (Chunk == null)
		//				throw new ArgumentNullException ("Chunk");
		//			return (Depth == 0) ? false : DependsOnIndirect (CRC, Chunk, new ChunkDescriptorCollection (), Depth);
		//		}
		//
		//		bool DependsOnIndirect (ChunkRepositoryCollection CRC, byte[] Chunk, ChunkDescriptorCollection memo, int Depth = 16)
		//		{
		//			if (DependsOn (Chunk))
		//				return true;
		//			if (Depth == 0)
		//				return false;
		//			SortedSet<byte[]> LM = new SortedSet<byte[]> (memo.list.Comparer);
		//			foreach (byte[] CD in list) {
		//				if (!memo.list.Contains (CD)) {
		//					memo.list.Add (CD);
		//					//are you sure?
		//					foreach (byte[] Dependency in CRC.GetDependencies(CD))
		//						LM.Add (Dependency);
		//				}
		//			}
		//			foreach (byte[] CD in LM) {
		//				if (DependsOnIndirect (CRC, Chunk, memo, Depth - 1)) {	
		//					return true;
		//				}
		//			}
		//			return false;
		//		}
		//
		//		internal static ChunkDescriptorCollection ResolveIndirectDependencies (ChunkRepositoryCollection CRC, SortedSet<byte[]> Chunks, int Depth = 16)
		//		{
		//			ChunkDescriptorCollection memo = new ChunkDescriptorCollection (Chunks);
		//			ResolveIndirectDependencies (CRC, Chunks, memo, Depth);
		//			return memo;
		//		}
		//
		//		static void ResolveIndirectDependencies (ChunkRepositoryCollection CRC, SortedSet<byte[]> Chunks, ChunkDescriptorCollection memo, int Depth = 16)
		//		{
		//			if (Depth == 0)
		//				return;
		//			SortedSet<byte[]> LM = new SortedSet<byte[]> (memo.list.Comparer);
		//			foreach (byte[] MCD in Chunks) {
		//				foreach (byte[] CCD in CRC.GetDependencies(MCD)) {
		//					if (!memo.list.Contains (CCD)) {
		//						memo.list.Add (CCD);
		//						LM.Add (CCD);
		//					}
		//				}
		//			}
		//			ResolveIndirectDependencies (CRC, LM, memo, Depth - 1);
		//		}

		public static ChunkDescriptorCollection operator- (ChunkDescriptorCollection Left, ChunkDescriptorCollection Right)
		{
			if (Right == null)
				throw new ArgumentNullException ("Right");
			if (Left == null)
				throw new ArgumentNullException ("Left");
			ChunkDescriptorCollection RDC = new ChunkDescriptorCollection (Left.list);
			RDC.list.ExceptWith (Right);
			return RDC;
		}

		public static ChunkDescriptorCollection operator| (ChunkDescriptorCollection Left, ChunkDescriptorCollection Right)
		{
			if (Right == null)
				throw new ArgumentNullException ("Right");
			if (Left == null)
				throw new ArgumentNullException ("Left");
			ChunkDescriptorCollection RDC = new ChunkDescriptorCollection (Left.list);
			RDC.list.IntersectWith (Right);
			return RDC;
		}

		public static ChunkDescriptorCollection operator& (ChunkDescriptorCollection Left, ChunkDescriptorCollection Right)
		{
			if (Right == null)
				throw new ArgumentNullException ("Right");
			if (Left == null)
				throw new ArgumentNullException ("Left");
			ChunkDescriptorCollection RDC = new ChunkDescriptorCollection (Left.list);
			RDC.list.UnionWith (Right);
			return RDC;
		}

		public static ChunkDescriptorCollection operator^ (ChunkDescriptorCollection Left, ChunkDescriptorCollection Right)
		{
			if (Right == null)
				throw new ArgumentNullException ("Right");
			if (Left == null)
				throw new ArgumentNullException ("Left");
			return (Left | Right) - (Left & Right);
		}

		public IEnumerator<byte[]> GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return list.GetEnumerator ();
		}
	}
}
