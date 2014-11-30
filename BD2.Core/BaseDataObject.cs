using System;
using System.Collections.Generic;
using BD2.Core;

namespace BD2.Core
{
	public sealed class BaseDataObject : IComparable<BaseDataObject>
	{
		bool fullyLoaded;
		readonly FrontendInstanceBase frontendInstanceBase;
		readonly SortedDictionary<byte[], BaseDataObjectVersion> versions = new SortedDictionary<byte[], BaseDataObjectVersion> ();

		public bool FullyLoaded {
			get {
				return fullyLoaded;
			}
			internal set {
				fullyLoaded = value; 
			}
		}

		public SortedDictionary<byte[], BaseDataObjectVersion> Versions {
			get {
				return versions;
			}
		}

		public void InternVersion (BaseDataObjectVersion version)
		{
			versions.Add (version.ChunkID, version);
		}

		public BaseDataObject (FrontendInstanceBase frontendInstanceBase, byte[] objectID)
		{
			if (objectID == null)
				throw new ArgumentNullException ("objectID");
			if (frontendInstanceBase == null)
				throw new ArgumentNullException ("frontendInstanceBase");
			this.frontendInstanceBase = frontendInstanceBase;
			this.objectID = objectID;
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

		//for de/serialization purposes

		byte[] objectID;

		public byte[] ObjectID {
			get {
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
