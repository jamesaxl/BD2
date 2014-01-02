//
//  Node.cs
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
using System.Collections.Generic;

namespace BD2.FullTextSearch
{
	class Node
	{
		const int threshhold = 512;
		int count = 0;
		uint[] line_numbers = new uint[8];
		bool isRoot { get { return parent == null; } }

		Node parent;
		string part;
		string path;

		public Node (Node parent, string part)
		{
			this.parent = parent;
			this.part = part;
		}

		public Node ()
		{
			part = null;
			path = "";
			parent = null;
		}

		public string extractPart(string word) {
			if (word.StartsWith (path)) {
				return word.Remove (0, path.Length);
			}
			//error
			return null;
		}
		public Node GetNodeByPath(string path) {
		}
		static Node Getsub (string part)
		{
			if (!part.StartsWith (path))
				Node L;
			if (!subs.TryGetValue (part, out L)) {
				L = new Node ();
				subs.Add (word, L);
			}
			return L;
		}
		public Node Insert (string word, uint lineNumber)
		{
			string subpart = extractPart (word);
			Node Node = Getsub (subpart);
			if (subs.Count > some_threshhold) {
				//merge two or more words into one and make the new child adopt the old ones
			}
			Node.Insert (lineNumber);
			return Node;
		}

		SortedDictionary<string, Node> subs = new SortedDictionary<string, Node> ();

		static void gethashes (uint lineNumber, out uint id0, out uint id1)
		{
			id0 = ((uint)lineNumber.GetHashCode () % (threshhold * sizeof(uint) * 8));
			id1 = ((uint)(lineNumber.GetHashCode () >> 16) % (threshhold * sizeof(uint) * 8));
		}

		static void insert_hash (IList<uint> lineNumbers, uint lineNumber)
		{
			uint id0, id1;
			gethashes (lineNumber, out id0, out id1);
			lineNumbers [(int)(id0 >> 5)] |= (uint)((long)1) << (int)(id0 & 31);
			lineNumbers [(int)(id1 >> 5)] |= (uint)((long)1) << (int)(id1 & 31);
		}

		public float Contains (uint lineNumber)
		{
			if (count <= threshhold) {
				//TODO:do binary search here
				for (int n =0; n != count; n++)
					if (line_numbers [n] == lineNumber)
						return 1;
				return 0;
			} else {
				uint id0, id1;
				gethashes (lineNumber, out id0, out id1);
				bool v0 = ((line_numbers [(int)(id0 >> 5)]) & (uint)(((long)1) << (int)(id0 & 31))) != 0;
				bool v1 = ((line_numbers [(int)(id1 >> 5)]) & (uint)(((long)1) << (int)(id1 & 31))) != 0;
				return v0 && v1 ? 1 : 0;
			}
		}

		static uint[] ToBloomFilter (IList<uint> lineNumbers)
		{
			uint[] mLineNumbers = new uint[threshhold];
			for (int n = 0; n != lineNumbers.Count; n++)
				insert_hash (mLineNumbers, lineNumbers [n]);
			return mLineNumbers;
		}
		public void Insert (uint lineNumber)
		{
			if (count > threshhold) {
				insert_hash (line_numbers, lineNumber);
			} else {
				if (count > line_numbers.Length) {
					if (line_numbers.Length >= threshhold)
						line_numbers = ToBloomFilter (line_numbers);
					else
						Array.Resize<uint> (ref line_numbers, count * 2);
				}
				line_numbers [count] = lineNumber;
			}
			count++;
		}

		static uint[] ArrayIntersect (IList<uint> l, IList<uint> r)
		{
			uint[] p = new uint[l.Count];
			for (int N = 0; N != threshhold; N++)
				p [N] = l [N] & r [N];
			return p;
		}

		public Node Intersect (Node other)
		{
			Node newL = new Node ();
			newL.count = Math.Max (this.count, other.count);
			if ((count > threshhold) && (other.count > threshhold)) {
				newL.line_numbers = ArrayIntersect (other.line_numbers, line_numbers);
			} else if ((count > threshhold) && other.count <= threshhold) {
				newL.line_numbers = ArrayIntersect (line_numbers, ToBloomFilter (other.line_numbers));
			} else if (count <= threshhold && (other.count > threshhold)) {
				newL.line_numbers = ArrayIntersect (ToBloomFilter (line_numbers), other.line_numbers);
			} else {
				int newCount = count + other.count;
				if (newCount > threshhold) {
					newL.line_numbers = ArrayIntersect (ToBloomFilter (line_numbers), ToBloomFilter (other.line_numbers));
				} else {
					newL.count = newCount;
					newL.line_numbers = new uint[newCount];
					Array.Copy (line_numbers, newL.line_numbers, count);
					Array.Copy (other.line_numbers, 0, newL.line_numbers, count, other.count);
				}
			}
			return newL;
		}
	}
}
