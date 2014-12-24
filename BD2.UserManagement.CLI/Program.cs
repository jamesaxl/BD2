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
using System.Collections.Generic;
using BD2.Core;
using BD2.Core.KEx;
using System.Linq;
using System.ComponentModel;

namespace BD2.UserManagement.CLI
{
	static class MainClass
	{
		static object lock_query = new object ();
		static Queue<string> initial = new Queue<string> ();

		public static string Query (string message)
		{
			//yes, it's just this simple for now.
			lock (lock_query) {
				Console.Write (message + " ");
				if (initial.Count != 0) {
					Console.WriteLine (initial.Peek ());
					return initial.Dequeue ();
				}
				return Console.ReadLine ();
			}
		}

		static SortedSet<string> Modifiers = new SortedSet<string> ();
		static UserRepository UR;

		public static int ExtractModifiers (string[] parts)
		{
			for (int n = 0; n != parts.Length; n++) {
				if (!Modifiers.Contains (parts [n]))
					return n;
			}
			return -1;
		}



		public static void Main (string[] args)
		{
			if (args.Length == 1)
				foreach (string str in System.IO.File.ReadAllLines (args[0]))
					initial.Enqueue (str);
			SortedDictionary<int, System.Threading.Thread> jobs = new SortedDictionary<int, System.Threading.Thread> ();
			Modifiers.Add ("async");
			//BD2.Core.Database DB = new BD2.Core.Database ();
			string command;
			do {
				command = Query ("Command>");
				if (command == null)
					return;
				OffsetedArray<string> commandparts = command.Split (' ');
				string[] CommandParts = (string[])((string[])commandparts).Clone ();
				commandparts.Offset = ExtractModifiers (CommandParts);
				SortedSet<string> CommandModifiers = new SortedSet<string> (commandparts.GetStrippedPart ());
				switch (CommandParts [0]) {
				case "Open":
					{
						UR = new UserRepository (new DatabasePath (Query ("path")), new UserRepositoryConfiguration ());
					}
					break;
				case "CreateAdmin":
					{
						string name = Query ("name");
						string password = Query ("Password");
						string pepper = Query ("Pepper");
						UR.CreateUser (name, password, pepper, null);
					}
					break;
				case "Create":
					{
						string name = Query ("name");
						string password = Query ("Password");
						string pepper = (Query ("Pepper"));
						byte[] parentID;
						parentID = HexStringToByteArray (Query ("Parent").Replace (" ", "").Replace (":", ""));
						UR.CreateUser (name, password, pepper, parentID);
					}
					break;
				case "List":
					foreach (var U in UR.GetUsers ()) {
						Console.WriteLine ("{0}:{1}", U.Key.ToHexadecimal (), U.Value);
					}
					break;
				case "Exit":
					return;
				default:
					Console.Error.WriteLine (string.Format ("{0} is not a valid command.", CommandParts [0]));
					break;
				}
			} while(true);
		}

		static byte[] HexStringToByteArray (string hex)
		{
			return Enumerable.Range (0, hex.Length)
				.Where (x => x % 2 == 0)
				.Select (x => Convert.ToByte (hex.Substring (x,
				2), 16)).ToArray ();
		}
	}
}