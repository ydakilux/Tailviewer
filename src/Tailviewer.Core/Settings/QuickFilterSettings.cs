using System;
using System.Diagnostics.Contracts;
using System.Xml;
using Metrolib;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	///     The configuration of an application-wide quick filter.
	/// </summary>
	public sealed class QuickFilterSettings
		: ICloneable
		, ISerializableType
	{
		/// <summary>
		///     The id of this quick filter.
		///     Is used to define for each data source which quick filter is active or not.
		/// </summary>
		public QuickFilterId Id;

		/// <summary>
		///     True when the case of the filter value doesn't matter.
		/// </summary>
		public bool IgnoreCase;

		/// <summary>
		///     When set to false, then a line will only be shown if it matches the filter.
		///     When set to true, then only those lines NOT matching the filter will be shown.
		/// </summary>
		public bool IsInverted;

		/// <summary>
		///     How <see cref="Value" /> is to be intepreted.
		/// </summary>
		public FilterMatchType MatchType;

	/// <summary>
	///     The actual filter value, <see cref="MatchType" /> defines how it is interpreted.
	/// </summary>
	public string Value;

	/// <summary>
	///     The background color to use when highlighting matching log lines.
	///     When null, no highlighting is applied.
	/// </summary>
	public System.Windows.Media.Color? HighlightColor;

	/// <summary>
	///     The foreground (text) color to use when highlighting matching log lines.
	///     When null, the default foreground color is used.
	/// </summary>
	public System.Windows.Media.Color? ForegroundColor;

	/// <summary>
	///     When set to true, the filter operates in "highlight mode":
	///     all lines are shown, but matching lines are highlighted with the specified colors.
	///     When set to false (default), the filter operates in "hide mode":
	///     only matching lines are shown (current behavior).
	/// </summary>
	public bool IsHighlightOnly;

	/// <summary>
	///     Initializes this quick filter.
	/// </summary>
	public QuickFilterSettings()
	{
		Id = QuickFilterId.CreateNew();
		IgnoreCase = true;
		IsInverted = false;
		HighlightColor = null;
		ForegroundColor = null;
		IsHighlightOnly = false;
	}

		object ICloneable.Clone()
		{
			return Clone();
		}

	/// <summary>
	///     Restores this filter from the given xml reader.
	/// </summary>
	/// <param name="reader"></param>
	/// <returns></returns>
	public bool Restore(XmlReader reader)
	{
		var count = reader.AttributeCount;
		for (var i = 0; i < count; ++i)
		{
			reader.MoveToAttribute(i);

			switch (reader.Name)
			{
				case "id":
					Id = reader.ReadContentAsQuickFilterId();
					break;

				case "type":
					MatchType = reader.ReadContentAsEnum<FilterMatchType>();
					break;

				case "value":
					Value = reader.Value;
					break;

				case "ignorecase":
					IgnoreCase = reader.ReadContentAsBool();
					break;

				case "isinclude":
					IsInverted = reader.ReadContentAsBool();
					break;

				case "highlightcolor":
					HighlightColor = HexToColor(reader.Value);
					break;

				case "foregroundcolor":
					ForegroundColor = HexToColor(reader.Value);
					break;

				case "ishighlightonly":
					IsHighlightOnly = reader.ReadContentAsBool();
					break;
			}
		}

		if (Id == QuickFilterId.Empty)
			return false;

		return true;
	}

	/// <summary>
	///     Saves the contents of this object into the given writer.
	/// </summary>
	/// <param name="writer"></param>
	public void Save(XmlWriter writer)
	{
		writer.WriteAttribute("id", Id);
		writer.WriteAttributeEnum("type", MatchType);
		writer.WriteAttributeString("value", Value);
		writer.WriteAttributeBool("ignorecase", IgnoreCase);
		writer.WriteAttributeBool("isinclude", IsInverted);
		writer.WriteAttributeBool("ishighlightonly", IsHighlightOnly);
		if (HighlightColor.HasValue)
			writer.WriteAttributeString("highlightcolor", ColorToHex(HighlightColor.Value));
		if (ForegroundColor.HasValue)
			writer.WriteAttributeString("foregroundcolor", ColorToHex(ForegroundColor.Value));
	}

	/// <summary>
	///     Creates a deep clone of this object.
	/// </summary>
	/// <returns></returns>
	public QuickFilterSettings Clone()
	{
		return new QuickFilterSettings
		{
			Id = Id,
			IgnoreCase = IgnoreCase,
			IsInverted = IsInverted,
			MatchType = MatchType,
			Value = Value,
			HighlightColor = HighlightColor,
			ForegroundColor = ForegroundColor,
			IsHighlightOnly = IsHighlightOnly
		};
	}

		/// <summary>
		///     Tests if this filter and the given one would produce the same result
		///     for the same data.
		/// </summary>
		/// <param name="other"></param>
		/// <returns>True when there is no doubt that the two filters perform identical, false otherwise</returns>
		public bool IsEquivalent(QuickFilterSettings other)
		{
			if (ReferenceEquals(other, objB: null))
				return false;

			// We won't need to include the id because it doesn't have
			// any influence on the outcome of a filter operation.

			if (IgnoreCase != other.IgnoreCase)
				return false;

			if (IsInverted != other.IsInverted)
				return false;

			if (MatchType != other.MatchType)
				return false;

			if (!Equals(Value, other.Value))
				return false;

			return true;
		}

	/// <summary>
	///     Creates a new <see cref="ILogEntryFilter" /> which behaves just like this quick filter is
	///     configured. Any further changes to this object will NOT influence the returned filter:
	///     This method must be called and the newly returned filter be used instead.
	/// </summary>
	/// <returns></returns>
	[Pure]
	public ILogEntryFilter CreateFilter()
	{
		// When in highlight-only mode, we never want to invert the filter
		// because we need to match lines positively to highlight them.
		// The IsInverted flag only applies to "hide mode" filters.
		bool shouldInvert = IsHighlightOnly ? false : IsInverted;
		
		// For highlight filters, always use case-insensitive matching for better UX
		// (users expect "String" to match "string" when highlighting)
		bool shouldIgnoreCase = IsHighlightOnly ? true : IgnoreCase;
		
		return Filter.Create(Value, MatchType, shouldIgnoreCase, shouldInvert);
	}

	/// <inheritdoc />
	public void Serialize(IWriter writer)
	{
		writer.WriteAttribute("Id", Id);
		writer.WriteAttributeEnum("Type", MatchType);
		writer.WriteAttribute("Value", Value);
		writer.WriteAttribute("IgnoreCase", IgnoreCase);
		writer.WriteAttribute("IsInverted", IsInverted);
		writer.WriteAttribute("IsHighlightOnly", IsHighlightOnly);
		if (HighlightColor.HasValue)
			writer.WriteAttribute("HighlightColor", ColorToHex(HighlightColor.Value));
		if (ForegroundColor.HasValue)
			writer.WriteAttribute("ForegroundColor", ColorToHex(ForegroundColor.Value));
	}

	/// <inheritdoc />
	public void Deserialize(IReader reader)
	{
		reader.TryReadAttribute("Id", out Id);
		reader.TryReadAttributeEnum("Type", out MatchType);
		reader.TryReadAttribute("Value", out Value);
		reader.TryReadAttribute("IgnoreCase", out IgnoreCase);
		reader.TryReadAttribute("IsInverted", out IsInverted);
		reader.TryReadAttribute("IsHighlightOnly", out IsHighlightOnly);
		
		string highlightColorHex;
		if (reader.TryReadAttribute("HighlightColor", out highlightColorHex))
			HighlightColor = HexToColor(highlightColorHex);
		
		string foregroundColorHex;
		if (reader.TryReadAttribute("ForegroundColor", out foregroundColorHex))
			ForegroundColor = HexToColor(foregroundColorHex);
	}

	/// <summary>
	///     Converts a Color to a hex string format (#AARRGGBB).
	/// </summary>
	private static string ColorToHex(System.Windows.Media.Color color)
	{
		return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
	}

	/// <summary>
	///     Converts a hex string (#AARRGGBB or #RRGGBB) to a Color.
	///     Returns null if the string is invalid.
	/// </summary>
	private static System.Windows.Media.Color? HexToColor(string hex)
	{
		if (string.IsNullOrEmpty(hex))
			return null;

		try
		{
			hex = hex.TrimStart('#');
			
			if (hex.Length == 6)
			{
				// #RRGGBB format
				var r = Convert.ToByte(hex.Substring(0, 2), 16);
				var g = Convert.ToByte(hex.Substring(2, 2), 16);
				var b = Convert.ToByte(hex.Substring(4, 2), 16);
				return System.Windows.Media.Color.FromArgb(255, r, g, b);
			}
			else if (hex.Length == 8)
			{
				// #AARRGGBB format
				var a = Convert.ToByte(hex.Substring(0, 2), 16);
				var r = Convert.ToByte(hex.Substring(2, 2), 16);
				var g = Convert.ToByte(hex.Substring(4, 2), 16);
				var b = Convert.ToByte(hex.Substring(6, 2), 16);
				return System.Windows.Media.Color.FromArgb(a, r, g, b);
			}
		}
		catch (FormatException)
		{
			// Invalid hex string
		}
		catch (ArgumentException)
		{
			// Invalid substring indices
		}

		return null;
	}
}
}