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
using BD2.Daemon.Buses;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute ("e7002100-c14c-4170-9e0f-db54aea9a847")]
	[ObjectBusMessageDeserializerAttribute (typeof(ServiceResponseMessage), "Deserialize")]
	public class ServiceResponseMessage : ObjectBusMessage
	{
	
		Guid id;

		public Guid ID {
			get {
				return id;
			}
		}

		Guid requestID;

		public Guid RequestID {
			get {
				return requestID;
			}
		}

		ServiceResponseStatus status;

		public ServiceResponseStatus Status {
			get {
				return status;
			}
		}

		public static ObjectBusMessage Deserialize (byte[] bytes)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (bytes, false)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					return new ServiceResponseMessage (new Guid (BR.ReadBytes (16)), new Guid (BR.ReadBytes (16)), (ServiceResponseStatus)BR.ReadInt32 ());
				}
			}
		}

		public ServiceResponseMessage (Guid id, Guid requestID, ServiceResponseStatus status)
		{
			if (!Enum.IsDefined (typeof(ServiceResponseStatus), status)) {
				throw new ArgumentException ("Status is not valid", "status");
			}
			this.id = id;
			this.requestID = requestID;
			this.status = status;
		}

		#region implemented abstract members of ObjectBusMessage

		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (id.ToByteArray ());
					BW.Write (requestID.ToByteArray ());
					BW.Write ((int)status);
					return MS.ToArray ();
				}
			}
		}

		public override Guid TypeID {
			get {
				return Guid.Parse ("e7002100-c14c-4170-9e0f-db54aea9a847");
			}
		}

		#endregion
	}
}

