//
//  SchemaConversionRules.cs
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
using Mono.CSharp;

namespace BD2.Conv.MSSQL
{
	[Serializable]
	public class SchemaConversionRules
	{
		readonly string sourceTable;
		readonly Guid destinationTableID;
		readonly string firstPassTransform;
		readonly string[] intermediateTransforms;
		readonly SortedDictionary<Guid, string> lastPassTransform;
		CompiledMethod firstPassTransformMethod;
		CompiledMethod[] intermediateTransformMethods;
		SortedDictionary<Guid, CompiledMethod> lastPassTransformMethods;
		Evaluator evaluator;

		public string SourceTable {
			get {
				return sourceTable;
			}
		}

		public Guid DestinationTableID {
			get {
				return destinationTableID;
			}
		}

		public SchemaConversionRules (string sourceTable, Guid destinationTableID, string firstPassTransform, string[] intermediateTransfroms, SortedDictionary<Guid, string> lastPassTransform)
		{
			if (sourceTable == null)
				throw new ArgumentNullException ("sourceTable");
			if (firstPassTransform == null)
				throw new ArgumentNullException ("firstPassTransform");
			if (intermediateTransfroms == null)
				throw new ArgumentNullException ("intermediateTransfroms");
			if (lastPassTransform == null)
				throw new ArgumentNullException ("lastPassTransform");
			this.sourceTable = sourceTable;
			this.destinationTableID = destinationTableID;
			this.firstPassTransform = firstPassTransform;
			lock (intermediateTransfroms)
				this.intermediateTransforms = (string[])intermediateTransfroms.Clone ();
			lock (lastPassTransform)
				this.lastPassTransform = new SortedDictionary<Guid, string> (lastPassTransform);
		}

		void InitializeTransformMethods ()
		{
			Evaluator Eval = evaluator;
			if (Eval == null) {
				ReportPrinter rp = new NullReportPrinter ();
				CompilerSettings cs = new Mono.CSharp.CompilerSettings ();
				CompilerContext ctx = new Mono.CSharp.CompilerContext (cs, rp);
				Eval = new Mono.CSharp.Evaluator (ctx);
				Eval.ReferenceAssembly (System.Reflection.Assembly.GetExecutingAssembly ());
				//atomic and stupid at the same time, too much garbage.
				evaluator = Eval;
			}

			firstPassTransformMethod = Eval.Compile (firstPassTransform);
			intermediateTransformMethods = new CompiledMethod[intermediateTransforms.Length]; 
			for (int n = 0; n != intermediateTransforms.Length; n++) {
				intermediateTransformMethods [n] = Eval.Compile (intermediateTransforms [n]);
			}
			lastPassTransformMethods = new SortedDictionary<Guid, CompiledMethod> ();
			 
			foreach (var KVP in lastPassTransform) {
				lastPassTransformMethods.Add (KVP.Key, Eval.Compile (KVP.Value));
			}

		}

		SortedDictionary<Guid, object> Convert (ICloneable input)
		{
			if (input == null)
				throw new ArgumentNullException ("input");
			//TODO:FIX
			throw new NotImplementedException ("whoreshit");
			InitializeTransformMethods ();
			object Ref = input.Clone ();
			firstPassTransformMethod (ref Ref);
			foreach (var CM in intermediateTransformMethods)
				CM (ref Ref);
			SortedDictionary<Guid, Object> lRefs = new SortedDictionary<Guid, object> ();
			foreach (var KVP in lastPassTransformMethods) {
				KVP.Value (ref Ref);
				lRefs.Add (KVP.Key, Ref);
			}
			return lRefs;
		}
	}
}
