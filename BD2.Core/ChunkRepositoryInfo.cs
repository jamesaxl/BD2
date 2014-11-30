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
using BD2.Core;

namespace BD2.Core
{
	[Serializable]
	public sealed class ChunkRepositoryInfo
	{
		public string BasePath;
		public string DefaultStorageType;
		public SortedDictionary<string, string> StoragePaths;
		public SortedDictionary<string, string> StorageTypes;

		public KeyValueStorage<T> GetStorage<T> (DatabasePath basePath, string storageName) where T : class
		{
			string type = DefaultStorageType;
			if (StorageTypes.ContainsKey (storageName))
				type = StorageTypes [storageName];
			if (!StoragePaths.ContainsKey (storageName)) {
				if (StorageTypes.ContainsKey (storageName))
					throw new KeyNotFoundException (string.Format ("No path defined for storage [{0}], It has a type defined.(Misconfiguration?)", storageName));
				throw new KeyNotFoundException (string.Format ("No path defined for storage [{0}]", storageName));
			}
			string path = StoragePaths [storageName];
			switch (type) {
			case "LevelDB":
				return new LevelDBKeyValueStorage<T> (basePath.CreatePath (path));
			default:
				throw new NotImplementedException ();
			}
		}
	}

}
