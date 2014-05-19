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
using System.Collections.Generic;

namespace BD2.Frontend.Table.Model
{
	public abstract class ValueSerializerBase
	{
		public abstract byte TypeToID (Type type);

		public abstract Type IDToType (byte id);

		public abstract object Deserialize (byte typeID, System.IO.Stream stream);

		public object[] DeserializeArray (byte[] buffer)
		{
			System.IO.MemoryStream MS = new System.IO.MemoryStream (buffer);
			List<object> objects = new List<object> (); 
			while (MS.Position < MS.Length) {
				int Byte = MS.ReadByte ();
				objects.Add (Deserialize ((byte)Byte, MS));
			}
			return objects.ToArray ();
		}

		public abstract void Serialize (object obj, out byte typeID, out byte[] bytes);

		public void SerializeArray (object[] objects, out byte[] bytes)
		{
			System.IO.MemoryStream MS = new System.IO.MemoryStream ();
			foreach (object obj in objects) {
				byte tid;
				byte[] buf;
				Serialize (obj, out tid, out buf);
				MS.WriteByte (tid);
				MS.Write (buf, 0, buf.Length);
			}
			bytes = MS.ToArray ();
		}
	}
}

