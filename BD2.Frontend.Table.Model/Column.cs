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
using System.Collections.Generic;
using BD2.Core;

namespace BD2.Frontend.Table.Model
{
	public abstract class Column : BaseDataObject
	{
		string name;

		public string Name { get { return name; } }

		Type type;

		public Type Type { get { return type; } }

		long length;

		public long Length { get { return length; } }

		bool allowNull;

		public bool AllowNull { get { return allowNull; } }

		protected Column (FrontendInstanceBase frontendInstanceBase, byte[] chunkID, string name, Type type, bool allowNull, long length)
			:base (frontendInstanceBase, chunkID)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			this.name = name;
			this.type = type;
			this.allowNull = allowNull;
			this.length = length;
		}

		public override void Serialize (System.IO.Stream stream)
		{
			using (System.IO.BinaryWriter BW  = new System.IO.BinaryWriter (stream)) {
				BW.Write (name);
				BW.Write (((Frontend)FrontendInstanceBase.Frontend).ValueDeserializer.TypeToID (type));
				BW.Write (allowNull);
				BW.Write (length);
			}
		}
	}
}
