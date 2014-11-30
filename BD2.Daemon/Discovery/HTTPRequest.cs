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
using System.Text;

namespace BD2.Daemon.Discovery
{
	public sealed class HTTPRequest
	{
		string method = "NOTIFY";
		string uri = "*";
		string version = "HTTP/1.1";

		public string Method {
			get {
				return method;
			}
			set {
				method = value;
			}
		}

		public string URI {
			get {
				return uri;
			}
			set {
				uri = value;
			}
		}

		public string Version {
			get {
				return version;
			}
			set {
				version = value;
			}

		}

		Dictionary<string, string> arguments;

		public Dictionary<string, string> Arguments {
			get {
				return arguments;
			}
			set { 
				arguments = value;
			}

		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (string.Format ("{0} {1} {2}\r\n", method, uri, version));
			foreach (var kv in arguments) {
				sb.Append (string.Format ("{0}: {1}\r\n", kv.Key, kv.Value));
			}
			sb.Append ("\r\n");
			return sb.ToString ();
		}

		public HTTPRequest ()
		{
		}

		public HTTPRequest (System.IO.Stream stream)
		{
			System.IO.StreamReader SR = new System.IO.StreamReader (stream);
			arguments = new Dictionary<string, string> ();
			string request = SR.ReadLine ();
			string[] requestParts = request.Split (' ');
			method = requestParts [0];
			uri = requestParts [1];
			version = requestParts [2];
			string argument;
			while ((argument = SR.ReadLine ()) != "") {
				try {
					int colonOffset = argument.IndexOf (':');
					string argumentName = argument.Substring (0, colonOffset);
					string value = argument.Substring (colonOffset + 2);
					arguments.Add (argumentName, value);
				} catch (Exception ex) {
					Console.WriteLine (ex.Message);
					throw ex;
				}
			}
		}

	}
}
