//
//  Transform.cs
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

namespace BD2.Conv
{
	[Serializable]
	public class Transform
	{
		public LibraryConfiguration[] Libraries {
			get;
			set;
		}

		public DestinationEngine[] EnabledDestinationEngines {
			get;
			set;
		}

		public DestinationEngineInstance[] DestinationEngineInstances {
			get;
			set;
		}

		public SourceEngine[] EnabledSourceEngines {
			get;
			set;
		}

		public SourceEngineInstance[] SourceEsginInstances {
			get;
			set;
		}

		public SourceTransform[] SourceTransforms {
			get;
			set;
		}

		public TableNameTranslation[] TableNameTranslations {
			get;
			set;
		}
	}
}

