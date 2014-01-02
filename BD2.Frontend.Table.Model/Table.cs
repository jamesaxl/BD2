using System;
using System.Collections.Generic;
using System.IO;
using BD2.Common;


namespace BD2.Frontend.Table.Model
{
	public abstract class Table : BaseDataObject
	{
		public abstract string Name { get; }
		public abstract IEnumerable<Row> GetRows();
		public abstract IEnumerable<Row> GetRows(IndexBase Index);
		public abstract IEnumerable<Row> GetRows(Predicate<Row> Predicate);
		public abstract IEnumerable<Row> GetRows(IndexBase Index, Predicate<Row> Predicate);
		public abstract IEnumerable<IndexBase> GetIndices();
		public abstract IEnumerable<ColumnSet> GetColumnSets();
		public abstract Guid LoadConverter(ColumnSet Source, ColumnSet Destination, Func<byte[], byte[]> ConvertProc);
		public abstract Guid GetConverterID(ColumnSet Source, ColumnSet Destination, bool Dijkstra);
		public abstract Func<byte[], byte[]> GetConverterByID(Guid ID);
		public abstract void UnloadConverter(Guid ID);

	}
}
