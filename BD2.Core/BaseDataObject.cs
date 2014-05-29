using System;
using System.Collections.Generic;
using BD2.Common;

namespace BD2.Core
{
	public abstract class BaseDataObject : Serializable, IComparable<BaseDataObject>
	{
		public virtual IEnumerable<BaseDataObject> GetDependenies ()
		{
			yield break;
		}

		public bool IsVolatile {
			get {
				return chunkID == null;
			}
		}

		byte[] chunkID;
		FrontendInstanceBase frontendInstanceBase;
		byte[] objectID;

		protected BaseDataObject (FrontendInstanceBase frontendInstanceBase, byte[] chunkID)
		{
			if (frontendInstanceBase == null)
				throw new ArgumentNullException ("frontendInstanceBase");
			this.frontendInstanceBase = frontendInstanceBase;
			this.chunkID = chunkID;
		}

		internal void SetChunkID (byte[] newChunkID)
		{
			if (newChunkID == null)
				throw new ArgumentNullException ("newChunkID");
			if (chunkID != null)
				throw new InvalidOperationException ();
			chunkID = newChunkID;
		}

		public FrontendInstanceBase FrontendInstanceBase {
			get {
				return frontendInstanceBase;
			}
		}

		public Frontend Frontend {
			get {
				return frontendInstanceBase.Frontend;
			}
		}

		public class ComparerID : IComparer<BaseDataObject>
		{
			int IComparer<BaseDataObject>.Compare (BaseDataObject x, BaseDataObject y)
			{
				return  ByteSequenceComparer.Shared.Compare (x.ObjectID, y.ObjectID);
			}
		}

		internal byte[] ChunkID {
			get {
				return (byte[])chunkID.Clone ();
			}
		}
		//for de/serialization purposes
		public abstract Guid ObjectType { get; }

		byte[] puoid;

		public byte[] GetPersistentUniqueObjectID ()
		{
			if (puoid != null)
				return puoid;
			System.IO.MemoryStream MS = new System.IO.MemoryStream ();
			Serialize (MS);
			puoid = MS.ToArray ().SHA256 ();
			return puoid;
		}

		public byte[] ObjectID {
			get {
				if (objectID == null)
					objectID = GetPersistentUniqueObjectID ();
				return objectID;
			}
		}
		#region IComparable implementation
		public int CompareTo (BaseDataObject other)
		{
			if (other == null)
				throw new ArgumentNullException ("other");
			int R = ByteSequenceComparer.Shared.Compare (ObjectID, other.ObjectID);
			if (R == 0) {
				R = other.Frontend.CompareTo (Frontend);
			}
			return R;
		}
		#endregion
	}
}
