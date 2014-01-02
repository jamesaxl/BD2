//
//  SnapshotDescriptor.cs
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

namespace BD2.Common
{
	public class SnapshotDescriptor : Serializable
	{

		Guid id;

		public Guid ID {
			get {
				return id;
			}
		}

		Guid[] sourceSnapshots;
		Guid[] include;
		Guid[] exclude;

		public SnapshotDescriptor (Guid id, List<Guid> sourceSnapshots,
		                           List<Guid> include, List<Guid> exclude)
		{
			this.id = id;
			if (sourceSnapshots == null)
				throw new ArgumentNullException ("SourceSnapshots");
			if (include == null)
				throw new ArgumentNullException ("Include");
			if (exclude == null)
				throw new ArgumentNullException ("Exclude");
			this.sourceSnapshots = sourceSnapshots.ToArray ();
			this.include = include.ToArray ();
			this.exclude = exclude.ToArray ();
		}

		public override ObjectSerializationContext Serialize ()
		{
			return new SnapshotSerializationContext (this);
		}

		class SnapshotSerializationContext : ObjectSerializationContext
		{
			SnapshotDescriptor snapshotDescriptor;
			#region implemented abstract members of BD2.Common.ObjectSerializationContext
			public override Serializable GetObject ()
			{
				return snapshotDescriptor;
			}

			public override byte[] GetAttributes (Guid Type)
			{
				return null;
			}

			public override bool CanApplyProxy (Guid Type)
			{
				return false;
			}

			public override byte[] GetBytes ()
			{
				byte[] Bytes = new byte[28 + ((snapshotDescriptor.sourceSnapshots.Length + snapshotDescriptor.include.Length + snapshotDescriptor.exclude.Length) << 4)];
				System.IO.BinaryWriter BW = new System.IO.BinaryWriter (new System.IO.MemoryStream (Bytes, true));
				BW.Write (snapshotDescriptor.id);
				BW.Write (snapshotDescriptor.sourceSnapshots);
				BW.Write (snapshotDescriptor.include);
				BW.Write (snapshotDescriptor.exclude);
				return Bytes;
			}
			#endregion
			internal SnapshotSerializationContext (SnapshotDescriptor SnapshotDescriptor)
			{
				snapshotDescriptor = SnapshotDescriptor;
			}
		}
	}
}

