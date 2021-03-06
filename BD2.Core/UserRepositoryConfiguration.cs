// /*
//  * Copyright (c) 2014 Behrooz Amoozad
//  * All rights reserved.
//  *
//  * Redistribution and use in source and binary forms, with or without
//  * modification, are permitted provided that the following conditions are met:
//  *     * Redistributions of source code must retain the above copyright
//  *       notice, this list of conditions and the following disclaimer.
//  *     * Redistributions in binary form must reproduce the above copyright
//  *       notice, this list of conditions and the following disclaimer in the
//  *       documentation and/or other materials provided with the distribution.
//  *     * Neither the name of the bd2 nor the
//  *       names of its contributors may be used to endorse or promote products
//  *       derived from this software without specific prior written permission.
//  *
//  * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//  * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//  * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//  * DISCLAIMED. IN NO EVENT SHALL Behrooz Amoozad BE LIABLE FOR ANY
//  * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//  * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//  * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//  * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//  * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//  * */

using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;
using BD2.Core;
using System.Net.Security;

namespace BD2.Core
{
	[Serializable]
	public sealed class UserRepositoryConfiguration
	{
		public KeyValueStorageConfiguration Users = new KeyValueStorageConfiguration ("Users", "BPlus");
		public KeyValueStorageConfiguration UserKeys = new KeyValueStorageConfiguration ("UserKeys", "BPlus");
		public KeyValueStorageConfiguration UserCerts = new KeyValueStorageConfiguration ("UserCerts", "BPlus");
		public KeyValueStorageConfiguration UserSigningKeys = new KeyValueStorageConfiguration ("UserSigningKeys", "BPlus");
		public KeyValueStorageConfiguration UserSigningCerts = new KeyValueStorageConfiguration ("UserSigningCerts", "BPlus");
		public KeyValueStorageConfiguration UserParents = new KeyValueStorageConfiguration ("UserParents", "BPlus");
		public KeyValueStorageConfiguration UserRepositores = new KeyValueStorageConfiguration ("UserRepositories", "BPlus");
		public KeyValueStorageConfiguration Repositores = new KeyValueStorageConfiguration ("Repositories", "BPlus");
		public KeyValueStorageConfiguration Meta = new KeyValueStorageConfiguration ("Meta", "BPlus");
		public KeyValueStorageConfiguration Permissions = new KeyValueStorageConfiguration ("Permissions", "BPlus");
	}
	
}
