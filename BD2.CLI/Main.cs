//
//  Main.cs
//
//  Author:
//       Behrooz Amoozad <behrooz0az@gmail.com>
//
//  Copyright (c) 2013 behrooz
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
