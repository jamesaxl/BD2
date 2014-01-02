/*
 * Copyright (c) 2013-2014 Behrooz Amoozad
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
using BSO;
using System.IO;

namespace BD2
{
	public class RemoteChunk : ChunkDescriptor
	{
		string path;

		public string Path {
			get {
				return path;
			}
		}

		Peer repo;

		public RemoteChunk (Peer Repo, long PrivID)
		{
			if (Repo == null)
				throw new ArgumentNullException ("Repo");
			if (Path == null)
				throw new ArgumentNullException ("Path");
			repo = Repo;
			path = Path;
			mPrivID = PrivID;
		}

		byte[] data;

		public byte[] Data {
			get {
				if (data == null) {
					byte[] Bytes;
					if (wData == null) {
						Bytes = System.IO.File.ReadAllBytes (path);
						data = Bytes;
						hash = null;
						return data;
					} else {
						Bytes = (byte[])wData.Target;
						if (Bytes == null) {
							Bytes = System.IO.File.ReadAllBytes (path);
							wData.Target = Bytes;
						}
						return Bytes;
					}
				}
				return data;
			} 
		}

		WeakReference wData;

		public override void GoConservative ()
		{
			if (data != null) {
				wData = new WeakReference (data);
				data = null;
			}
		}

		public override void GoLive ()
		{
			data = Data;
			wData = null;
		}

		public RemoteChunk (Peer Repo, long PrivID, string Path, byte[] Data)
		{
			data = Data;
			repo = Repo;
			path = Path;
			mPrivID = PrivID;
			//sync
			System.IO.File.WriteAllBytes (Path, Data);
		}

		public override long Length {
			get {
				return data.Length;
			}
		}

		byte[] hash;

		public override byte[] Hash {
			get {
				if (hash == null)
					hash = Data.SHA1 ();
				return (byte[])hash.Clone ();
			}
		}

		public override Stream GetData ()
		{
			bool L = false;
			return new ParallelPagedEnumerator<Tuple<int, byte[]>> (Data, (r) => {
				if (L == false) {
					L = true;
					return new Tuple<int, Byte[]> (0, (byte[])r);
				}
				return null;
			}, 8, 2);
		}

		private long mPrivID;

		public override long PrivID { get { return mPrivID; } }
	}
}

