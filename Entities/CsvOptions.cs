namespace Csv
{
	public class CsvOptions
	{
		internal const char QUOTE = '\"';
		internal const char DELIMITER = ',';

		public string DateTimeFormat { get; set; } = "yyyy/MM/dd HH:mm:ss";

		public char ArraySeperator { get; set; } = '|';
		public bool Quoting { get; set; } = true;
		/// <summary>
		/// Use only when reading to match the column index to the property by its name.
		/// </summary>
		public string[] HeadersNames { get; set; } = null;
		public bool IncludeHeaders { get; set; } = true;

		internal CsvFilter Filter { get; set; } = null;

		public static CsvOptions CreateDefault()
		{
			return new CsvOptions();
		}
	}
}
