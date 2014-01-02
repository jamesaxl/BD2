//
//  Frontend.cs
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
using BSO;
using System.Collections.Generic;

namespace BD2.Common
{
	public abstract class Frontend : IComparable
	{
		//For comparison 'only' just to make things smoother in case of needing to sort or search or whatever somewhere.
		Guid id = Guid.NewGuid ();
		SortedDictionary<Snapshot, Reference<FrontendInstanceBase>> instances = new SortedDictionary<Snapshot, Reference<FrontendInstanceBase>> ();
		FrontendCollection frontendCollection;
		LockManager lockManager = new LockManager ();

		public int CompareTo (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			Frontend f = obj as Frontend;
			if (obj == null)
				throw new ArgumentException ("Argument obj must be of type Frontend.", "obj");
			return f.id.CompareTo (id);
		}

		internal LockManager LockManager {
			get {
				return lockManager;
			}
		}

		public FrontendCollection FrontendCollection {
			get {
				return frontendCollection;
			}
		}

		public Frontend (Database database)
		{
			if (database == null)
				throw new ArgumentNullException ("database");
			this.frontendCollection = database.Frontends;
			database.Frontends.AddFrontend (this);
		}

		public abstract string Name { get; }

		public FrontendInstanceBase GetInstance (Snapshot Snapshot, bool Create)
		{
			Reference<FrontendInstanceBase> instance;
			lock (instances) {
				if (instances.TryGetValue (Snapshot, out instance))
					return instance.WaitForValidValue ();
				if (Create) {
					instance = new Reference<FrontendInstanceBase> (null, false);
					instances.Add (Snapshot, instance);
				} else {
					return null;
				}
			}																																																																																																																																			
			instance.Value = OnInstantiate (Snapshot);
			return instance;
		}

		protected abstract FrontendInstanceBase OnInstantiate (Snapshot Snapshot);

		public IEnumerable<Reference<FrontendInstanceBase>> GetInstances ()
		{
			//todo:make something with less security risk
			return (IEnumerable<Reference<FrontendInstanceBase>>)(instances.Values);
		}

		internal void CommitTransaction (ChunkData transaction)
		{
			if (transaction == null)
				throw new ArgumentNullException ("transaction");
			frontendCollection.CommitTransaction (transaction);
		}

		internal void HandleTransaction (ChunkData ChunkData)
		{
			if (ChunkData == null)
				throw new ArgumentNullException ("ChunkData");
			foreach (FrontendInstanceBase FIB in instances) {
				FIB.CommitTransaction (ChunkData);
			}
		}

		public abstract bool SupportsMultipleInstances { get; }

		protected abstract BaseDataObject ConvertToObject (Transaction transactio);
	}
}
