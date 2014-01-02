//
//  ColumnSetOffsetHandler.cs
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
using System.IO;
namespace BD2.Frontend.Table.Model
{
	public abstract class ColumnSetOffsetHandler
	{
		private static object lock_Handlers = new object();
		private static System.Collections.Generic.SortedDictionary<string, ColumnSetOffsetHandler> handlers = new SortedDictionary<string, ColumnSetOffsetHandler>();
		public ColumnSetOffsetHandler(){
			try {
				if(Register)
					lock(lock_Handlers)
						handlers.Add (Name, this);
			} catch(Exception ex) {
				throw new SystemException("Something is really wrong with your cpu/memory configuration", ex);//I should do this more often
			} finally {

			}
		}
		public abstract string Name{ get; }
		protected abstract bool Register{ get; }
		public abstract bool Streamed { get; }
		public abstract bool CanCache { get; }
		public abstract bool Cache { get; set; }
		public abstract long[] GetOffsetMapFor(bool RelativeIn, bool RelativeOut, ColumnSet ColumnSet, Stream RawData);
		public abstract long[] GetOffsetMapFor(bool RelativeIn, bool RelativeOut, ColumnSet ColumnSet, byte[] RawData);
		/// <summary>
		/// Gets the offset map for columns .
		/// </summary>
		/// <returns>
		/// Number of offsets written.
		/// </returns>
		/// <param name='RelativeIn'>
		/// Whether the input is relative.
		/// </param>
		/// <param name='RelativeOut'>
		/// Whether the output should be relative.
		/// </param>
		/// <param name='Columns'>
		/// Columns to write metadata for.
		/// </param>
		/// <param name='Values'>
		/// Values to take into account for writing metadata.
		/// </param>
		public abstract int GetOffsetMapFor(bool RelativeIn, bool RelativeOut, ColumnSet ColumnSet, object[] Values, Stream Out);
		/// <summary>
		/// Gets the offset map for columns .
		/// </summary>
		/// <returns>
		/// Number of offsets written.
		/// </returns>
		/// <param name='RelativeIn'>
		/// Whether the input is relative.
		/// </param>
		/// <param name='RelativeOut'>
		/// Whether the output should be relative.
		/// </param>
		/// <param name='Columns'>
		/// Columns to write metadata for.
		/// </param>
		/// <param name='Values'>
		/// Values to take into account for writing metadata.
		/// </param>
		public abstract int GetOffsetMapFor(bool RelativeIn, bool RelativeOut, ColumnSet ColumnSet, object[] Values, byte[] Out);
	}	
}