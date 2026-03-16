using System.Windows.Media;
using Tailviewer.Api;

namespace Tailviewer.Ui.QuickFilter
{
	/// <summary>
	///     Represents a filter used for visual highlighting only (not for hiding lines).
	/// </summary>
	public sealed class HighlightFilter
	{
		/// <summary>
		///     The filter to apply for matching log lines.
		/// </summary>
		public ILogEntryFilter Filter { get; set; }

		/// <summary>
		///     The background color to use when highlighting matching lines.
		///     When null, a default highlight color should be used.
		/// </summary>
		public Color? HighlightColor { get; set; }

		/// <summary>
		///     The foreground (text) color to use when highlighting matching lines.
		///     When null, the default foreground color should be used.
		/// </summary>
		public Color? ForegroundColor { get; set; }
	}
}
