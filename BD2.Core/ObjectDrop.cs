//
//  ObjectDrop.cs
//
//  Author:
//       Behrooz Amoozad <behrooz0az@gmail.com>
//
//  Copyright (c) 2013 behrooz
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using BSO;
using BD2.Core;

namespace BD2
{
	public class ObjectDrop : BaseDataObject
	{
		Guid objectID;

		public override Guid ObjectID {
			get {
				return objectID;
			}
		}

		Guid underlyingObjectID;

		public Guid UnderlyingObjectID {
			get {
				return underlyingObjectID;
			}
		}

		public ObjectDrop (Guid ObjectID, Guid UnderlyingObjectID)
		{
			objectID = ObjectID;
			underlyingObjectID = UnderlyingObjectID;
		}

		private static ObjectDrop Deserialize (System.IO.BinaryReader Binaryreader)
		{

		}

		public override ObjectSerializationContext Serialize ()
		{
			return new ObjectDropSerializationContext (this);
		}

		class ObjectDropSerializationContext : ObjectSerializationContext
		{
			ObjectDrop obj;

			internal ObjectDropSerializationContext (ObjectDrop Obj)
			{
				obj = Obj;
			}

			#region implemented abstract members of ObjectSerializationContext

			public override byte[] GetAttributes (Guid Type)
			{
				return null;
			}

			public override bool CanApplyProxy (Guid Type)
			{
				return true;
			}

			public override Serializable GetObject ()
			{
				return obj;
			}

			public override byte[] GetBytes ()
			{
				byte[] Bytes = new byte[32];
				System.Buffer.BlockCopy (obj.objectID.ToByteArray (), 0, Bytes, 0, 16);
				System.Buffer.BlockCopy (obj.underlyingObjectID.ToByteArray (), 0, Bytes, 16, 16);
				return Bytes;
			}

			#endregion

		}
	}
}
