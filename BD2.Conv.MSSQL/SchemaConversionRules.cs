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
