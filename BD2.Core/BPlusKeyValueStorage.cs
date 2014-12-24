// /*
//  * Copyright (c) 2014 Behrooz Amoozad
//  * All rights reserved.
//  *
//  * Redistribution and use in source and binary forms, with or without
//  * modification, are permitted provided that the following conditions are met:
//  *     * Redistributions of source code must retain the above copyright
//  *       notice, this list of conditions and the following disclaimer.
//  *     * Redistributions in binary form must reproduce the above copyright
//  *       notice, this list of conditions and the following disclaimer in the
//  *       documentation and/or other materials provided with the distribution.
//  *     * Neither the name of the bd2 nor the
//  *       names of its contributors may be used to endorse or promote products
//  *       derived from this software without specific prior written permission.
//  *
//  * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//  * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//  * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//  * DISCLAIMED. IN NO EVENT SHALL Behrooz Amoozad BE LIABLE FOR ANY
//  * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//  * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//  * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//  * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//  * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//  * */
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CSharpTest.Net.Serialization;
using System.IO.MemoryMappedFiles;
using System.IO;
using CSharpTest.Net.Collections;

namespace BD2.Core
{
	public sealed class BPlusKeyValueStorage : KeyValueStorage<byte[]>, IDisposable
	{
		long capacity;

		[StructLayout (LayoutKind.Explicit, Pack = 1)]
		struct MetaHeader
		{
			[FieldOffset (0)]
			public ulong MAGIC;
			[FieldOffset (8)]
			public long WriteOffset;
			[FieldOffset (16)]
			public int GrowRate;

		}

		class ObjectHeader
		{
			public bool Deleted;
			public int Length;
			public byte[] Key;
		}


		const long reservedBytes = 65536;


		DatabasePath path;
		BPlusTree<byte[], long> index;
		MemoryMappedFile mmfBlock;
		MemoryMappedViewAccessor maMeta;
		MetaHeader mh;

		string name;

		public BPlusKeyValueStorage (DatabasePath path, string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (path == null)
				throw new ArgumentNullException ("path");
			this.name = name;
			this.path = path;
			Initialize ();
		}

		#region implemented abstract members of KeyValueStorage

		FileStream fs;
		BPlusTree<byte[], long>.OptionsV2 options;

		public override void Initialize ()
		{
			if (!Directory.Exists (path.Path))
				Directory.CreateDirectory (path.Path);
			//string indexFileName = path.CreatePath ("index").Path;
			string blockFileName = path.CreatePath (name).Path;
			Console.WriteLine ("Name: {0}", name);
			//Console.WriteLine ("Index: {0}", indexFileName);
			Console.WriteLine ("Block: {0}", blockFileName);
			//bool indexExists = File.Exists (indexFileName);
			bool blockExists = File.Exists (blockFileName);
			options = new BPlusTree<byte[], long>.OptionsV2 (BytesSerializer.RawBytes, PrimitiveSerializer.Int64);
			options.KeyComparer = ByteSequenceComparer.Shared;
			options.StorageType = StorageType.Memory;
			index = new BPlusTree<byte[], long> (options);
			index.DebugSetOutput (Console.Out);
			Console.WriteLine ("Count: {0}", index.Count);
			Console.WriteLine ();
			fs = File.Open (blockFileName, FileMode.OpenOrCreate);
			if ((capacity = fs.Length) == 0)
				fs.SetLength (capacity = 0x110000);
			fs.Close ();
			mmfBlock = MemoryMappedFile.CreateFromFile (blockFileName, FileMode.Open);
			maMeta = mmfBlock.CreateViewAccessor (0, reservedBytes, MemoryMappedFileAccess.ReadWrite);
			if (!blockExists) {
				mh.MAGIC = 0x88DFB78311EFF07A;
				mh.WriteOffset = 0;
				mh.GrowRate = 0x100000;
				maMeta.Write <MetaHeader> (0, ref mh);
			} else {
				maMeta.Read<MetaHeader> (0, out mh);
				if (mh.MAGIC != 0x88DFB78311EFF07A)
					throw new InvalidDataException ("file magic doesn't match.");
			}
			CreateIndex ();
		}

		void CreateIndex ()
		{
			long offset = 0;
			if (mh.WriteOffset == 0)
				return;
			using (MemoryMappedViewStream maData = mmfBlock.CreateViewStream (reservedBytes, mh.WriteOffset)) {
				using (System.IO.BinaryReader br = new BinaryReader (maData)) {
					while ((offset = br.BaseStream.Position) != mh.WriteOffset) {
						bool Deleted = br.ReadBoolean ();
						br.BaseStream.Seek (3, SeekOrigin.Current);
						int Length = br.ReadInt32 ();
						if (!Deleted) {
							byte[] Key = br.ReadBytes (32);
							index.Add (Key, offset);
							br.BaseStream.Seek (Length, SeekOrigin.Current);
						} else {
							br.BaseStream.Seek (Length + 32, SeekOrigin.Current);
						}
					}
				}
			}
		}

		public override void Dispose ()
		{
			maMeta.Dispose ();
			mmfBlock.Dispose ();
			index.Dispose ();
		}

		public override IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator ()
		{
			lock (index)
				foreach (var t in index)
					yield return new KeyValuePair<byte[], byte[]> (t.Key, Get (t.Key));
		}

		public override IEnumerator<byte[]> EnumerateKeys ()
		{
			lock (index)
				foreach (var t in index.Keys)
					yield return t;
		}

		void EnsureCapacity (long required)
		{
			lock (index) {
				long newLength = capacity;
				while (newLength < required) {
					newLength += mh.GrowRate;
				}
				if (newLength != capacity) {
					mmfBlock.Dispose ();
					string blockFileName = path.CreatePath ("block").Path;
					FileStream fs = File.Open (blockFileName, FileMode.OpenOrCreate);
					if ((capacity = fs.Length) == 0)
						fs.SetLength (newLength);
					fs.Close ();
					mmfBlock = MemoryMappedFile.CreateFromFile (blockFileName, FileMode.Open);
				}
			}
		}

		public override void Put (byte[] key, byte[] value)
		{
			try {
				Console.WriteLine ("{0}:", name);
				Console.Write ("{0}:", key.ToHexadecimal ());
				try {
					Console.WriteLine ("{0}:", System.Text.Encoding.Unicode.GetString (value));
				} catch {
					Console.WriteLine ("{0}", value.ToHexadecimal ());
				}
			} catch {
				Console.WriteLine ("XXX");
			}

			long OO;
			lock (index)
				if (index.TryGetValue (key, out OO)) {
					//TODO:resize object if we can
					Delete (key);
					Put (key, value);
				} else {
					ObjectHeader ObjectHeader = new ObjectHeader ();
					ObjectHeader.Deleted = false;
					ObjectHeader.Length = value.Length;
					ObjectHeader.Key = key;
					int ObjectSize = value.Length;
					const int ObjectHeaderSize = 40;
					OO = mh.WriteOffset;
					mh.WriteOffset += ObjectSize + ObjectHeaderSize;
					EnsureCapacity (mh.WriteOffset);
					MemoryMappedViewStream accessor = mmfBlock.CreateViewStream (reservedBytes + OO, ObjectSize + ObjectHeaderSize, MemoryMappedFileAccess.Write);
					byte[] ObjectHeaderBytes = new byte[ObjectHeaderSize];
					System.IO.BinaryWriter bw = new BinaryWriter (new MemoryStream (ObjectHeaderBytes, true));
					bw.Write (ObjectHeader.Deleted);
					bw.BaseStream.Seek (3, SeekOrigin.Current);
					bw.Write (ObjectHeader.Length);
					bw.Write (ObjectHeader.Key);
					accessor.Write (ObjectHeaderBytes, 0, ObjectHeaderSize);
					accessor.Write (value, 0, value.Length);
					accessor.Flush ();
					accessor.Dispose ();
					maMeta.Write (0, ref mh);
					maMeta.Flush ();
					index.Add (key, OO);
					index.Commit ();
				}

		}

		public override byte[] Get (byte[] key)
		{
			lock (index) {
				long OO = index [key];
				//TODO: FIX THE UPPERBOUND
				MemoryMappedViewAccessor accessor = mmfBlock.CreateViewAccessor (reservedBytes + OO, capacity - (reservedBytes + OO), MemoryMappedFileAccess.Read);
				if (accessor.ReadBoolean (0))
					throw new InvalidDataException ();
				const int ObjectHeaderSize = 40;
				byte[] outArr = new byte[accessor.ReadInt32 (4)];
				accessor.ReadArray<byte> (ObjectHeaderSize, outArr, 0, outArr.Length);
				accessor.Dispose ();
				return outArr;
			}
		}

		public override void Delete (byte[] key)
		{
			lock (index) {
				long OO = index [key];
				MemoryMappedViewAccessor accessor = mmfBlock.CreateViewAccessor (reservedBytes + OO, 1, MemoryMappedFileAccess.ReadWrite);
				accessor.Write (0, true);
				accessor.Flush ();
				accessor.Dispose ();
				index.Remove (key);
				index.Commit ();
			}
		}

		#endregion

		#region implemented abstract members of KeyValueStorage

		public override IEnumerable<KeyValuePair<byte[], byte[]>> EnumerateFrom (byte[] start)
		{
			lock (index)
				foreach (var t in index.EnumerateFrom (start))
					yield return new KeyValuePair<byte[], byte[]> (t.Key, Get (t.Key));
		}

		public override IEnumerable<KeyValuePair<byte[], byte[]>> EnumerateRange (byte[] start, byte[] end)
		{
			lock (index)
				foreach (var t in index.EnumerateRange (start, end))
					yield return new KeyValuePair<byte[], byte[]> (t.Key, Get (t.Key));
		}

		public override int Count {
			get {
				lock (index)
					return index.Count;
			}
		}

		#endregion
	}
}

