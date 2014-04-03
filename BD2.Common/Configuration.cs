//
//  Configuration.cs
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
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

namespace BD2.Common
{
	public static class Configuration
	{
		static string defaultConfigName = "Default";
		public static readonly bool MS;

		static Configuration ()
		{
			System.PlatformID PID = System.Environment.OSVersion.Platform;
			MS = (PID == PlatformID.Win32NT) || (PID == PlatformID.WinCE) || (PID == PlatformID.Win32Windows) || (PID == PlatformID.Win32S);
		}

		public static string DefaultConfigName {
			set {
				defaultConfigName = value;
			}
		}

		public static string HOMEVARIABLE {
			get { 
				if (MS)
					return "UserProfile";
				return "HOME";
			}
		}

		public static void SetConfig<T> (string ApplicationName, string ConfigurationName, string DataName, Type[] ExtraTypes, T Value) where T : class
		{
			if (ConfigurationName == null) {//the parameter to be removed in future to ensure safe multi profile usage.
				ConfigurationName = defaultConfigName;
			}
#if DEBUG
			else {
				Console.Error.WriteLine ("(EE) System profile failed to load");

			}
#endif
			XmlSerializer XMLS = new XmlSerializer (typeof(T), ExtraTypes);
			string UP = System.Environment.GetEnvironmentVariable (HOMEVARIABLE);
			if (Directory.Exists (UP)) {
				string PDIR = UP + Path.DirectorySeparatorChar + ApplicationName + Path.DirectorySeparatorChar;
				if (!Directory.Exists (PDIR))
					Directory.CreateDirectory (PDIR);
				string CDIR = PDIR + ConfigurationName + Path.DirectorySeparatorChar;
				if (!Directory.Exists (CDIR))
					Directory.CreateDirectory (CDIR);
				string CFILE = CDIR + DataName + ".Config";
				File.Delete (CFILE);
				StreamWriter SW = new StreamWriter (File.OpenWrite (CFILE));
				XMLS.Serialize (SW, Value);
				SW.Close ();
			} else
				throw new Exception ();
		}

		static Dictionary<String, Dictionary<String, Dictionary<String, object>>> Configs = new Dictionary<string, Dictionary<string, Dictionary<string, object>>> ();

		public static T GetConfig<T> (string ApplicationName, string ConfigurationName, string DataName, Type[] ExtraTypes, Func<string, string, T> CreateDefault) where T : class
		{

			if (ConfigurationName == null) {//the parameter to be removed in future to ensure safe multi profile usage.
				ConfigurationName = defaultConfigName;
			}
			if (!Configs.ContainsKey (ApplicationName)) {
				Configs.Add (ApplicationName, new Dictionary<string, Dictionary<string, object>> ());
			}
			Dictionary<string, Dictionary<string, object>> AppConfigs = Configs [ApplicationName];
			lock (AppConfigs) {
				if (!AppConfigs.ContainsKey (ConfigurationName)) {
					AppConfigs.Add (ConfigurationName, new Dictionary<string, object> ());
				}
				Dictionary<string, object> ConfConfigs = AppConfigs [ConfigurationName];
				lock (ConfConfigs) {
					if (!ConfConfigs.ContainsKey (DataName)) {
						XmlSerializer XMLS = new XmlSerializer (typeof(T), ExtraTypes);
						T Result = null;
						string UP = System.Environment.GetEnvironmentVariable (HOMEVARIABLE);
						if (Directory.Exists (UP)) {
							if (File.Exists (UP + Path.DirectorySeparatorChar + ApplicationName + Path.DirectorySeparatorChar + ConfigurationName + Path.DirectorySeparatorChar + DataName + ".Config")) {
								Result = (T)XMLS.Deserialize (new StreamReader (UP + Path.DirectorySeparatorChar + ApplicationName + Path.DirectorySeparatorChar + ConfigurationName + Path.DirectorySeparatorChar + DataName + ".Config"));
							} else {
								Result = CreateDefault (ConfigurationName, DataName);
								if (!Directory.Exists (UP + Path.DirectorySeparatorChar + ApplicationName + Path.DirectorySeparatorChar))
									Directory.CreateDirectory (UP + Path.DirectorySeparatorChar + ApplicationName + Path.DirectorySeparatorChar);
								if (!Directory.Exists (UP + Path.DirectorySeparatorChar + ApplicationName + Path.DirectorySeparatorChar + ConfigurationName + Path.DirectorySeparatorChar))
									Directory.CreateDirectory (UP + Path.DirectorySeparatorChar + ApplicationName + Path.DirectorySeparatorChar + ConfigurationName + Path.DirectorySeparatorChar);
								StreamWriter SW = new StreamWriter (File.OpenWrite (UP + Path.DirectorySeparatorChar + ApplicationName + Path.DirectorySeparatorChar + ConfigurationName + Path.DirectorySeparatorChar + DataName + ".Config"));
								XMLS.Serialize (SW, Result);
								SW.Close ();
							}
							ConfConfigs.Add (DataName, Result);
							return Result;
						}
						throw new Exception ("Something went wrong with BD2.Configuration.");
					}
					return (T)ConfConfigs [DataName];
				}
			}
		}
	}
}