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
 * DISCLAIMED. IN NO EVENT SHALL Behrooz Amoozad BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * */
using System;
using System.Collections.Generic;

namespace BD2.Core
{
	public abstract class FrontendInstanceBase
	{
		Snapshot snapshot;

		public Snapshot Snapshot {
			get {
				return snapshot;
			}
		}

		Frontend frontend;

		public Frontend Frontend {
			get {
				return frontend;
			}
		}

		protected FrontendInstanceBase (Snapshot snapshot, Frontend frontend)
		{
			if (frontend == null)
				throw new ArgumentNullException ("frontend");
			this.snapshot = snapshot;
			this.frontend = frontend;
		}

		protected void Initialize ()
		{
			foreach (byte[] chunks in snapshot.GetChunks ()) {
				//something like this
				//TODO:HACK:FIX:XXX
				//CreateObject( snapshot.Database.Backends.GetRepositories ().GetEnumerator ().Current.PullData (chunks));
			}
		}

		public string Name { get { return snapshot.Name; } }

		protected abstract void CreateObject (byte[] bytes);

		protected abstract IEnumerable<BaseDataObject> GetVolatileObjects ();

		protected abstract IEnumerable<BaseDataObject> GetObjectWithID (byte[] objectID);

		internal void GetVolatileData (System.IO.BinaryWriter binaryWriter)
		{
			SortedSet<BaseDataObject> baseDataObjects = new SortedSet<BaseDataObject> ();
			foreach (BaseDataObject baseDataObject in baseDataObjects)
				GetVolatileData (binaryWriter, baseDataObjects, baseDataObject);
		}

		protected abstract void PurgeObject (BaseDataObject baseDataObject);

		public void PurgeVolatileData ()
		{
			foreach (BaseDataObject baseDataObject in GetVolatileObjects()) {
				PurgeObject (baseDataObject);
			}
		}

		internal void GetVolatileData (System.IO.BinaryWriter binaryWriter, SortedSet<BaseDataObject> objects, BaseDataObject baseDataObject)
		{
			binaryWriter.Write (SerializeSingleObject (baseDataObject));
			SortedSet<BaseDataObject> finishedList = new SortedSet<BaseDataObject> ();
			foreach (BaseDataObject dependency in baseDataObject.GetDependenies ()) {
				if (dependency.IsVolatile) {
					if (!finishedList.Contains (dependency)) {
						finishedList.Add (dependency);
						GetVolatileData (binaryWriter, objects, dependency);
					}
				}
			}
		}

		protected abstract byte[] SerializeSingleObject (BaseDataObject baseDataObject);
	}
}
