//leveldb-sharp
//
//Copyright (c) 2013 Behrooz Amoozad <behrooz0az@gmail.com>
//Copyright (c) 2012 Mirco Bauer <meebey@meebey.net>
//Copyright (c) 2011 The LevelDB Authors
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are
//met:
//
//   * Redistributions of source code must retain the above copyright
//     notice, this list of conditions and the following disclaimer.
//   * Redistributions in binary form must reproduce the above
//     copyright notice, this list of conditions and the following disclaimer
//     in the documentation and/or other materials provided with the
//     distribution.
//   * Neither the name of Google Inc. nor the names of its
//     contributors may be used to endorse or promote products derived from
//     this software without specific prior written permission.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
//A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
//OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
//SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
//LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
//DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
//THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
//OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;

namespace LevelDB
{
	public sealed class Slice : IDisposable
	{

		public bool OwnsHandle { get; private set; }

		public IntPtr Handle { get; private set; }

		public int Length { get; private set; }

		public bool Disposed { get; private set; }

		public Slice (IntPtr handle, int length, bool ownsHandle)
		{
			Handle = handle;
			Length = length;
			OwnsHandle = ownsHandle;
		}

		public void Read (byte[] buffer, int offset, int length, int sliceOffset)
		{
			if (sliceOffset < 0)
				throw new ArgumentOutOfRangeException ("sliceOffset", "SliceOffset cannot be negative.");
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "Offset < 0");
			if ((offset + length > buffer.Length) ||
			    (sliceOffset + length > this.Length))
				throw new ArgumentOutOfRangeException ("length", "Length out of range of Slice/Buffer");
			System.Runtime.InteropServices.Marshal.Copy (Handle + sliceOffset, buffer, offset, length);
		}

		public void Read (byte[] buffer, int offset, int length)
		{
			Read (buffer, offset, length, 0);
		}

		~Slice ()
		{
			Dispose ();
		}

		#region IDisposable implementation

		~Slice ()
		{
			Dispose (false);
		}

		void Dispose (bool disposing)
		{
			var disposed = Disposed;
			if (disposed) {
				return;
			}
			Disposed = true;

			if (disposing) {
				// free managed

			}
			// free unmanaged
			if (OwnsHandle) {
				var handle = Handle;
				if (handle != IntPtr.Zero) {
					Handle = IntPtr.Zero;
					Native.leveldb_free (handle);
				}
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		#endregion
	}
}
