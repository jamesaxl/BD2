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

namespace BD2.Conv.Daemon.MSSQL
{
	public class ServiceParameters
	{
		string connectionString;

		public string ConnectionString {
			get {
				return connectionString;
			}
		}

		public ServiceParameters (string connectionString)
		{
			if (connectionString == null)
				throw new ArgumentNullException ("connectionString");
			this.connectionString = connectionString;

		}

		public byte[] Serialize ()
		{
			using (System.IO.MemoryStream MS =  new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS, System.Text.Encoding.Unicode)) {
					BW.Write (connectionString);
				}
				return MS.ToArray ();
			}
		}

		public static ServiceParameters Deserialize (byte[] bytes)
		{
			string connectionString;
			using (System.IO.MemoryStream MS =  new System.IO.MemoryStream (bytes,false)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS,System.Text.Encoding.Unicode)) {
					connectionString = BR.ReadString ();
				}
			}
			return new ServiceParameters (connectionString);
		}
	}
}

