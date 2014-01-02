using System;
using System.Collections.Generic;

namespace BD2.Common
{
	public abstract class BaseDataObject : Serializable, IComparable<BaseDataObject>
	{
		Frontend frontend;
		Dictionary<FrontendInstanceBase, BaseDataObjectStateTracker> state;
		private byte[] chunkDescriptor;

		public Frontend Frontend {
			get {
				return frontend;
			}
		}

		public BaseDataObjectStateTracker GetTrackerFor (FrontendInstanceBase fib)
		{
			lock (state) {
				BaseDataObjectStateTracker st;
				if (state.TryGetValue (fib, out st)) {
					return st;
				}
			}
			return null;
		}

		public class Comparer_ID : IComparer<BaseDataObject>
		{
			int IComparer<BaseDataObject>.Compare (BaseDataObject x, BaseDataObject y)
			{
			

				return y.ObjectID.CompareTo (x.ObjectID);
			}
		}

		internal byte[] ChunkDescriptor {
			get {
				return (byte[])chunkDescriptor.Clone ();
			}
		}
		//for de/serialization porpuses
		public abstract Guid ObjectType { get; }

		public abstract Guid ObjectID { get; }

		#region IComparable implementation

		public int CompareTo (BaseDataObject other)
		{
			if (other == null)
				throw new ArgumentNullException ("other");
			int R = other.ObjectID.CompareTo (this.ObjectID);
			if (R == 0) {
				R = other.frontend.CompareTo (this.frontend);
			}
			return R;
		}

		#endregion

	}
}
