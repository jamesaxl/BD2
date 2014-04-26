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

namespace BD2.Daemon
{
	/// <summary>
	/// This class is NOT guaranteed to be thread-safe
	/// </summary>
	public class OStream : Stream
	{
		StreamPair streamPair;
		MemoryStream bufferStream;

		public StreamPair StreamPair {
			get {
				return streamPair;
			}
		}

		internal OStream (StreamPair streamPair)
		{
			if (streamPair == null)
				throw new ArgumentNullException ("streamPair");
			this.streamPair = streamPair;
		}
		#region implemented abstract members of Stream
		public override void Flush ()
		{
			throw new NotSupportedException ();
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			bool hasData = bufferStream != null;
			if (!hasData) {
				byte[] bytes = streamPair.Dequeue ();
				hasData = bytes != null;
				if (hasData)
					bufferStream = new MemoryStream (bytes);
				else
					return 0;
			}
			int read = 0;
			while ((read != count) && hasData) {
				bufferStream.Read (buffer, read, (int)Math.Min (count, bufferStream.Length - bufferStream.Position));
				if (bufferStream.Length == bufferStream.Position) {
					byte[] bytes = streamPair.Dequeue ();
					hasData = bytes != null;
					if (hasData)
						bufferStream = new MemoryStream (bytes);
					else {
						bufferStream = null;
						break;
					}
				}
			}
			return read;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ();
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException ();
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException ();
		}

		public override bool CanRead {
			get {
				return true;
			}
		}

		public override bool CanSeek {
			get {
				return false;
			}
		}

		public override bool CanWrite {
			get {
				return false;
			}
		}

		public override long Length {
			get {
				throw new NotSupportedException ();
			}
		}

		public override long Position {
			get {
				throw new NotSupportedException ();
			}
			set {
				throw new NotSupportedException ();
			}
		}
		#endregion
	}
}

