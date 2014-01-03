//
//  BD2TypeAttribute.cs
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
using System.IO;

namespace BD2.Core
{
	public class BD2TypeAttribute : Attribute
	{
		private static object lock_types = new object ();
		private static System.Collections.Generic.SortedDictionary<Guid, BD2TypeAttribute> types;
		public static  BD2TypeAttribute GetType (Guid ID)
		{
			lock(lock_types)
				return types[ID];
		}
		Guid id;
		public Guid ID { get { return id; } }
		Func<byte[], object> deserialize;
		Func<Stream, object> streamDeserialize;
		Func<object, byte[]> serialize;
		Func<object, Stream> streamSerialize;
		public BD2TypeAttribute (Guid ID, Func<byte[], object> Deserialize, Func<Stream, object> StreamDeserialize,
		                         Func<object, byte[]> Serialize, Func<object, Stream> StreamSerialize)
		{
			if(id.CompareTo(Guid.Empty) == 0)
				throw new ArgumentException ("ID");
			if ((Deserialize == null) && (StreamDeserialize == null))
				throw new ArgumentNullException ("Deserialize & StreamDeserialize");
			if ((Serialize == null) && (StreamSerialize == null))
				throw new ArgumentNullException ("Serialize & StreamSerialize");
			id = ID;
			deserialize = Deserialize;
			serialize = Serialize;
			streamDeserialize = StreamDeserialize;
			streamSerialize = StreamSerialize;
						lock(lock_types)
			types.Add(id, this);
		}
		public BD2TypeAttribute (Guid ID, Func<byte[], object> Deserialize,
		                         Func<object, byte[]> Serialize)
			:this(ID, Deserialize, null, Serialize, null)
		{ }
		public BD2TypeAttribute (Guid ID, Func<Stream, object> StreamDeserialize,
		                         Func<object, Stream> StreamSerialize)
			:this(ID, null, StreamDeserialize, null, StreamSerialize)
		{ }
	}
}

