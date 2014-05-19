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

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("744d4d93-ea3a-487d-a4d6-9ea1d241e7a7")]
	[ObjectBusMessageDeserializerAttribute(typeof(BusReadyMessage), "Deserialize")]
	public class BusReadyMessage : ObjectBusMessage
	{
		public static ObjectBusMessage Deserialize (byte[] buffer)
		{
			return new BusReadyMessage (new Guid (buffer));
		}

		Guid objectBusSessionID;

		public Guid ObjectBusSessionID {
			get {
				return objectBusSessionID;
			}
		}

		public BusReadyMessage (Guid objectBusSessionID)
		{
			this.objectBusSessionID = objectBusSessionID;
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			return objectBusSessionID.ToByteArray ();
		}

		public override Guid TypeID {
			get {
				return Guid.Parse ("744d4d93-ea3a-487d-a4d6-9ea1d241e7a7");
			}
		}
		#endregion
	}
}

