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
using BD2.Daemon;
using BD2.Daemon.Buses;

namespace BD2.Core.Network
{
	public sealed class InternSymmetricKeyRequestMessage : ObjectBusMessage
	{

		Guid id;

		public Guid ID {
			get {
				return id;
			}
		}

		readonly byte[] userID;

		public byte[] UserID {
			get {
				return userID;
			}
		}

		readonly byte[] keyID;

		public byte[] KeyID {
			get {
				return keyID;
			}
		}

		readonly byte[] encryptedKeyBytes;

		public byte[] EncryptedKeyBytes {
			get {
				return encryptedKeyBytes;
			}
		}

		readonly byte[][] userList;

		public byte[][] UserList {
			get {
				return userList;
			}
		}

		public InternSymmetricKeyRequestMessage (Guid id, byte[] userID, byte[] keyID, byte[] encryptedKeyBytes, byte[][] userList)
		{
			if (userID == null)
				throw new ArgumentNullException ("userID");
			if (keyID == null)
				throw new ArgumentNullException ("keyID");
			if (encryptedKeyBytes == null)
				throw new ArgumentNullException ("encryptedKeyBytes");
			if (userList == null)
				throw new ArgumentNullException ("userList");
			this.id = id;
			this.userID = userID;
			this.keyID = keyID;
			this.encryptedKeyBytes = encryptedKeyBytes;
			this.userList = userList;
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

