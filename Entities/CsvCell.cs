using Csv.Extentions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Csv
{
	internal class CsvCell
	{
		private readonly CsvOptions _options;
		private readonly object _value;
		private readonly bool _inner; //Used for arrays seperation
		private bool NeedQuote => !_inner && _options.Quoting;

		private CsvCell(object value, bool inner, CsvOptions options)
		{
			_options = options;
			_value = value;
			_inner = inner;
		}
		private CsvCell(CsvOptions options)
		{
			_options = options;
			_inner = false;
		}


		#region Serialization

		internal static string Serialize(object value, CsvOptions options)
		{
			return Serialize(value, false, options);
		}

		private static string Serialize(object value, bool inner, CsvOptions options)
		{
			return new CsvCell(value, inner, options).Serialize();
		}

		private string Serialize()
		{
			if (_value == null)
			{
				return string.Empty;
			}
			else if (_value is string s)
			{
				return Serialize(s);
			}
			else if (_value is IList list && !_inner)
			{
				return Serialize(list);
			}
			else if (_value is DateTime dt)
			{
				return Serialize(dt);
			}

			return _value.ToString();
		}

		private string Serialize(DateTime dateTime)
		{
			return dateTime.ToString(_options.DateTimeFormat);
		}
		private string Serialize(IEnumerable list)
		{
			var result = "";

			foreach (var node in list)
			{
				result += Serialize(node, true, _options) + _options.ArraySeperator;
			}

			return result.Length == 0 ? null : Serialize(result.Remove(result.Length - 1));
		}
		private string Serialize(string str)
		{
			str = str.Clean();
			if (str != null && NeedQuote)
			{
				str = str.Quote();
			}

			return str;
		}

		#endregion

		#region Deserialization

		internal static bool SetValue(PropertyInfo prop, object obj, string str, CsvOptions options)
		{
			return new CsvCell(options).SetValue(prop, obj, str);
		}

		private bool SetValue(PropertyInfo prop, object obj, string str)
		{
			var result = true;
			var type = prop.PropertyType;
			if (typeof(ICollection).IsAssignableFrom(type))
			{
				var elemType = type.GetElementType();
				var list = DeserializeArray(str, elemType).ToList();
				var arr = Array.CreateInstance(elemType, list.Count);
				var i = 0;

				list.ForEach(a => arr.SetValue(a, i++));
				prop.SetValue(obj, arr);
			}
			else
			{
				var value = Deserialize(str, type);
				if (result = IsSelected(value, prop))
				{
					prop.SetValue(obj, value);
				}
			}
			return result;
		}

		private bool IsSelected(object value, PropertyInfo prop)
		{
			var result = true;
			if (_options.Filter?.Selectors != null)
			{
				if (_options.Filter.Selectors.TryGetValue(prop.Name, out var selector))
				{
					if (selector.Type == typeof(string))
					{
						result = ((ColumnPredicate<string>)selector).Select((string)value);
					}
					else if (selector.Type == typeof(double))
					{
						result = ((ColumnPredicate<double>)selector).Select((double)value);
					}
					else if (selector.Type == typeof(int))
					{
						result = ((ColumnPredicate<int>)selector).Select((int)value);
					}
				}
			}
			return result;
		}

		private IEnumerable<object> DeserializeArray(string str, Type elementType)
		{
			foreach (var subStr in str.Split(new[] { _options.ArraySeperator }, StringSplitOptions.None))
			{
				yield return Deserialize(subStr, elementType);
			}
		}

		private object Deserialize(string str, Type type)
		{
			if (string.IsNullOrEmpty(str))
			{
				return null;
			}
			if (type == typeof(DateTime))
			{
				return str.ParseDateTime(_options.DateTimeFormat);
			}
			return Convert.ChangeType(str, Nullable.GetUnderlyingType(type) ?? type);
		}


		#endregion
	}
}
