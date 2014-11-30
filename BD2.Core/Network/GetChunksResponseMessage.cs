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
using BD2.Daemon;
using BD2.Daemon.Buses;

namespace BD2.Core.Network
{
	public class GetChunksResponseMessage : ObjectBusMessage
	{
		Guid requestID;

		public Guid RequestID {
			get {
				return requestID;
			}
		}

		readonly Dictionary<byte[], Tuple<byte[], Dictionary<byte[], byte[]>>> parts;

		public Dictionary<byte[], Tuple<byte[], Dictionary<byte[], byte[]>>> Parts {
			get {
				return parts;
			}
		}

		readonly Exception exception;

		public Exception Exception {
			get {
				return exception;
			}
		}

		public GetChunksResponseMessage (Guid requestID, Dictionary<byte[], Tuple<byte[], Dictionary<byte[], byte[]>>> parts, Exception exception)
		{
			if (parts == null)
				throw new ArgumentNullException ("parts");
			foreach (var cd in parts) {
				if (cd.Key == null)
					throw new ArgumentException ("parts has a null key");
				if (cd.Value == null)
					throw new ArgumentNullException (string.Format ("parts[{0}]", cd.Key));
				var t = cd.Value;
				if (t.Item1 == null)
					throw new ArgumentNullException (string.Format ("parts[{0}].Data", cd.Key));
				if (t.Item2 == null)
					throw new ArgumentNullException (string.Format ("parts[{0}].Parts", cd.Key));
				foreach (var p in t.Item2) {
					if (p.Key == null)
						throw new ArgumentException (string.Format ("parts[{0}].Parts has a null key", cd.Key));
					if (cd.Value == null)
						throw new ArgumentNullException (string.Format ("parts[{0}].Parts[{1}]", cd.Key, p.Key));
				}
			}
			this.requestID = requestID;
			this.parts = parts;
			this.exception = exception;

		}

		#region implemented abstract members of ObjectBusMessage

		public override byte[] GetMessageBody ()
		{
			throw new NotImplementedException ();
		}

		public override Guid TypeID {
			get {
				throw new NotImplementedException ();
			}
		}

		#endregion
	}
}

