using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Csv
{
	public static class CsvReader
	{
		public static IEnumerable<T> Read<T>(TextReader reader, CsvFilter filter = null) where T : class, new()
		{
			return Read<T>(reader, CsvOptions.CreateDefault(), filter);
		}
		public static IEnumerable<T> Read<T>(TextReader reader, CsvOptions options, CsvFilter filter = null) where T : class, new()
		{
			return new CsvReader<T>(reader, options, filter).Read();
		}
	}
	internal class CsvReader<T> where T : class, new()
	{
		private readonly TextReader _reader;
		private readonly CsvOptions _options;
		private PropertyInfo[] _headers;

		internal CsvReader(TextReader reader, CsvOptions options, CsvFilter filter)
		{
			_reader = reader;
			_options = options;
			_options.Filter = filter;
		}

		internal IEnumerable<T> Read()
		{
			if (Headers(out _headers))
			{
				while (ReadLine(out var line))
				{
					if (Deserialize(line, out var result))
					{
						yield return result;
					}
				}
			}
		}

		private bool Headers(out PropertyInfo[] headers)
		{
			headers = _options.IncludeHeaders
				? ReadLine(out var line)
					? CsvRow<T>.DeserializeHeaders(line, _options)
					: null
				: CsvRow<T>.DeserializeHeaders(_options.HeadersNames, _options);

			return headers?.All(header => header == null) == false;
		}

		private bool ReadLine(out string line)
		{
			return (line = _reader.ReadLine()) != null;
		}

		private bool Deserialize(string line, out T obj)
		{
			return (obj = CsvRow<T>.Deserialize(line, _headers, _options)) != null;
		}
	}
}
