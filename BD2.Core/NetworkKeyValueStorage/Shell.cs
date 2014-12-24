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
using System.Threading.Tasks;
using BD2.Daemon;
using Microsoft.Win32;
using BD2.Core.KEx;
using BD2.Daemon.Buses;

namespace BD2.Core.NetworkKeyValueStorage
{
	public sealed class Shell<T> : KeyValueStorage<T>  where T : class
	{
		readonly ObjectBusSession session;

		public ObjectBusSession Session {
			get {
				return session;
			}
		}

		public Shell (ObjectBusSession session)
		{
			if (session == null)
				throw new ArgumentNullException ("session");
			this.session = session;

		}

		void GetValuesResponseMessageReceived (ObjectBusMessage obj)
		{
			throw new NotImplementedException ();
		}

		#region implemented abstract members of KeyValueStorage

		public override System.Collections.Generic.IEnumerator<byte[]> EnumerateKeys ()
		{
			throw new NotImplementedException ();
		}

		public override void Initialize ()
		{
			session.RegisterType (typeof(GetValuesResponseMessage), GetValuesResponseMessageReceived);
		}

		public override void Dispose ()
		{
			throw new NotImplementedException ();
		}

		public override System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<byte[], T>> GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		public override void Put (byte[] key, T value)
		{
			throw new NotImplementedException ();
		}

		public override T Get (byte[] key)
		{
			throw new NotImplementedException ();
		}

		public override void Delete (byte[] key)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region implemented abstract members of KeyValueStorage

		public override System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<byte[], T>> EnumerateFrom (byte[] start)
		{
			throw new NotImplementedException ();
		}

		public override System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<byte[], T>> EnumerateRange (byte[] start, byte[] end)
		{
			throw new NotImplementedException ();
		}

		public override int Count {
			get {
				throw new NotImplementedException ();
			}
		}

		#endregion
	}
}

