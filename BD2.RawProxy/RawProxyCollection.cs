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

namespace BD2.RawProxy
{
	public sealed class RawProxyCollection : System.Collections.Generic.ICollection<RawProxyv1>
	{
		SortedDictionary<byte[], RawProxyv1> rpd;
		List<RawProxyv1> rps;
		//TODO: provide facilites for proxies which know ids for other proxies to access them, for example the id for the compressv1 is always known and can be accessed by all but no one can't guess the id for a cryptov1 serialized with a specific key
		//TODO(if time): provide facilities in chunkRepository to let RawProxyCollection to know exactly which proxies have access to others
		public RawProxyCollection ()
		{
			rps = new List<RawProxyv1> ();
			rpd = new SortedDictionary<byte[], RawProxyv1> ();
		}

		#region ICollection implementation

		public void Add (RawProxyv1 item)
		{
			rps.Add (item);
			rpd.Add (item.ObjectID, item);
		}

		public void Clear ()
		{
			rps.Clear ();
		}

		public bool Contains (RawProxyv1 item)
		{
			return rps.Contains (item);
		}

		public void CopyTo (RawProxyv1[] array, int arrayIndex)
		{
			throw new NotSupportedException ();
		}

		public bool Remove (RawProxyv1 item)
		{
			return rps.Remove (item);
		}

		public int Count {
			get {
				return rps.Count;
			}
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		#endregion

		#region IEnumerable implementation

		public IEnumerator<RawProxyv1> GetEnumerator ()
		{
			return rps.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return rps.GetEnumerator ();
		}

		#endregion

		public byte[] ChainDecode (byte[] payload)
		{
			List<RawProxyv1> rrps = new List<RawProxyv1> (rps);
			rrps.Reverse ();
			return ChainDecode (payload, rrps);
		}

		public static byte[] ChainDecode (byte[] payload, IEnumerable<RawProxyv1> proxiesInOrder)
		{
			foreach (RawProxyv1 proxy in proxiesInOrder) {
				payload = proxy.Decode (payload);
			}
			return payload;
		}

		public byte[] ChainDecode (byte[] payload, IEnumerable<byte[]> proxiesInOrder)
		{
			foreach (byte[] proxy in proxiesInOrder) {
				payload = rpd [proxy].Decode (payload);
			}
			return payload;
		}

		public byte[] ChainEncode (byte[] payload)
		{
			return ChainEncode (payload, rps);
		}

		public static byte[] ChainEncode (byte[] payload, IEnumerable<RawProxyv1> proxiesInOrder)
		{
			foreach (RawProxyv1 proxy in proxiesInOrder) {
				payload = proxy.Encode (payload);
			}
			return payload;
		}

		public byte[] ChainEncode (byte[] payload, IEnumerable<byte[]> proxiesInOrder)
		{
			foreach (byte[] proxy in proxiesInOrder) {
				payload = rpd [proxy].Encode (payload);
			}
			return payload;
		}

		public byte[] Serialize ()
		{
			System.IO.MemoryStream MS = new System.IO.MemoryStream ();
			foreach (var rp in rps) {
				byte[] buf = rp.Serialize ();
				MS.Write (buf, 0, buf.Length);
			}
			return MS.ToArray ();
		}
	}
}

