//
//  CDRSorterhelper.cs
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
using System.Threading;

namespace BD2.Core
{
	internal sealed class CDRSorterhelper
	{
		ChunkRepository remote; public ChunkRepository Remote { get { return remote; } set { remote = value; } }
		SortedSet<byte[]> originalDescriptors; public SortedSet<byte[]> OriginalDescriptors { get { return originalDescriptors; } set { originalDescriptors = value; } }
		SortedSet<byte[]> exceptedDescriptors; public SortedSet<byte[]> ExceptedDescriptors { get { return exceptedDescriptors; } set { exceptedDescriptors = value; } }
		SortedSet<byte[]> missingDescriptors; public SortedSet<byte[]> MissingDescriptors { get { return missingDescriptors; } set { missingDescriptors = value; } }
		public CDRSorterhelper(ChunkRepository Remote)
		{
			remote = Remote;
			originalDescriptors = Remote.GetIndependentChunks();
		}
	}
	
	//public delegate void ChunkInsertedEventHandler 	(object sender, ChunkInsertedEventArgs e);
	
}
