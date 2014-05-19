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
using BD2.Frontend.Table.Model;

namespace BD2.Frontend.Table
{
	public class GenericValueDeserializer : BD2.Frontend.Table.Model.ValueSerializerBase
	{

		public override object Deserialize (byte typeID, System.IO.Stream stream)
		{
			throw new NotImplementedException ();
		}

		public static byte GetTypeID (Type type)
		{
			string TFQN = type.FullName;
			switch (TFQN) {
			case "System.Byte":
			case "System.SByte":
				return 48;
			case "System.Boolean":
				break;
			case "System.Int16":
				break;
			case "System.UInt16":
				break;
			case "System.Int32":
				break;
			case "System.UInt32":
				break;
			case "System.Int64":
				break;
			case "System.UInt64":
				break;
			case "System.String":
				break;
			case "System.Char":
				break;
			case "System.DateTime":
				break;
			}
			return 0;
		}

		public override void Serialize (object obj, out byte typeID, out byte[] bytes)
		{
//			typeIDs.Add (typeof(bool), 104);
//			typeIDs.Add (typeof(char), 175);
//			typeIDs.Add (typeof(byte), 48);
//			typeIDs.Add (typeof(short), 52);
//			typeIDs.Add (typeof(int), 56);
//			typeIDs.Add (typeof(long), 127);
//			typeIDs.Add (typeof(float), 62);
//			typeIDs.Add (typeof(double), 106);
//			typeIDs.Add (typeof(Guid), 36);
//			typeIDs.Add (typeof(String), 231);

			typeID = GetTypeID (obj.GetType ());
			bytes = null;


		}
	}
}