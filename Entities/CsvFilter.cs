using System.Collections.Generic;
using System.Linq;

namespace Csv
{
	public class CsvFilter
	{
		internal Dictionary<string, IColumnPredicate> Selectors { get; }

		public CsvFilter(params IColumnPredicate[] selectors)
		{
			Selectors = selectors.ToDictionary(x => x.HeaderName, x => x);
		}
	}
}
