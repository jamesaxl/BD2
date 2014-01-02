//
//  SQLDatabase.cs
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

namespace BD2.Conv.MSSQL
{
	public class SQLDatabase
	{
		readonly SqlConnectionConfiguration configuration;

		internal SQLDatabase (SqlConnectionConfiguration configuration)
		{
			if (configuration == null)
				throw new ArgumentNullException ("configuration");
			this.configuration = configuration;
		}

		public IDbCommand CreateCommand ()
		{
			return new SqlCommand ();
		}

		public IDbDataParameter CreateParameter (string name, DbType type, int length, string columnName)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (columnName == null)
				throw new ArgumentNullException ("columnName");
			SqlParameter Param = new SqlParameter ();
			Param.ParameterName = name;
			Param.SourceColumn = columnName;
			Param.DbType = type;
			Param.Size = length;
			return Param;
		}

		public IDbDataParameter CreateParameter (string name, object type, int length, string columnName)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (type == null)
				throw new ArgumentNullException ("type");
			if (columnName == null)
				throw new ArgumentNullException ("columnName");
			SqlParameter Param = new SqlParameter ();
			Param.ParameterName = name;
			Param.SourceColumn = columnName;
			Param.SqlDbType = (SqlDbType)type;
			Param.Size = length;
			return Param;
		}

		public IDbConnection GetNewConnection ()
		{
			SqlConnection Connection = (SqlConnection)configuration.GetConnection ();
			while (Connection.State != ConnectionState.Open)
				try {
					Connection.Open ();
				} catch {
					Connection.Dispose ();
					System.Threading.Thread.Sleep (100);
					Connection = (SqlConnection)configuration.GetConnection ();
				}
			return Connection;
		}

		protected void HandleException (string text, Exception ex)
		{
			if (text == null)
				throw new ArgumentNullException ("text");
			if (ex == null)
				throw new ArgumentNullException ("ex");
			var sqlException = ex as SqlException;
			if (sqlException != null) {
				ConsoleColor CC = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine (text);
				System.Collections.IEnumerator errors = sqlException.Errors.GetEnumerator ();
				while (errors.MoveNext ())
					Console.WriteLine (errors.Current);
				Console.ForegroundColor = CC;
			} else
				throw new InvalidOperationException (string.Format ("Invalid Exception Type:{0}", ex.GetType ().FullName), ex);
		}
	}
}