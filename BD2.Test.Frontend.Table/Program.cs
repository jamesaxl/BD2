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

namespace BD2.Test.Frontend.Table
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			string databaseName = "Test";
			BD2.Core.Database db = new BD2.Core.Database (new BD2.Chunk.ChunkRepository[] { new BD2.Repo.Leveldb.Repository ("/home/behrooz/test") }, new BD2.Core.Frontend[] { new BD2.Frontend.Table.Frontend (new BD2.Frontend.Table.GenericValueDeserializer ()) }, databaseName);
			BD2.Frontend.Table.FrontendInstance frontendInstance = (BD2.Frontend.Table.FrontendInstance)(db.GetFrontend ("BD2.Frontend.Table")).CreateInstanse (db.GetSnapshot ("Primary"));
			frontendInstance.CreateRow (frontendInstance.GetTable ("Table2"), frontendInstance.GetColumnSet (new BD2.Frontend.Table.Model.Column[] { frontendInstance.GetColumn ("Name", typeof(String),false,0) }), new object[] { "This is yet another test." });
			foreach (var R in frontendInstance.GetTable ("Table2").GetRows (frontendInstance.GetColumnSet (new BD2.Frontend.Table.Model.Column[] { frontendInstance.GetColumn ("Name", typeof(String),false,0) }))) {
				foreach (var V in R.GetValues ()) {
					Console.WriteLine (V);
				}
			}
			frontendInstance.Flush ();
		}
	}
}
