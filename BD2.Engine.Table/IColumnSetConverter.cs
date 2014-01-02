//
//  IColumnSetConverter.cs
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
	public interface IColumnSetConverter
	{
		IEnumerable<Guid> GetOfficiallySupportedColumnSets();
		int CostOf(Guid ID);
		int StreamCostOf(Guid ID);
		bool IsStreamedFor(Guid ID);
		bool IsParallelFor(Guid ID);
		bool Supports(ColumnSet Source);
		byte[] Convert(byte[] Input);
		Stream Convert(Stream Input);
		IEnumerable<byte[]> ConvertBatch(IEnumerable<byte[]> Input);
		IEnumerable<Stream> ConvertBatch(IEnumerable<Stream> Input);
	}
	
}
