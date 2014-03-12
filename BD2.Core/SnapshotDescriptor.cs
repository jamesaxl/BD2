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
using BD2.Common;

namespace BD2.Core
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
			#region implemented abstract members of BD2.Core.ObjectSerializationContext
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

