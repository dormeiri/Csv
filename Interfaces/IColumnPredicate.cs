using System;

namespace Csv
{
	public interface IColumnPredicate
	{
		Type Type { get; }
		string HeaderName { get; set; }
	}
}
