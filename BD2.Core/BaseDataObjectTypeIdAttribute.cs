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
using BD2.Core;

namespace BD2
{
	public sealed class BaseDataObjectTypeIdAttribute : Attribute
	{
		static System.Collections.Generic.SortedDictionary<Guid, BaseDataObjectTypeIdAttribute> attribs = new SortedDictionary<Guid, BaseDataObjectTypeIdAttribute> ();

		public static BaseDataObjectTypeIdAttribute GetAttribFor (Guid id)
		{
			return attribs [id];
		}

		Guid id;

		public Guid Id {
			get {
				return id;
			}
		}

		void ResolveMethod ()
		{
			deserialize = deserializerType.GetMethod (deserializerProcedureName);
		}

		System.Reflection.MethodInfo deserialize;

		public BaseDataObject Deserialize (FrontendInstanceBase fib, byte[] chunkID, byte[] buffer)
		{
			if (fib == null)
				throw new ArgumentNullException ("fib");
			if (chunkID == null)
				throw new ArgumentNullException ("chunkID");
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (deserialize == null) {
				ResolveMethod ();
			}
			return (BaseDataObject)deserialize.Invoke (null, new object[] { fib, chunkID, buffer });
		}

		Type deserializerType;

		public Type DeserializerType {
			get {
				return deserializerType;
			}
		}

		string deserializerProcedureName;

		public string DeserializerProcedureName {
			get {
				return deserializerProcedureName;
			}
		}

		public BaseDataObjectTypeIdAttribute (string id, Type deserializerType, string deserializerProcedureName)
		{
			if (deserializerType == null)
				throw new ArgumentNullException ("deserializerType");
			if (deserializerProcedureName == null)
				throw new ArgumentNullException ("deserializerProcedureName");
			this.id = Guid.Parse (id);
			this.deserializerType = deserializerType;
			this.deserializerProcedureName = deserializerProcedureName;
			attribs.Add (this.id, this);
			ResolveMethod ();
		}
	}
}
