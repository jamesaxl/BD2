/*
  * Copyright (c) 2014 Behrooz Amoozad
  * All rights reserved.
  *
  * Redistribution and use in source and binary forms, with or without
  * modification, are permitted provided that the following conditions are met:
  *    * Redistributions of source code must retain the above copyright
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
using System.Collections.Generic;

namespace BD2.Core
{
	public class ChunkData
	{
		readonly byte[] id;
		readonly byte[][] dependencies;
		readonly ChunkRepository source;
		readonly byte[] data;
		readonly byte[][] signatures;

		public byte[] ID {
			get {
				return id;
			}
		}

		public byte[][] Dependencies {
			get {
				return dependencies;
			}
		}

		public ChunkRepository Source {
			get {
				return source;
			}
		}

		public byte[] Data {
			get {
				return data;
			}
		}

		public byte[][] Signatures {
			get {
				return signatures;
			}
		}

		public ChunkData (byte[] id, byte[] data, byte[][] dependencies, byte[][] signatures, ChunkRepository source)
		{
			if (id == null)
				throw new ArgumentNullException ("id");
			if (data == null)
				throw new ArgumentNullException ("data");
			if (dependencies == null)
				throw new ArgumentNullException ("dependencies");
			for (int n = 0; n != dependencies.Length; n++) {
				if (dependencies [n] == null) {
					throw new ArgumentNullException (string.Format ("dependencies[{0}]", n));
				}
			}
			if (source == null)
				throw new ArgumentNullException ("source");
			if (signatures == null)
				throw new ArgumentNullException ("signatures");
			for (int n = 0; n != signatures.Length; n++) {
				if (signatures [n] == null) {
					throw new ArgumentNullException (string.Format ("signatures[{0}]", n));
				}
			}

			this.id = id;
			this.data = data;
			this.dependencies = dependencies;
			this.source = source;
			this.signatures = signatures;
		}
	}
}

