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
		#region implemented abstract members of ValueSerializerBase
		public override byte TypeToID (Type type)
		{
			string TFQN = type.FullName;
			switch (TFQN) {
			case "System.Boolean":
				return 1;
			case "System.Byte":
				return 2;
			case "System.SByte":
				return 128;
			case "System.Int16":
				return 3;
			case "System.UInt16":
				return 129;
			case "System.Int32":
				return 4;
			case "System.UInt32":
				return 130;
			case "System.Int64":
				return 5;
			case "System.UInt64":
				return 131;
			case "System.Double":
				return 9;
			case "System.String":
				return 16;
			case "System.Char":
				return 144;
			case "System.DateTime":
				return 24;
			case "System.Guid":
				return 32;
			case "System.Byte[]":
				return 36;
			default:
				throw new Exception (string.Format ("Serialization for type <{0}> is not supported", type.FullName));
			}
		}

		public override Type IDToType (byte id)
		{
			switch (id) {
			case 1:
				return typeof(Boolean);
			case 2:
				return typeof(Byte);
			case 128:
				return typeof(SByte);
			case 3:
				return typeof(Int16);
			case 129:
				return typeof(UInt16);
			case 4:
				return typeof(Int32);
			case 130:
				return typeof(UInt32);
			case 5:
				return typeof(Int64);
			case 131:
				return typeof(UInt64);
			case 9:
				return typeof(Double);
			case 16:
				return typeof(String);
			case 144:
				return typeof(Char);
			case 24:
				return typeof(DateTime);
			case 32:
				return typeof(Guid);
			case 36:
				return typeof(byte[]);
			default:
				throw new Exception (string.Format ("Serialization for type <{0}> is not supported", id));
			}
		}
		#endregion
		public override object Deserialize (System.IO.BinaryReader binaryReader)
		{
			bool hasValue = binaryReader.ReadBoolean ();
			if (!hasValue)
				return null;
			int typeID = binaryReader.ReadByte ();
			switch (typeID) {
			case 1:
				return binaryReader.ReadBoolean ();
			case 2:
				return binaryReader.ReadByte ();
			case 128:
				return binaryReader.ReadSByte ();
			case 3:
				return binaryReader.ReadInt16 ();
			case 129:
				return binaryReader.ReadUInt16 ();
			case 4:
				return binaryReader.ReadInt32 ();
			case 130:
				return binaryReader.ReadUInt32 ();
			case 5:
				return binaryReader.ReadInt64 ();
			case 131:
				return binaryReader.ReadUInt64 ();
			case 9:
				return binaryReader.ReadDouble ();
			case 16:
				return binaryReader.ReadString ();
			case 144:
				return binaryReader.ReadChar ();
			case 24:
				return new DateTime (binaryReader.ReadInt64 ());
			case 32:
				return new Guid (binaryReader.ReadBytes (16));
			case 36:
				return binaryReader.ReadBytes (binaryReader.ReadInt32 ());
			default:
				throw new Exception (string.Format ("Serialization for type <{0}> is not supported", typeID));
			}
		}

		public override void Serialize (object obj, System.IO.BinaryWriter binaryWriter)
		{
			if (obj == null) {
				binaryWriter.Write (false);
				return;
			}
			binaryWriter.Write (true);
			byte typeID = TypeToID (obj.GetType ());
			binaryWriter.Write (typeID);
			switch (typeID) {
			case 1:
				binaryWriter.Write ((bool)obj);
				break;
			case 2:
				binaryWriter.Write ((byte)obj);
				break;
			case 128:
				binaryWriter.Write ((sbyte)obj);
				break;
			case 3:
				binaryWriter.Write ((short)obj);
				break;
			case 129:
				binaryWriter.Write ((ushort)obj);
				break;
			case 4:
				binaryWriter.Write ((int)obj);
				break;
			case 130:
				binaryWriter.Write ((uint)obj);
				break;
			case 5:
				binaryWriter.Write ((long)obj);
				break;
			case 131:
				binaryWriter.Write ((ulong)obj);
				break;
			case 16:
				binaryWriter.Write ((string)obj);
				break;
			case 144:
				binaryWriter.Write ((char)obj);
				break;
			case 9:
				binaryWriter.Write ((Double)obj);
				break;
			case 24:
				binaryWriter.Write (((DateTime)obj).Ticks);
				break;
			case 32:
				binaryWriter.Write (((Guid)obj).ToByteArray ());
				break;
			case 36:
				binaryWriter.Write (((byte[])obj).Length);
				binaryWriter.Write ((byte[])obj);
				break;
			default:
				throw new Exception (string.Format ("Serialization for type <{0}> is not supported", obj.GetType ()));
			}
		}
	}
}