using System;

namespace Csv
{
	[AttributeUsage(AttributeTargets.Property)]
	public class CsvProperty : Attribute
	{

		public bool Ignore { get; set; } = false;
		public string HeaderName { get; set; } = null;

		public CsvProperty() { }
	}
}
