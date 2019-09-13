using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Csv
{
	internal class CsvRow<T> where T : class, new()
	{
		internal T Obj { get; }
		private readonly CsvOptions _options;

		private CsvRow(CsvOptions options)
		{
			_options = options;
			Obj = new T(); //Not null for deserialization
		}
		private CsvRow(T obj, CsvOptions options)
		{
			_options = options;
			Obj = obj;
		}

		#region Serialization

		internal static string SerializeHeader(CsvOptions options)
		{
			return new CsvRow<T>(options).SerializeHeader();
		}
		internal static string Serialize(T obj, CsvOptions options)
		{
			return new CsvRow<T>(obj, options).Serialize();
		}

		private string SerializeHeader()
		{
			var result = "";
			foreach (var (prop, attr) in GetGetProperties())
			{
				result += CsvCell.Serialize(attr.HeaderName ?? prop.Name, _options) + CsvOptions.DELIMITER;
			}
			return Finalize(result);
		}
		private string Serialize()
		{
			var result = "";
			foreach (var (prop, attr) in GetGetProperties())
			{
				result += CsvCell.Serialize(prop.GetValue(Obj), _options) + CsvOptions.DELIMITER;
			}
			return Finalize(result);
		}

		#endregion

		#region Deserialization

		internal static PropertyInfo[] DeserializeHeaders(string row, CsvOptions options)
		{
			return new CsvRow<T>(options).DeserializeHeader(row);
		}
		internal static PropertyInfo[] DeserializeHeaders(string[] headerNames, CsvOptions options)
		{
			return new CsvRow<T>(options).DeserializeHeaders(headerNames);
		}
		internal static T Deserialize(string row, PropertyInfo[] headers, CsvOptions options)
		{
			return new CsvRow<T>(options).Deserialize(row, headers);
		}

		private PropertyInfo[] DeserializeHeader(string row)
		{
			return DeserializeHeaders(SplitRow(row));
		}
		private PropertyInfo[] DeserializeHeaders(IEnumerable<string> headerNames)
		{
			var dict = GetSetProperties().ToDictionary(pa => pa.attr.HeaderName ?? pa.prop.Name, pa => pa.prop);
			var result = new List<PropertyInfo>();
			foreach (var header in headerNames)
			{
				//Add null where property not found to indicate to ignore this column index
				result.Add(dict.TryGetValue(header ?? string.Empty, out var prop)
						? prop
						: null
					);
			}

			return result.ToArray();
		}
		private T Deserialize(string row, PropertyInfo[] headers)
		{
			var i = 0;
			foreach (var cell in SplitRow(row))
			{
				var prop = headers[i++];
				if (prop != null)
				{
					if (!CsvCell.SetValue(prop, Obj, cell, _options))
					{
						return null;
					}
				}
			}
			return Obj;
		}

		private IEnumerable<string> SplitRow(string row)
		{
			//TODO: Fix newline in quotes
			var quoteSwitch = false;
			var cell = "";
			foreach (var c in row)
			{
				switch (c)
				{
					case CsvOptions.DELIMITER when !quoteSwitch:
						yield return cell;
						cell = "";
						break;
					case CsvOptions.QUOTE when _options.Quoting:
						quoteSwitch = !quoteSwitch;
						break;
					default:
						cell += c;
						break;
				}
			}
			yield return cell;
		}

		#endregion

		private static IEnumerable<(PropertyInfo prop, CsvProperty attr)> GetGetProperties()
		{
			return GetProperties(true, false);
		}
		private static IEnumerable<(PropertyInfo prop, CsvProperty attr)> GetSetProperties()
		{
			return GetProperties(false, true);
		}
		private static IEnumerable<(PropertyInfo prop, CsvProperty attr)> GetProperties(bool get, bool set)
		{
			foreach (var prop in typeof(T).GetProperties().Where(p => (set ? p.GetSetMethod() != null : true) && (get ? p.GetGetMethod() != null : true)))
			{
				var attr = (CsvProperty)Attribute.GetCustomAttribute(prop, typeof(CsvProperty));
				if (attr == null)
				{
					yield return (prop, new CsvProperty());
				}
				else if (attr.Ignore == false)
				{
					yield return (prop, attr);
				}
			}
		}
		private static string Finalize(string row)
		{
			return string.IsNullOrEmpty(row) ? string.Empty : row.Remove(row.Length - 1);
		}
	}
}
