using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media;

namespace Tailviewer.Ui.LogView
{
	/// <summary>
	///     Builds RTF and HTML clipboard strings from a sequence of rendered <see cref="TextLine" />s,
	///     preserving foreground and background highlight colors.
	/// </summary>
	internal static class RichClipboardHelper
	{
		/// <summary>
		///     Represents a single colored text span within a line.
		/// </summary>
		public struct ColoredSpan
		{
			public string Text;
			public Color? Foreground;   // null = default (no explicit color)
			public Color? Background;   // null = no highlight
		}

		/// <summary>
		///     Represents one log line's worth of colored spans.
		/// </summary>
		public struct ColoredLine
		{
			public List<ColoredSpan> Spans;
		}

		/// <summary>
		///     Extracts colored span data from a list of TextLines.
		///     Must be called on the UI thread (accesses TextLine.Segments which triggers WPF FormattedText).
		/// </summary>
		public static List<ColoredLine> ExtractLines(IEnumerable<TextLine> lines)
		{
			var result = new List<ColoredLine>();
			foreach (var line in lines)
			{
				var spans = new List<ColoredSpan>();
				var segments = line.Segments; // triggers CreateTextIfNecessary()
			foreach (var seg in segments)
			{
				var span = new ColoredSpan { Text = seg.Text };

				// Foreground: extract from ForegroundBrush field
				if (seg.ForegroundBrush is SolidColorBrush fg)
					span.Foreground = fg.Color;

				// Background: extract from BackgroundBrush field
				if (seg.BackgroundBrush is SolidColorBrush bg)
					span.Background = bg.Color;

				spans.Add(span);
			}
				// If no segments, fall back to raw content
				if (spans.Count == 0)
					spans.Add(new ColoredSpan { Text = line.LogEntry.RawContent ?? string.Empty });
				result.Add(new ColoredLine { Spans = spans });
			}
			return result;
		}

		/// <summary>
		///     Builds an RTF string from the given colored lines.
		/// </summary>
		/// <remarks>
		///     Uses <c>\chshdng0\chcbpatN\cbN</c> for background colors instead of <c>\highlightN</c>.
		///     Word does not support full RGB colors via <c>\highlight</c> and approximates them to a
		///     limited palette; <c>\chshdng0\chcbpat</c> is the Word-compatible workaround documented
		///     in RTF Spec 1.9.1 and confirmed by the Microsoft Terminal team (PR #16035).
		///     <c>\cbN</c> is appended for compatibility with other RTF readers that follow the spec.
		/// </remarks>
		public static string BuildRtf(List<ColoredLine> lines)
		{
			// Collect unique colors for the color table
			var colors = new List<Color>();
			foreach (var line in lines)
				foreach (var span in line.Spans)
				{
					if (span.Foreground.HasValue && !colors.Contains(span.Foreground.Value))
						colors.Add(span.Foreground.Value);
					if (span.Background.HasValue && !colors.Contains(span.Background.Value))
						colors.Add(span.Background.Value);
				}

			var sb = new StringBuilder();
			sb.Append(@"{\rtf1\ansi\deff0");

			// Font table — required for Word to honour the font selection
			sb.Append(@"{\fonttbl{\f0\fnil\fcharset0 Courier New;}}");

			// Color table (1-indexed; empty leading entry = auto/default color)
			sb.Append(@"{\colortbl;");
			foreach (var c in colors)
				sb.AppendFormat(@"\red{0}\green{1}\blue{2};", c.R, c.G, c.B);
			sb.Append('}');

			sb.Append(@"\f0\fs20 ");

			for (int i = 0; i < lines.Count; i++)
			{
				foreach (var span in lines[i].Spans)
				{
					bool hasFg = span.Foreground.HasValue;
					bool hasBg = span.Background.HasValue;

					if (hasFg || hasBg)
					{
						sb.Append('{');
						if (hasFg)
							sb.AppendFormat(@"\cf{0} ", colors.IndexOf(span.Foreground.Value) + 1);
						if (hasBg)
						{
							int bgIdx = colors.IndexOf(span.Background.Value) + 1;
							// \chshdng0\chcbpatN = Word-compatible background color
							// \cbN               = standard RTF background color (other readers)
							sb.AppendFormat(@"\chshdng0\chcbpat{0}\cb{0} ", bgIdx);
						}
						sb.Append(EscapeRtf(span.Text));
						sb.Append('}');
					}
					else
					{
						sb.Append(EscapeRtf(span.Text));
					}
				}
				if (i < lines.Count - 1)
					sb.Append(@"\par ");
			}

			sb.Append('}');
			return sb.ToString();
		}

		/// <summary>
		///     Returns the RTF content as a null-terminated ANSI (Windows-1252) byte stream wrapped in a
		///     <see cref="MemoryStream" />, suitable for <c>DataObject.SetData(DataFormats.Rtf, stream)</c>.
		/// </summary>
		/// <remarks>
		///     WPF's <c>DataObject.SetData(DataFormats.Rtf, string)</c> has a known encoding bug: it puts the
		///     string on the clipboard as UTF-16, which Word's RTF parser cannot read (RTF must be 7-bit ASCII /
		///     ANSI).  Passing the data as a raw <see cref="MemoryStream" /> bypasses WPF's conversion and
		///     lets Windows hand the bytes directly to Word.
		/// </remarks>
		public static MemoryStream BuildRtfStream(List<ColoredLine> lines)
		{
			string rtf = BuildRtf(lines);
			// RTF must be ANSI (Windows-1252). Use Encoding.Default on Windows, which is the system ANSI
			// code page (1252 for en-US / western locales). Non-ASCII chars are already \u-escaped by
			// EscapeRtf, so this is safe even on systems with a different ANSI code page.
			byte[] bytes = Encoding.Default.GetBytes(rtf);
			// Null-terminate so that Win32 RTF readers see a proper C-string boundary.
			var ms = new MemoryStream(bytes.Length + 1);
			ms.Write(bytes, 0, bytes.Length);
			ms.WriteByte(0);
			ms.Position = 0;
			return ms;
		}

		/// <summary>
		///     Builds an HTML clipboard format string (with required header offsets) from the given colored lines.
		/// </summary>
		public static string BuildHtml(List<ColoredLine> lines)
		{
			// Build the HTML fragment body
			var fragment = new StringBuilder();
			for (int i = 0; i < lines.Count; i++)
			{
				foreach (var span in lines[i].Spans)
				{
					bool hasFg = span.Foreground.HasValue;
					bool hasBg = span.Background.HasValue;
					if (hasFg || hasBg)
					{
						fragment.Append("<span style=\"");
						if (hasFg)
							fragment.AppendFormat("color:{0};", ColorToHex(span.Foreground.Value));
						if (hasBg)
							fragment.AppendFormat("background-color:{0};", ColorToHex(span.Background.Value));
						fragment.Append("\">");
						fragment.Append(HtmlEncode(span.Text));
						fragment.Append("</span>");
					}
					else
					{
						fragment.Append(HtmlEncode(span.Text));
					}
				}
				if (i < lines.Count - 1)
					fragment.Append("<br/>\r\n");
			}

			string fragmentStr = fragment.ToString();

			string htmlDoc =
				"<!DOCTYPE html>\r\n<html>\r\n<body>\r\n" +
				"<!--StartFragment-->" + fragmentStr + "<!--EndFragment-->\r\n" +
				"</body>\r\n</html>";

			// Header template (fixed width fields)
			const string headerTemplate =
				"Version:0.9\r\n" +
				"StartHTML:0000000000\r\n" +
				"EndHTML:0000000000\r\n" +
				"StartFragment:0000000000\r\n" +
				"EndFragment:0000000000\r\n";

			int headerLen = Encoding.UTF8.GetByteCount(headerTemplate);
			int startHtml = headerLen;
			int endHtml = headerLen + Encoding.UTF8.GetByteCount(htmlDoc);

			string beforeFragment = "<!DOCTYPE html>\r\n<html>\r\n<body>\r\n<!--StartFragment-->";
			int startFragment = headerLen + Encoding.UTF8.GetByteCount(beforeFragment);
			int endFragment = startFragment + Encoding.UTF8.GetByteCount(fragmentStr);

			string header =
				"Version:0.9\r\n" +
				$"StartHTML:{startHtml:D10}\r\n" +
				$"EndHTML:{endHtml:D10}\r\n" +
				$"StartFragment:{startFragment:D10}\r\n" +
				$"EndFragment:{endFragment:D10}\r\n";

			return header + htmlDoc;
		}

		private static string EscapeRtf(string text)
		{
			if (string.IsNullOrEmpty(text))
				return string.Empty;

			var sb = new StringBuilder(text.Length);
			foreach (char c in text)
			{
				switch (c)
				{
					case '\\': sb.Append(@"\\"); break;
					case '{':  sb.Append(@"\{"); break;
					case '}':  sb.Append(@"\}"); break;
					case '\n': sb.Append(@"\par "); break;
					case '\r': break;
					case '\t': sb.Append(@"\tab "); break;
					default:
						if (c <= 0x7F)
							sb.Append(c);
						else
							sb.AppendFormat(@"\u{0}?", (short)c);
						break;
				}
			}
			return sb.ToString();
		}

		private static string HtmlEncode(string text)
		{
			if (string.IsNullOrEmpty(text))
				return string.Empty;

			return text
				.Replace("&", "&amp;")
				.Replace("<", "&lt;")
				.Replace(">", "&gt;")
				.Replace("\"", "&quot;")
				.Replace("\r\n", "<br/>\r\n")
				.Replace("\n", "<br/>\r\n");
		}

		private static string ColorToHex(Color c)
		{
			return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
		}
	}
}
