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

namespace BD2.RawProxy
{
	public sealed class RawProxyAttribute : Attribute
	{
		static System.Collections.Concurrent.ConcurrentDictionary<Guid, RawProxyAttribute> attribs = new System.Collections.Concurrent.ConcurrentDictionary<Guid, RawProxyAttribute> ();
		Guid guid;

		public Guid Guid {
			get {
				return guid;
			}
		}

		string deserializeMethodName;

		public string DeserializeMethodName {
			get {
				return deserializeMethodName;
			}
		}

		System.Reflection.MethodInfo deserializeMethod;

		public System.Reflection.MethodInfo DeserializeMethod {
			get {
				return deserializeMethod;
			}
		}

		public RawProxyAttribute (Type type, string guid, string deserializeMethodName)
		{
			if (guid == null)
				throw new ArgumentNullException ("guid");
			if (deserializeMethodName == null)
				throw new ArgumentNullException ("deserializeMethodName");
			this.deserializeMethodName = deserializeMethodName;
			this.guid = Guid.Parse (guid);
			deserializeMethod = type.GetMethod (deserializeMethodName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
			attribs.AddOrUpdate (this.guid, (g) => {
				return this;
			}, (g,o) => {
				return this;
			}
			);
		}

		RawProxyv1 Deserialize (byte[] buffer)
		{
			return (RawProxyv1)deserializeMethod.Invoke (null, new object[] { buffer });
		}

		public static BD2.RawProxy.RawProxyv1 DeserializeFromRawData (byte[] value)
		{
			System.IO.MemoryStream MS = new System.IO.MemoryStream (value);
			byte[] guidBytes = new byte[16];
			MS.Read (guidBytes, 0, 16);
			Guid guid = new Guid (guidBytes);
			byte[] payload = new byte[value.Length - 16];
			MS.Read (payload, 0, value.Length - 16);
			return attribs [guid].Deserialize (payload);
		}
	}
}