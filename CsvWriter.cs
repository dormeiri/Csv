using System.Collections.Generic;
using System.IO;

namespace Csv
{
	public class CsvWriter<T> where T : class, new()
	{
		private readonly TextWriter _writer;
		private readonly CsvOptions _options;

		public CsvWriter(TextWriter writer)
		{
			_writer = writer;
			_options = CsvOptions.CreateDefault();
		}

		public CsvWriter(TextWriter writer, CsvOptions options)
		{
			_writer = writer;
			_options = options;
		}

		public void Write(IEnumerable<T> objs)
		{
			foreach (var obj in objs)
			{
				Write(obj);
			}
		}

		public void Write(T obj)
		{
			WriteLine(CsvRow<T>.Serialize(obj, _options));
		}

		public void WriteHeader()
		{
			WriteLine(CsvRow<T>.SerializeHeader(_options));
		}

		private void WriteLine(string row)
		{
			_writer.WriteLine(row);
		}
	}
}
