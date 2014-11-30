/*
 * Copyright (c) 2013-2014 Behrooz Amoozad
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

namespace BD2.RawProxy.compress
{
	[RawProxyAttribute (typeof(compressv1), "873a0bc6-8631-43cd-8763-764810156050", "Deserialize")]
	public class compressv1 : RawProxyv1
	{
		static readonly Guid TypeGuid = new Guid ("873a0bc6-8631-43cd-8763-764810156050");

		public compressv1 (object configuration)
			: base (configuration)
		{
		}

		#region implemented abstract members of BD2.RawProxy.RawProxyv1

		public override string Name {
			get {
				return "Compress";
			}
		}

		public override Guid Type {
			get {
				return TypeGuid;
			}
		}

		public override byte[] Decode (byte[] Input)
		{
			switch (Input [0]) {
			case 0:
				System.IO.Compression.GZipStream gzs = new System.IO.Compression.GZipStream (new System.IO.MemoryStream (Input, 1, Input.Length - 1), System.IO.Compression.CompressionMode.Decompress);
				System.IO.MemoryStream rms/*i didn't mean it, it happend*/ = new System.IO.MemoryStream (Input.Length * 3);
				gzs.CopyTo (rms);
				return rms.ToArray ();
			default:
				throw new NotSupportedException ("Invalid/Unsupported compression algorithm.");
			}
		}

		#region implemented abstract members of RawProxyv1

		protected override byte[] DoSerialize ()
		{
			return new byte[1] { 1 };
		}

		public static RawProxyv1 Deserialize (byte[] buffer)
		{
			if ((buffer.Length == 1) && buffer [0] == 1) {
				return new compressv1 (null);
			}
			throw new NotSupportedException ("Requested version/configuration of compression proxy is not available.");
		}

		#endregion

		public override byte[] Encode (byte[] Input)
		{
			System.IO.Compression.GZipStream gzs = new System.IO.Compression.GZipStream (new System.IO.MemoryStream (Input), System.IO.Compression.CompressionMode.Compress);
			System.IO.MemoryStream RMS/*i didn't mean it, it happend*/ = new System.IO.MemoryStream (Input.Length >> 1);
			RMS.WriteByte (0);
			gzs.CopyTo (RMS);
			return RMS.ToArray ();
		}

		public override byte[] Encode (byte[] Input, byte[] Attributes)
		{
			if (Attributes == null)
				throw new ArgumentNullException ("Attributes");
			if (Attributes.Length != 1)
				throw new ArgumentException ("Invalid Attributes supplied.", "Attributes");
			System.IO.MemoryStream rms/*i didn't mean it*/ = new System.IO.MemoryStream (Input.Length >> 1);
			rms.WriteByte (Attributes [0]);
			switch (Attributes [0]) {
			case 0:
				System.IO.Compression.GZipStream gzs = new System.IO.Compression.GZipStream (new System.IO.MemoryStream (Input), System.IO.Compression.CompressionMode.Compress);
				gzs.CopyTo (rms);
				return rms.ToArray ();
			default:
				throw new Exception ("Unknown algorithm requested.");
			}
		}

		#endregion
	}
}

