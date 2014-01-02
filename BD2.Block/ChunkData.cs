//
//  ChunkData.cs
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
using System.IO;
using System.Collections.Generic;

namespace BD2.Block
{
	public class ChunkData
	{
		byte[] id;
		//		Guid author;
		//		string comment;
		//		SortedDictionary<byte[], byte[]> extendedAttributes;
		//		RawProxy.ProxyCombination[] proxyCombinations;
		//		byte[][] dependencies;
		public virtual int Version {
			get {
				return 0x00000001;
			}
		}

		public byte[] ID {
			get {
				return (byte[])id.Clone ();
			}
		}
		//		public Guid Author {
		//			get {
		//				return author;
		//			}
		//		}
		//
		//		public string Comment {
		//			get {
		//				return comment;
		//			}
		//		}
		//
		//		public virtual IEnumerable<KeyValuePair<byte[], byte[]>> ExtendedAttributes {
		//			get {
		//				return extendedAttributes;
		//			}
		//		}
		//
		//		private RawProxy.ProxyCombination[] ProxyCombinations {
		//			get {
		//				return proxyCombinations;
		//			}
		//		}
		//
		//		internal byte[][] Dependencies {
		//			get {
		//				return dependencies;
		//			}
		//		}
		//
		//		public IEnumerator<byte[]> EnumerateDependencies {
		//			get {
		//				return  (IEnumerator<byte[]>)(dependencies.GetEnumerator ());
		//			}
		//		}
		private ChunkData ()
		{

		}
		//		public static ChunkData GetData (System.IO.Stream RawData, bool MetaOnly)
		//		{
		//			//  Version 0x00000001 chunk structure:
		//			//  4 bytes magic
		//			//  4 bytes version
		//
		//			System.IO.BinaryReader BR = new BinaryReader (RawData);
		//			uint Magic = BR.ReadUInt32 ();
		//			if (Magic != 0xc3174e4a)
		//				throw new InvalidDataException ("Wrong magic bytes");
		//			uint Version = BR.ReadUInt32 ();
		//			switch (Version) {
		//			case 0x00000001:
		//				return GetDatav1 (BR, true, MetaOnly);
		//			default:
		//				throw new Exception (string.Format ("version {0} structures are not supported by this assembly.", Version));
		//			}
		//		}
		//
		//		public static ChunkData GetDatav1 (System.IO.BinaryReader BR, bool HeaderStripped, bool MetaOnly)
		//		{
		//
		//			//  4 bytes dependency count
		//			//    per dependency:
		//			//    32 bytes object id
		//
		//			//  16 bytes author
		//			//  4 bytes comments length
		//			//  [comments length] bytes comment
		//
		//			//  4 bytes proxy configuration count
		//			//    per proxy configuration:
		//			//    16 bytes type guid
		//			//    4 bytes initialization vector length
		//			//    [initialization vector length] bytes initialization vector
		//
		//			//  4 bytes proxy combination count
		//			//    per proxy combination
		//			//    4 byte proxy count
		//			//    per proxy
		//			//      4 byte proxy id
		//
		//			//  4 bytes extended attributes count
		//			//    per extended attribute:
		//			//    4 bytes key length
		//			//    [key length] bytes key
		//			//    4 bytes value length
		//			//    [value length] bytes value
		//			//  4 bytes object count
		//			//    per object:
		//			//    16 bytes id
		//			//    4 bytes proxy combination id
		//			//    4 bytes length<TYPEID LENGTH(16 BYTES) IS SUBTRACTED>
		//			//    [length] bytes object data
		//			//
		//			if (!HeaderStripped)
		//				BR.ReadInt64 ();//read 8 bytes without accessing the heap
		//			int DependencyCount = BR.ReadInt32 ();
		//			byte[][] Dependencies = new byte[DependencyCount][];
		//			for (int n = 0; n != DependencyCount; n++) {
		//				Dependencies [n] = BR.ReadBytes (32);
		//			} 
		//																																												
		//			Guid Author = new Guid (BR.ReadBytes (16));
		//			BR.ReadBytes (3);//skip padding
		//			int CommentsLength = BR.ReadInt32 ();
		//			string Comment = System.Text.Encoding.Unicode.GetString (BR.ReadBytes (CommentsLength));
		//
		//			//proxy configurations
		//			int proxyConfigurationsCount = BR.ReadInt32 ();
		//			Tuple<Guid, byte[]>[] proxyConfigurations = new Tuple<Guid, byte[]>[proxyConfigurationsCount];
		//			for (int n = 0; n != proxyConfigurationsCount; n++) {
		//				proxyConfigurations [n] = new Tuple<Guid, byte[]> (new Guid (BR.ReadBytes (16)), BR.ReadBytes (BR.ReadInt32 ()));
		//			}
		//
		//			//proxy combinations
		//			int proxyCombinationsCount = BR.ReadInt32 ();
		//			int[][] proxyCombinations = new int[proxyCombinationsCount][];
		//			for (int n = 0; n != proxyCombinationsCount; n++) {
		//				int itemCount = BR.ReadInt32 ();
		//				int[] Current = new int[itemCount];
		//				for (int m = 0; m != itemCount; m++) {
		//					Current [m] = BR.ReadInt32 ();
		//				}
		//				proxyCombinations [n] = Current;
		//			}
		//
		//			//extended attributes
		//			int ExtendedAttributesCount = BR.ReadInt32 ();
		//			SortedDictionary<byte[], byte[]> ExtendedAttributes = null;
		//			if (ExtendedAttributesCount != 0) {
		//				ExtendedAttributes = new SortedDictionary<byte[], byte[]> ();
		//				for (int EAID = 0; EAID != ExtendedAttributesCount; EAID++) {
		//					int KeyLength = BR.ReadInt32 ();
		//					byte[] Key = BR.ReadBytes (KeyLength);
		//					int ValueLength = BR.ReadInt32 ();
		//					byte[] Value = BR.ReadBytes (ValueLength);
		//					ExtendedAttributes.Add (Key, Value);
		//				}
		//			}
		//			SortedSet<BaseDataObjectDescriptor> BaseDataObjectDescriptors = new SortedSet<BaseDataObjectDescriptor> ();
		//			if (!MetaOnly) {
		//				//objects
		//				int ObjectCount = BR.ReadInt32 ();
		//				for (int ObjectIndex = 0; ObjectIndex != ObjectCount; ObjectIndex++) {
		//					//Guid ObjectType = new Guid(BR.ReadBytes(16));
		//					Guid ObjectID = new Guid (BR.ReadBytes (16));
		//					int ProxyCombination = BR.ReadInt32 ();
		//					int Length = BR.ReadInt32 ();
		//					byte[] RawData = BR.ReadBytes (Length);
		//					BaseDataObjectDescriptor desc = new BaseDataObjectDescriptor (ObjectID, RawData, ProxyCombination);
		//					BaseDataObjectDescriptors.Add (desc);
		//				}
		//				if (BR.BaseStream.Position != BR.BaseStream.Length) {
		//					throw new Exception ("Invalid chunk");
		//				}
		//			}
		//			return new ChunkData () {
		//				author = Author,
		//				comment = Comment,
		//				dependencies = Dependencies,
		//				extendedAttributes = ExtendedAttributes,
		//				objectDescriptors = BaseDataObjectDescriptors
		//			};
		//		}
		//
		//		public static ChunkData FromTransaction (Transaction transaction)
		//		{
		//			ChunkData CD = new ChunkData ();
		//			CD.id = null;
		//			CD.author = transaction.Author;
		//			CD.comment = transaction.Comment;
		//			return CD;
		//		}
	}
}
