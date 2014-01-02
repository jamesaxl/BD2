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