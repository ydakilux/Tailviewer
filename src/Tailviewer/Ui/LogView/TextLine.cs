using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Tailviewer.Api;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Settings;
using Tailviewer.Ui.QuickFilter;

namespace Tailviewer.Ui.LogView
{
	/// <summary>
	///     Is responsible for representing the contents of a <see cref="LogEntry" /> as a <see cref="FormattedText" />,
	///     ready to be rendered.
	/// </summary>
public sealed class TextLine
{
	private readonly HashSet<LogLineIndex> _hoveredIndices;
	private readonly IReadOnlyLogEntry _logEntry;
	private readonly List<TextSegment> _segments;
	private readonly TextSettings _textSettings;
	private readonly TextBrushes _textBrushes;
	private readonly HashSet<LogLineIndex> _selectedIndices;
	private ISearchResults _searchResults;
	private List<HighlightFilter> _highlightFilters;
	private Brush _lastForegroundBrush;
	private bool _colorByLevel;
	private bool _isFocused;

		public TextLine(IReadOnlyLogEntry logEntry,
		                HashSet<LogLineIndex> hoveredIndices,
		                HashSet<LogLineIndex> selectedIndices,
		                bool colorByLevel)
			: this(logEntry, hoveredIndices, selectedIndices, colorByLevel, TextSettings.Default,
			       new TextBrushes(null))
		{}

		public TextLine(IReadOnlyLogEntry logEntry,
		                HashSet<LogLineIndex> hoveredIndices,
		                HashSet<LogLineIndex> selectedIndices,
		                bool colorByLevel,
		                TextSettings textSettings,
		                TextBrushes textBrushes)
		{
			if (logEntry == null) throw new ArgumentNullException(nameof(logEntry));
			if (hoveredIndices == null) throw new ArgumentNullException(nameof(hoveredIndices));
			if (selectedIndices == null) throw new ArgumentNullException(nameof(selectedIndices));
			if (textBrushes == null) throw new ArgumentNullException(nameof(textBrushes));

			_logEntry = logEntry;
			_hoveredIndices = hoveredIndices;
			_selectedIndices = selectedIndices;
			_segments = new List<TextSegment>();
			_colorByLevel = colorByLevel;
			_textSettings = textSettings;
			_textBrushes = textBrushes;
			_isFocused = true;
		}

		public bool IsFocused
		{
			get { return _isFocused; }
			set
			{
				if (value == _isFocused)
					return;

				_isFocused = value;
				_segments.Clear();
			}
		}

		public IReadOnlyLogEntry LogEntry => _logEntry;

		public bool ColorByLevel
		{
			get { return _colorByLevel; }
			set
			{
				if (value == _colorByLevel)
					return;

				_colorByLevel = value;
				_segments.Clear();
			}
		}

		public bool IsHovered => _hoveredIndices.Contains(_logEntry.Index);

		public Brush ForegroundBrush
		{
			get
			{
				return _textBrushes.ForegroundBrush(IsSelected, IsFocused, ColorByLevel, _logEntry.LogLevel);
			}
		}

		public Brush BackgroundBrush
		{
			get
			{
				return _textBrushes.BackgroundBrush(IsSelected, IsFocused, ColorByLevel, _logEntry.LogLevel, (int) _logEntry.LogEntryIndex);
			}
		}

		public bool IsSelected => _selectedIndices.Contains(_logEntry.Index);

		public IReadOnlyList<TextSegment> Segments
		{
			get
			{
				CreateTextIfNecessary();
				return _segments;
			}
		}

	public ISearchResults SearchResults
	{
		set
		{
			_searchResults = value;
			_segments.Clear();
		}
	}

	public List<HighlightFilter> HighlightFilters
	{
		get { return _highlightFilters; }
		set
		{
			if (Equals(value, _highlightFilters))
				return;

			_highlightFilters = value;
			_segments.Clear();
		}
	}

	private void CreateTextIfNecessary()
		{
			Brush regularForegroundBrush = ForegroundBrush;
			if (_segments.Count == 0 || _lastForegroundBrush != regularForegroundBrush)
			{
				_segments.Clear();

				Brush highlightedBrush = TextBrushes.HighlightedForegroundBrush;
				var searchResults = _searchResults;
				var unformattedMessage = _logEntry.RawContent ?? string.Empty;

			// Collect all filter matches first
			var allFilterMatches = new List<(int Index, int Count, Brush ForegroundBrush, Brush BackgroundBrush)>();
			if (_highlightFilters != null && _highlightFilters.Count > 0)
			{
				foreach (var highlightFilter in _highlightFilters)
				{
					if (highlightFilter?.Filter == null)
						continue;

					try
					{
						if (highlightFilter.Filter.PassesFilter(_logEntry))
						{
							var filterMatches = new List<LogLineMatch>();
							highlightFilter.Filter.Match(_logEntry, filterMatches);
							
							if (filterMatches.Count > 0)
							{
								log4net.LogManager.GetLogger(typeof(TextLine)).InfoFormat(
									"[RENDER] Line {0} matched filter, {1} matches, Color={2}",
									_logEntry.Index, filterMatches.Count, highlightFilter.HighlightColor);
							}
							
							foreach (var match in filterMatches)
							{
								var backgroundBrush = highlightFilter.HighlightColor.HasValue
									? TextBrushes.CreateBrush(highlightFilter.HighlightColor.Value)
									: TextBrushes.HighlightedBackgroundBrush;
								var foregroundBrush = highlightFilter.ForegroundColor.HasValue
									? TextBrushes.CreateBrush(highlightFilter.ForegroundColor.Value)
									: regularForegroundBrush;

								allFilterMatches.Add((match.Index, match.Count, foregroundBrush, backgroundBrush));
							}
						}
					}
					catch
					{
						// Ignore filter errors
					}
				}
			}

			if (searchResults != null)
			{
				try
				{
					// The search results are based on the unformatted message (e.g. before we replace tabs by the appropriate
					// amount of spaces). Therefore we'll have to subdivide the message into chunks based on the search results
					// and then reformat the individual chunks.
					// Merge search matches with filter highlight matches so both are shown simultaneously.
					var allMatches = new List<(int Index, int Count, Brush ForegroundBrush, Brush BackgroundBrush, bool IsSearch)>();
					foreach (LogLineMatch match in searchResults.MatchesByLine[_logEntry.Index])
						allMatches.Add((match.Index, match.Count, highlightedBrush, null, true));
					foreach (var fm in allFilterMatches)
						allMatches.Add((fm.Index, fm.Count, fm.ForegroundBrush, fm.BackgroundBrush, false));
					allMatches.Sort((a, b) => a.Index != b.Index ? a.Index.CompareTo(b.Index) : (a.IsSearch ? 0 : 1));

					string substring;
					int lastIndex = 0;
					foreach (var match in allMatches)
					{
						if (match.Index < lastIndex) continue; // skip overlapping
						if (match.Index > lastIndex)
						{
							substring = unformattedMessage.Substring(lastIndex, match.Index - lastIndex);
							AddSegmentsFrom(FormatMessage(substring), regularForegroundBrush, isRegular: true);
						}
						var count = Math.Min(match.Count, unformattedMessage.Length - match.Index);
						if (count <= 0) continue;
						substring = unformattedMessage.Substring(match.Index, count);
						if (match.BackgroundBrush != null)
							AddSegmentsFrom(FormatMessage(substring), match.ForegroundBrush, match.BackgroundBrush, isRegular: false);
						else
							AddSegmentsFrom(FormatMessage(substring), match.ForegroundBrush, isRegular: false);
						lastIndex = match.Index + count;
					}

				if (lastIndex <= unformattedMessage.Length - 1)
				{
					substring = unformattedMessage.Substring(lastIndex);
					AddSegmentsFrom(FormatMessage(substring), regularForegroundBrush, isRegular: true);
				}
			}
			catch (Exception)
			{
				_segments.Clear();

				string message = FormatMessage(_logEntry.RawContent);
				AddSegmentsFrom(message, regularForegroundBrush, isRegular: true);
			}
		}
		else if (allFilterMatches.Count > 0)
			{
				// No search results, but we have filter matches - subdivide based on filters
				try
				{
					// Sort matches by index to process them in order
					allFilterMatches.Sort((a, b) => a.Index.CompareTo(b.Index));
					
					string substring;
					int lastIndex = 0;
					foreach (var match in allFilterMatches)
					{
						if (match.Index > lastIndex)
						{
							substring = unformattedMessage.Substring(lastIndex, match.Index - lastIndex);
							AddSegmentsFrom(FormatMessage(substring), regularForegroundBrush, isRegular: true);
						}

						if (match.Index < unformattedMessage.Length)
						{
							var actualCount = Math.Min(match.Count, unformattedMessage.Length - match.Index);
							substring = unformattedMessage.Substring(match.Index, actualCount);
							AddSegmentsFrom(FormatMessage(substring), match.ForegroundBrush, match.BackgroundBrush, isRegular: false);
							lastIndex = match.Index + actualCount;
						}
					}

					if (lastIndex <= unformattedMessage.Length - 1)
					{
						substring = unformattedMessage.Substring(lastIndex);
						AddSegmentsFrom(FormatMessage(substring), regularForegroundBrush, isRegular: true);
					}
				}
				catch (Exception)
				{
					_segments.Clear();

					string message = FormatMessage(_logEntry.RawContent);
					AddSegmentsFrom(message, regularForegroundBrush, isRegular: true);
				}
			}
			else
			{
				string message = FormatMessage(_logEntry.RawContent);
				AddSegmentsFrom(message, regularForegroundBrush, isRegular: true);
			}
		_lastForegroundBrush = regularForegroundBrush;
	}
}

	[Pure]
	private string FormatMessage(string logLineMessage)
	{
		var builder = new StringBuilder(logLineMessage ?? string.Empty);
		ReplaceTabsWithSpaces(builder, _textSettings.TabWidth);
		return builder.ToString();
	}

	private void AddSegmentsFrom(string message, Brush brush, bool isRegular)
	{
		const int maxCharactersPerSegment = 512;
		int segmentCount = (int) Math.Ceiling(1.0 * message.Length / maxCharactersPerSegment);
		for (int i = 0; i < segmentCount; ++i)
		{
			var start = i * maxCharactersPerSegment;
			var remaining = message.Length - start;
			var length = Math.Min(remaining, maxCharactersPerSegment);
			var segment = message.Substring(i * maxCharactersPerSegment, length);
			_segments.Add(new TextSegment(segment, brush, isRegular, _textSettings));
		}
	}

	private void AddSegmentsFrom(string message, Brush foregroundBrush, Brush backgroundBrush, bool isRegular)
	{
		const int maxCharactersPerSegment = 512;
		int segmentCount = (int) Math.Ceiling(1.0 * message.Length / maxCharactersPerSegment);
		for (int i = 0; i < segmentCount; ++i)
		{
			var start = i * maxCharactersPerSegment;
			var remaining = message.Length - start;
			var length = Math.Min(remaining, maxCharactersPerSegment);
			var segment = message.Substring(i * maxCharactersPerSegment, length);
			_segments.Add(new TextSegment(segment, foregroundBrush, backgroundBrush, isRegular, _textSettings));
		}
	}

	private void AddSegmentsFrom(string message, int index, int count, Brush foregroundBrush, Brush backgroundBrush, bool isRegular)
	{
		if (index < 0 || index >= message.Length || count <= 0)
			return;

		var actualCount = Math.Min(count, message.Length - index);
		var substring = message.Substring(index, actualCount);
		var formattedSubstring = FormatMessage(substring);

		const int maxCharactersPerSegment = 512;
		int segmentCount = (int) Math.Ceiling(1.0 * formattedSubstring.Length / maxCharactersPerSegment);
		for (int i = 0; i < segmentCount; ++i)
		{
			var start = i * maxCharactersPerSegment;
			var remaining = formattedSubstring.Length - start;
			var length = Math.Min(remaining, maxCharactersPerSegment);
			var segment = formattedSubstring.Substring(i * maxCharactersPerSegment, length);
			_segments.Add(new TextSegment(segment, foregroundBrush, backgroundBrush, isRegular, _textSettings));
		}
	}

	public void Render(DrawingContext drawingContext,
		                   double xOffset,
		                   double y,
		                   double actualWidth,
		                   bool colorByLevel)
		{
			CreateTextIfNecessary();

			int coloredCount = 0;
			for (int k = 0; k < _segments.Count; ++k)
				if (_segments[k].BackgroundBrush != null)
					coloredCount++;
			if (coloredCount > 0 || (_highlightFilters != null && _highlightFilters.Count > 0))
			{
				log4net.LogManager.GetLogger(typeof(TextLine)).InfoFormat(
					"[RENDER3] After CreateTextIfNecessary: totalSegments={0} coloredSegments={1} highlightFilters={2} line={3}",
					_segments.Count, coloredCount, _highlightFilters?.Count ?? 0, _logEntry.Index);
			}

			Brush regularBackgroundBrush = BackgroundBrush;

			double x = xOffset;

		for (int i = 0; i < _segments.Count; ++i)
		{
			TextSegment segment = _segments[i];
			if (segment.BackgroundBrush != null)
			{
				log4net.LogManager.GetLogger(typeof(TextLine)).InfoFormat(
					"[RENDER2] Segment[{0}] '{1}' BackgroundBrush={2} x={3} visible={4} actualWidth={5}",
					i,
					segment.Text?.Length > 20 ? segment.Text.Substring(0, 20) : segment.Text,
					segment.BackgroundBrush,
					x, IsVisible(actualWidth, x, segment.Width), actualWidth);
			}
			if (IsVisible(actualWidth, x, segment.Width))
			{
				Brush brush;
				// Use custom background brush if available
				if (segment.BackgroundBrush != null)
				{
					brush = segment.BackgroundBrush;
				}
				else if (segment.IsRegular)
				{
					brush = regularBackgroundBrush;
				}
				else
				{
					brush = IsSelected
						? TextBrushes.HighlightedSelectedBackgroundBrush
						: TextBrushes.HighlightedBackgroundBrush;
				}

				if (brush != null)
				{
					var rect = new Rect(x, y,
					                    segment.Width,
					                    _textSettings.LineHeight);
					drawingContext.DrawRectangle(brush, null, rect);
				}

					var topLeft = new Point(x, y);
					drawingContext.DrawText(segment.FormattedText, topLeft);
				}
				x += segment.Width;
			}

			if (x < actualWidth && regularBackgroundBrush != null)
			{
				var rect = new Rect(x, y,
				                    actualWidth - x,
				                    _textSettings.LineHeight);
				drawingContext.DrawRectangle(regularBackgroundBrush, null, rect);
			}
		}

		[Pure]
		public static bool IsVisible(double actualWidth, double x, double segmentWidth)
		{
			const int visibleXMin = 0;
			var visibleXMax = actualWidth;

			var xMin = x;
			var xMax = xMin + segmentWidth;

			var isVisible = !(xMax < visibleXMin || xMin > visibleXMax);
			return isVisible;
		}

		public static void ReplaceTabsWithSpaces(StringBuilder builder, int tabWidth)
		{
			for (int i = 0; i < builder.Length;)
			{
				if (builder[i] == '\t')
				{
					var already = i % tabWidth;
					var remaining = tabWidth - already;
					builder.Remove(i, 1);
					builder.Insert(i, " ", remaining);

					i += remaining;
				}
				else
				{
					++i;
				}
			}
		}
	}
}