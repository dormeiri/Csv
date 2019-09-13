using System;
using System.Globalization;

namespace Csv.Extentions
{
	internal static class CellExtention
	{
		internal static string Quote(this string str)
		{
			return CsvOptions.QUOTE + str + CsvOptions.QUOTE;
		}

		internal static string Unquote(this string str)
		{
			return str.TrimStart(CsvOptions.QUOTE).TrimEnd(CsvOptions.QUOTE);
		}

		internal static string Clean(this string str)
		{
			return str?.Replace("\"", "").Replace("\n", "").Replace("\r", "");
		}
		internal static DateTime ParseDateTime(this string str, string dateTimeFormat)
		{
			return DateTime.ParseExact(str, dateTimeFormat, CultureInfo.InvariantCulture);
		}
	}
}
