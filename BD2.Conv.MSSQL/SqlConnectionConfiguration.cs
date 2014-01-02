/*
 * Copyright (c) 2013-2014 Behrooz Amoozad
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
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;

namespace BD2.Conv.MSSQL
{
	[Serializable]
	public class SqlConnectionConfiguration
	{
		public int Id;
		public string Server;
		public string UserName;
		public string Password;
		public string ConnectionProtocol;
		public string DatabaseName;
		public string ApplicationName;
		public bool EncryptConnection;
		public int PacketSize;
		public bool UseIntegratedSecurity;

		string GetConnectionString ()
		{
			return string.Format ("Data Source={0}{1};User Id={2};Database={3};Password={4}", Server, UseIntegratedSecurity ? ";Trusted_Connection=true" : "", UserName, DatabaseName, Password) + (ConnectionProtocol == null ? string.Format (";Network Library={0}", ConnectionProtocol) : "") + ";MultipleActiveResultSets=true";
		}

		public IDbConnection GetConnection ()
		{
			SqlConnection Conn;
			Conn = new SqlConnection (GetConnectionString ());
			return Conn;
		}

		public override string ToString ()
		{
			return string.Format ("[SqlConnectionConfiguration: ConnectionString={0}]", GetConnectionString ());
		}

		readonly SortedDictionary<string, SortedSet<STable>> tablesets = new SortedDictionary<string, SortedSet<STable>> ();

		public SortedSet<STable> GetTableStore (string name)
		{
			lock (tablesets) {
				SortedSet<STable> tableset;
				if (tablesets.TryGetValue (name, out tableset)) {
					return tableset;
				}
				tableset = new SortedSet<STable> ();
				tablesets.Add (name, tableset);
				return tableset;
			}
		}
	}
}