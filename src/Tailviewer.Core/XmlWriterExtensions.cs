using System;
using System.Globalization;
using System.Windows.Media;
using System.Xml;
using Tailviewer.Api;

namespace Tailviewer.Core
{
	/// <summary>
	///     Extension methods for the <see cref="XmlWriter" /> class.
	/// </summary>
	public static class XmlWriterExtensions
	{
		/// <summary>
		///     Writes the given id into an attribute with the given name.
		/// </summary>
		public static void WriteAttribute(this XmlWriter writer, string localName, DataSourceId id)
		{
			writer.WriteAttributeGuid(localName, id.Value);
		}

		/// <summary>
		///     Writes the given id into an attribute with the given name.
		/// </summary>
		public static void WriteAttribute(this XmlWriter writer, string localName, QuickFilterId id)
		{
			writer.WriteAttributeGuid(localName, id.Value);
		}

		/// <summary>
		///     Writes a <see cref="Guid"/> value as an attribute.
		/// </summary>
		public static void WriteAttributeGuid(this XmlWriter writer, string localName, Guid value)
		{
			writer.WriteAttributeString(localName, value.ToString());
		}

		/// <summary>
		///     Writes a <see cref="bool"/> value as an attribute.
		/// </summary>
		public static void WriteAttributeBool(this XmlWriter writer, string localName, bool value)
		{
			writer.WriteAttributeString(localName, value.ToString(CultureInfo.InvariantCulture));
		}

		/// <summary>
		///     Writes a <see cref="double"/> value as an attribute.
		/// </summary>
		public static void WriteAttributeDouble(this XmlWriter writer, string localName, double value)
		{
			writer.WriteAttributeString(localName, value.ToString(CultureInfo.InvariantCulture));
		}

		/// <summary>
		///     Writes an enum value as an attribute.
		/// </summary>
		public static void WriteAttributeEnum<T>(this XmlWriter writer, string localName, T value) where T : struct, Enum
		{
			writer.WriteAttributeString(localName, value.ToString());
		}

		/// <summary>
		///     Writes a <see cref="Color"/> value as an attribute.
		/// </summary>
		public static void WriteAttributeColor(this XmlWriter writer, string localName, Color color)
		{
			writer.WriteAttributeString(localName, color.ToString());
		}

		/// <summary>
		///     Writes an <see cref="int"/> value as an attribute.
		/// </summary>
		public static void WriteAttributeInt(this XmlWriter writer, string localName, int value)
		{
			writer.WriteAttributeString(localName, value.ToString(CultureInfo.InvariantCulture));
		}

		/// <summary>
		///     Writes a <see cref="DateTime"/> value as an attribute.
		/// </summary>
		public static void WriteAttributeDateTime(this XmlWriter writer, string localName, DateTime value)
		{
			writer.WriteAttributeString(localName, value.ToString("O", CultureInfo.InvariantCulture));
		}

		/// <summary>
		///     Writes a byte array as a Base64-encoded attribute.
		/// </summary>
		public static void WriteAttributeBase64(this XmlWriter writer, string localName, byte[] value)
		{
			if (value == null)
				writer.WriteAttributeString(localName, string.Empty);
			else
				writer.WriteAttributeString(localName, Convert.ToBase64String(value));
		}
	}
}