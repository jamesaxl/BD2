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
using System.IO;

namespace BD2.Frontend.Table.Model
{
	public abstract class ColumnSetOffsetHandler
	{
		private static object lock_Handlers = new object ();
		private static System.Collections.Generic.SortedDictionary<string, ColumnSetOffsetHandler> handlers = new SortedDictionary<string, ColumnSetOffsetHandler> ();

		public ColumnSetOffsetHandler ()
		{
			try {
				if (Register)
					lock (lock_Handlers)
						handlers.Add (Name, this);
			} catch (Exception ex) {
				throw new SystemException ("Something is really wrong with your cpu/memory configuration", ex);//I should do this more often
			} finally {

			}
		}

		public abstract string Name{ get; }

		protected abstract bool Register{ get; }

		public abstract bool Streamed { get; }

		public abstract bool CanCache { get; }

		public abstract bool Cache { get; set; }

		public abstract long[] GetOffsetMapFor (bool RelativeIn, bool RelativeOut, ColumnSet ColumnSet, Stream RawData);

		public abstract long[] GetOffsetMapFor (bool RelativeIn, bool RelativeOut, ColumnSet ColumnSet, byte[] RawData);

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
		public abstract int GetOffsetMapFor (bool RelativeIn, bool RelativeOut, ColumnSet ColumnSet, object[] Values, Stream Out);

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
		public abstract int GetOffsetMapFor (bool RelativeIn, bool RelativeOut, ColumnSet ColumnSet, object[] Values, byte[] Out);
	}
}