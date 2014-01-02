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
 * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BD2.RawProxy
{
	public sealed class ProxyCombination
	{
		Tuple<RawProxyv1, byte[]>[] proxies;

		/// <summary>
		/// Initializes a new instance of the <see cref="BD2.RawProxy.ProxyCombination"/> class.
		/// </summary>
		/// <param name="Proxies">Proxies and Extended arguments of each if there is any.</param>
		public ProxyCombination (Tuple<RawProxyv1, byte[]>[] Proxies)
		{
			if (Proxies == null)
				throw new ArgumentNullException ("Proxies");
			this.proxies = new Tuple<RawProxyv1, byte[]>[Proxies.Length];
			for (int n = 0; n != Proxies.Length; n++) {
				proxies [n] = new Tuple<RawProxyv1, byte[]> (Proxies [n].Item1, (byte[])Proxies [n].Item2.Clone ());
			}
		}

		public byte[] Decode (byte[] Input)
		{
			if (Input == null)
				throw new ArgumentNullException ("Input");
			foreach (var T in proxies) {
				Input = T.Item1.Decode (Input);
			}
			return Input;
		}

		public byte[] Encode (byte[] Input)
		{
			if (Input == null)
				throw new ArgumentNullException ("Input");
			foreach (var T in proxies) {
				if (T.Item2 != null)
					Input = T.Item1.Encode (Input, T.Item2);
				else
					Input = T.Item1.Encode (Input);
			}
			return Input;
		}
	}
}
