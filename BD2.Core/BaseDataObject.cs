using System;
using System.Collections.Generic;
using BD2.Common;

namespace BD2.Core
{
	public sealed class BaseDataObject : Serializable, IComparable<BaseDataObject>
	{
		Frontend frontend;
		Dictionary<FrontendInstanceBase, BaseDataObjectStateTracker> states;
		byte[] chunkDescriptor;

		public Frontend Frontend {
			get {
				return frontend;
			}
		}

		public BaseDataObjectStateTracker GetTrackerFor (FrontendInstanceBase fib)
		{
			lock (states) {
				BaseDataObjectStateTracker st;
				if (states.TryGetValue (fib, out st)) {
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
		//for de/serialization purposes
		public Guid ObjectType { get; }

		public Guid ObjectID { get; }
		#region IComparable implementation
		public int CompareTo (BaseDataObject other)
		{
			if (other == null)
				throw new ArgumentNullException ("other");
			int R = other.ObjectID.CompareTo (ObjectID);
			if (R == 0) {
				R = other.frontend.CompareTo (frontend);
			}
			return R;
		}
		#endregion
	}
}
