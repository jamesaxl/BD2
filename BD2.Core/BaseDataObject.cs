using System;
using System.Collections.Generic;
using BD2.Common;

namespace BD2.Core
{
	public abstract class BaseDataObject : Serializable, IComparable<BaseDataObject>
	{
		public abstract IEnumerable<BaseDataObject> GetDependenies ();

		public bool IsVolatile {
			get {
				return chunkID == null;
			}
		}

		byte[] chunkID;
		FrontendInstanceBase frontendInstanceBase;
		Guid objectID;

		protected BaseDataObject (FrontendInstanceBase frontendInstanceBase, Guid objectID, byte[] chunkID)
		{
			if (frontendInstanceBase == null)
				throw new ArgumentNullException ("frontendInstanceBase");
			this.frontendInstanceBase = frontendInstanceBase;
			this.objectID = objectID;
			this.chunkID = chunkID;
		}

		protected void SetChunkID (byte[] newChunkID)
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
				return y.ObjectID.CompareTo (x.ObjectID);
			}
		}

		internal byte[] ChunkID {
			get {
				return (byte[])chunkID.Clone ();
			}
		}
		//for de/serialization purposes
		public abstract Guid ObjectType { get; }

		public Guid ObjectID {
			get {
				return objectID;
			}
		}
		#region IComparable implementation
		public int CompareTo (BaseDataObject other)
		{
			if (other == null)
				throw new ArgumentNullException ("other");
			int R = other.ObjectID.CompareTo (ObjectID);
			if (R == 0) {
				R = other.Frontend.CompareTo (Frontend);
			}
			return R;
		}
		#endregion
	}
}
