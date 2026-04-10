using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Tailviewer.Ui.Converters
{
	/// <summary>
	/// Converts a TimeSpan to a human-readable string, with optional unit filtering.
	/// </summary>
	public sealed class TimeSpanConverter : IValueConverter
	{
		public TimeSpanConverter()
		{
		}

		/// <summary>
		/// Whether to exclude milliseconds from the output.
		/// </summary>
		public bool IgnoreMilliseconds { get; set; }

		/// <summary>
		/// Whether to exclude seconds from the output.
		/// </summary>
		public bool IgnoreSeconds { get; set; }

		/// <summary>
		/// Whether to exclude minutes from the output.
		/// </summary>
		public bool IgnoreMinutes { get; set; }

		/// <summary>
		/// Whether to exclude hours from the output.
		/// </summary>
		public bool IgnoreHours { get; set; }

		/// <summary>
		/// Whether to exclude days from the output.
		/// </summary>
		public bool IgnoreDays { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return null;

			if (!(value is TimeSpan timeSpan))
				return value;

			var parts = new List<string>();

			if (timeSpan.Days > 0 && !IgnoreDays)
			{
				parts.Add(timeSpan.Days == 1 ? "1 day" : $"{timeSpan.Days} days");
			}

			if (timeSpan.Hours > 0 && !IgnoreHours)
			{
				parts.Add(timeSpan.Hours == 1 ? "1 hour" : $"{timeSpan.Hours} hours");
			}

			if (timeSpan.Minutes > 0 && !IgnoreMinutes)
			{
				parts.Add(timeSpan.Minutes == 1 ? "1 minute" : $"{timeSpan.Minutes} minutes");
			}

			if (timeSpan.Seconds > 0 && !IgnoreSeconds)
			{
				parts.Add(timeSpan.Seconds == 1 ? "1 second" : $"{timeSpan.Seconds} seconds");
			}

			if (timeSpan.Milliseconds > 0 && !IgnoreMilliseconds)
			{
				parts.Add(timeSpan.Milliseconds == 1 ? "1 millisecond" : $"{timeSpan.Milliseconds} milliseconds");
			}

			// If all units are zero or ignored, show "0 seconds"
			if (parts.Count == 0)
			{
				if (!IgnoreSeconds)
					return "0 seconds";
				if (!IgnoreMilliseconds)
					return "0 milliseconds";
				return "0";
			}

			// Join with commas and "and" for the last item
			if (parts.Count == 1)
				return parts[0];

			if (parts.Count == 2)
				return $"{parts[0]} and {parts[1]}";

			var allButLast = string.Join(", ", parts.Take(parts.Count - 1));
			return $"{allButLast}, and {parts[parts.Count - 1]}";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
