//
//  WeakChunkReference.cs
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

namespace BD2.Common
{
	public class WeakChunkReference
	{
		Func<ChunkData> createTarget;
		BSO.WeakReference<ChunkData> reference;
		public bool IsAlive {
			get
			{
				return reference.IsAlive;
			}
		}
		public ChunkData Target {
			get
			{
				ChunkData CD = reference.Target;
				if(CD == null)
					return Resurrect();
				return CD;
			}
		}
		public ChunkData Resurrect ()
		{
			ChunkData CD = reference.Target;
			if(CD == null) {
				ChunkData NCD = createTarget();
				reference.Target = NCD;
				if(NCD == null)
					throw new Exception("ChunkData resurrection failed @ WeakChunkReference.\nThis is not an error prevention mechanism.\nThis is a real error.\nAnd its not what you think it is.");
				return NCD;
			}
			return CD;
		}
		public WeakChunkReference (ChunkData Target, Func<ChunkData> CreateTarget)
		{
			reference = new BSO.WeakReference<ChunkData> (Target);
			createTarget = CreateTarget;
		}
	}
}
