//
//  SqlConnectionConfigurationA.cs
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