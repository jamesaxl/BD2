//
//  ColumnSet.cs
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
using BD2.Common;


namespace BD2.Frontend.Table.Model
{
	public abstract class ColumnSet : BaseDataObject
	{
		public abstract ColumnSetOffsetHandler OffsetHandler{  get; }
		public abstract Table Table { get; }
		public abstract IEnumerable<Column> GetColumns ();
		public abstract object[] FromRaw(byte[] Raw);
		public abstract object[] FromRawStream(Stream Raw);
		public abstract byte[] ToRaw(object[] Objects);
		/// <summary>
		/// Writes Objects to Stream
		/// </summary>
		/// <returns>
		/// The number of bytes written to stream
		/// </returns>
		/// <param name='Objects'>
		/// Objects to serialize as column values
		/// </param>
		/// <param name='Stream'>
		/// stream to write data to
		/// </param>
		public abstract int ToRawStream(object[] Objects, Stream Stream);
		public abstract void Retrieve(Column Column);
	}
}