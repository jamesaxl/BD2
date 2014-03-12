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
using System.Collections.Generic;
using System.IO;
using BD2.Core;

namespace BD2.Frontend.Table.Model
{
	public abstract class ColumnSet : BaseDataObject
	{
		public abstract ColumnSetOffsetHandler OffsetHandler{ get; }

		public abstract Table Table { get; }

		public abstract IEnumerable<Column> GetColumns ();

		public abstract object[] FromRaw (byte[] Raw);

		public abstract object[] FromRawStream (Stream Raw);

		public abstract byte[] ToRaw (object[] Objects);

		/// <summary>
		/// Writes Objects to Stream
		/// </summary>
		/// <returns>
		/// The number of bytes written to stream
		/// </returns>
		/// <param name='Objects'>
		/// Objects to serialize as column values
		/// </param>
		/// <param name='Stream'>
		/// stream to write data to
		/// </param>
		public abstract int ToRawStream (object[] Objects, Stream Stream);

		public abstract void Retrieve (Column Column);
	}
}