using System;

namespace Csv
{
	public struct ColumnPredicate<T> : IColumnPredicate
	{
		public string HeaderName { get; set; }
		public Predicate<T> Select { get; set; }
		public Type Type => typeof(T);

		public ColumnPredicate(string headerName, Predicate<T> filter)
		{
			HeaderName = headerName;
			Select = filter;
		}
	}
}
