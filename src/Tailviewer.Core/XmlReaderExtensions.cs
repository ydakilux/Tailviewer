using System;
using System.Globalization;
using System.Windows.Media;
using System.Xml;
using log4net;
using Tailviewer.Api;

namespace Tailviewer.Core
{
	/// <summary>
	///     Extension methods for the <see cref="XmlReader" /> class.
	/// </summary>
	public static class XmlReaderExtensions
	{
		/// <summary>
		///     Reads the current attribute value as a <see cref="Guid"/>.
		/// </summary>
		public static Guid ReadContentAsGuid(this XmlReader reader)
		{
			var str = reader.ReadContentAsString();
			return Guid.TryParse(str, out var result) ? result : Guid.Empty;
		}

		/// <summary>
		///     Reads the current attribute value as a <see cref="bool"/>.
		/// </summary>
		public static bool ReadContentAsBool(this XmlReader reader)
		{
			var str = reader.ReadContentAsString();
			return bool.TryParse(str, out var result) && result;
		}

		/// <summary>
		///     Reads the current attribute value as a <see cref="bool"/> (alias for ReadContentAsBool).
		/// </summary>
		public static bool ReadContentAsBoolean(this XmlReader reader)
		{
			return reader.ReadContentAsBool();
		}

		/// <summary>
		///     Reads the current attribute value as an enum of type <typeparamref name="T"/>.
		/// </summary>
		public static T ReadContentAsEnum<T>(this XmlReader reader) where T : struct, Enum
		{
			var str = reader.ReadContentAsString();
			return Enum.TryParse<T>(str, ignoreCase: true, out var result) ? result : default;
		}

		/// <summary>
		///     Reads the contents as a <see cref="DataSourceId" />.
		/// </summary>
		public static DataSourceId ReadContentAsDataSourceId(this XmlReader reader)
		{
			var guid = reader.ReadContentAsGuid();
			return new DataSourceId(guid);
		}

		/// <summary>
		///     Reads the contents as a <see cref="QuickFilterId" />.
		/// </summary>
		public static QuickFilterId ReadContentAsQuickFilterId(this XmlReader reader)
		{
			var guid = reader.ReadContentAsGuid();
			return new QuickFilterId(guid);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="attributeName"></param>
		/// <param name="log"></param>
		/// <param name="defaultValue"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static void ReadAttributeAsInt(this XmlReader reader, string attributeName, ILog log, int defaultValue, out int value)
		{
			if (!TryMoveToAttribute(reader, attributeName, log))
			{
				value = defaultValue;
			}
			else
			{
				var content = reader.ReadContentAsString();
				if (!int.TryParse(content, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
				{
					log.WarnFormat("Cannot parse value '{0}' as an integer, restoring default value for attribute '{1}' instead",
					               content,
					               attributeName);
					value = defaultValue;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="attributeName"></param>
		/// <param name="log"></param>
		/// <param name="defaultValue"></param>
		/// <param name="value"></param>
		public static void ReadAttributeAsColor(this XmlReader reader, string attributeName, ILog log, Color defaultValue, out Color value)
		{
			if (!TryMoveToAttribute(reader, attributeName, log))
			{
				value = defaultValue;
			}
			else
			{
				var content = reader.ReadContentAsString();
				try
				{
					value = (Color) ColorConverter.ConvertFromString(content ?? string.Empty);
				}
				catch (Exception e)
				{
					log.WarnFormat("Cannot parse value '{0}' as an integer, restoring default value for attribute '{1}' instead:\r\n{2}",
					               content,
					               attributeName,
					               e);
					value = defaultValue;
				}
			}
		}

		/// <summary>
		///     Reads the current attribute value as an <see cref="int"/>.
		/// </summary>
		public static int ReadContentAsInt(this XmlReader reader)
		{
			var str = reader.ReadContentAsString();
			return int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : 0;
		}

		/// <summary>
		///     Reads the current attribute value as a <see cref="DateTime"/> (ISO 8601 round-trip format).
		///     Named "DateTime2" for compatibility with Metrolib.
		/// </summary>
		public static DateTime ReadContentAsDateTime2(this XmlReader reader)
		{
			var str = reader.ReadContentAsString();
			return DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result)
				? result
				: default;
		}

		/// <summary>
		///     Reads the current attribute value as a <see cref="double"/>.
		///     Named "Double2" for compatibility with Metrolib.
		/// </summary>
		public static double ReadContentAsDouble2(this XmlReader reader)
		{
			var str = reader.ReadContentAsString();
			return double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : 0.0;
		}

		/// <summary>
		///     Reads the current attribute value as a Base64-decoded byte array.
		/// </summary>
		public static byte[] ReadContentAsBase64(this XmlReader reader)
		{
			var str = reader.ReadContentAsString();
			if (string.IsNullOrEmpty(str))
				return null;
			try
			{
				return Convert.FromBase64String(str);
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private static bool TryMoveToAttribute(this XmlReader reader, string attributeName, ILog log)
		{
			if (!reader.MoveToAttribute(attributeName))
			{
				log.InfoFormat("Cannot find attribute '{0}', restoring default value instead", attributeName);
				return false;
			}

			return true;
		}
	}
}