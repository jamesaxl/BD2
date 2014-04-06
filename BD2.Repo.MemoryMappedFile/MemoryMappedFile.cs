//
//  MemoryMappedFile.cs
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
using BSO;
using System.IO;
using BD2.Core;

namespace BD2.Repo.MemoryMappedFile
{
	public class MemoryMappedFile : ChunkData
	{
		string path;

		public string Path { get { return path; } }

		MemoryMappedFileRepository repo;

		public MemoryMappedFile (ChunkDescriptor ChunkDescriptor, MemoryMappedFileRepository Repo, string Path)
			:base (ChunkDescriptor)
		{
			if (ChunkDescriptor == null)
				throw new ArgumentNullException ("ChunkDescriptor");
			if (Repo == null)
				throw new ArgumentNullException ("Repo");
			if (Path == null)
				throw new ArgumentNullException ("Path");
			repo = Repo;
			path = Path;
		}

		public MemoryMappedFile (ChunkDescriptor ChunkDescriptor, MemoryMappedFileRepository Repo, string Path, byte[] Data)
			:base(ChunkDescriptor)
		{
			if (ChunkDescriptor == null)
				throw new ArgumentNullException ("ChunkDescriptor");
			if (Data == null)
				throw new ArgumentNullException ("Data");
			if (Path == null)
				throw new ArgumentNullException ("Path");
			if (Repo == null)
				throw new ArgumentNullException ("Repo");
			repo = Repo;
			path = Path;
			System.IO.File.WriteAllBytes (Path, Data);
			Init ();
		}

		public MemoryMappedFile (ChunkDescriptor ChunkDescriptor, MemoryMappedFileRepository Repo, string Path, Stream Stream)
			:base(ChunkDescriptor)
		{
			if (ChunkDescriptor == null)
				throw new ArgumentNullException ("ChunkDescriptor");
			if (Path == null)
				throw new ArgumentNullException ("Path");
			if (Repo == null)
				throw new ArgumentNullException ("Repo");
			repo = Repo;
			path = Path;
			Stream OStream = System.IO.File.OpenWrite (path);
			long Count = ((Stream.Length - 1) >> 20) + 1;
			for (long n = 0; n != Count; n++) {
				byte[] Bytes = new byte[1 << 20];
				Stream.Read (Bytes, 0, 1 << 20);
				OStream.Write (Bytes, 0, 1 << 20);
			}
			Init ();
		}

		System.IO.MemoryMappedFiles.MemoryMappedFile MMF;

		void Init ()
		{
			MMF = System.IO.MemoryMappedFiles.MemoryMappedFile.OpenExisting (
				path, System.IO.MemoryMappedFiles.MemoryMappedFileRights.Read,
				HandleInheritability.None);
		}

		public override Stream GetRawData (int Offset, int Count  = -1)
		{
			//TODO: TEST
			return MMF.CreateViewStream (Offset, Count);
		}

		public override bool IsAvailable {
			get {
				return System.IO.File.Exists (path);//enough for now
			}
		}
	}
}
