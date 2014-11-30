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
using System.Text;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace BD2.Daemon.Discovery
{
	/// <summary>
	/// Very simple SSDP like discovery protocol
	/// </summary>
	public sealed class SimpleDiscovery
	{

		readonly Raw raw;
		readonly HTTPRequest req;

		public SimpleDiscovery (Raw raw)
		{
			this.raw = raw;
			if (raw == null)
				throw new ArgumentNullException ("raw");
			raw.SetReceiveCallback (MessageReceived);
			req = new HTTPRequest ();
			req.Method = "NOTIFY";
			req.URI = "*";
			req.Version = "HTTP/1.1";

		}

		public event EventHandler<DiscoverMessageReceivedEventArgs> DiscoverMessageReceived;

		void MessageReceived (Tuple<IPEndPoint, byte[]> obj)
		{
			byte[] buffer = obj.Item2;
			HTTPRequest recreq = new HTTPRequest (new MemoryStream (buffer));
			if (DiscoverMessageReceived != null)
				DiscoverMessageReceived.Invoke (this, new DiscoverMessageReceivedEventArgs (obj.Item1, recreq));
		}


		byte[] CreateMessage ()
		{
			return Encoding.ASCII.GetBytes (req.ToString ());
		}


		public void SetMessageInformation (Dictionary<string, string> arguments)
		{
			if (arguments == null)
				throw new ArgumentNullException ("arguments");
			if (req.Arguments == null) {
				req.Arguments = arguments;
				raw.SetTransmitCallback (CreateMessage);
			} else {
				req.Arguments = arguments;
			}
		}
	}
}
	