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
using System.Collections.Generic;

namespace BD2.CLI
{
	class MainClass
	{
		static object lock_query = new object ();

		public static string Query (string Message)
		{
			//yes, it's just this simple for now.
			lock (lock_query) {
				Console.WriteLine (Message);
				return Console.ReadLine ();
			}
		}

		static SortedSet<string> Modifiers = new SortedSet<string> ();

		public static int ExtractModifiers (string[] Parts)
		{
			for (int n = 0; n != Parts.Length; n++) {
				if (!Modifiers.Contains (Parts [n]))
					return n;
			}
			return -1;
		}

		public static void Main (string[] args)
		{
			Modifiers.Add ("Async");
//			BD2.Common.Database DB = new BD2.Common.Database ();
			string command;
			do {
				command = Query ("Command>");
				BSO.OffsetedArray<string> commandparts = command.Split (' ');
				string[] CommandParts = (string[])((string[])commandparts).Clone ();
				commandparts.Offset = ExtractModifiers (CommandParts);
				string[] CommandModifiers = commandparts.GetStrippedPart ();
				switch (CommandParts [0]) {
				case "Open":
					switch (CommandParts [1]) {
					case "File":
						BD2.Block.ChunkRepository LRepo = new BD2.Repo.LevelDB.Repository ("/home/behrooz/Esfand/Repo/");
						LRepo.Pull ();
						break;
					case "Network":
						break;
					case "Socket":
						break;
					}
					break;
				case "Close":
					break;
				case "Execute":

					break;
				default:
					Console.Error.WriteLine (string.Format ("{0} is not a valid command.", CommandParts [0]));
					break;
				}
			} while(true);
		}
	}
}
