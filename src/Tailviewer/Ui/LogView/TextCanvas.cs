// Ignore Spelling: Tailviewer Indices

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using log4net;
using Metrolib;
using Metrolib.Controls;
using Tailviewer.Api;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Core;
using Tailviewer.Settings;
using Tailviewer.Ui.QuickFilter;
using Properties = Tailviewer.Core.Properties;

namespace Tailviewer.Ui.LogView
{
	/// <summary>
	///     Responsible for drawing the individual <see cref="IReadOnlyLogEntry" />s of the <see cref="ILogSource" />.
	/// </summary>
	public sealed class TextCanvas
		: FrameworkElement
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ScrollBar _horizontalScrollBar;
		private readonly HashSet<LogLineIndex> _hoveredIndices;
		private readonly HashSet<LogLineIndex> _selectedIndices;
		private readonly ScrollBar _verticalScrollBar;
		private TextSettings _textSettings;
		private TextBrushes _textBrushes;
	private readonly List<TextLine> _visibleTextLines;
	private readonly LogBufferList _visibleBufferBuffer;
	private readonly DispatchedSearchResults _searchResults;
	private List<HighlightFilter> _highlightFilters;
	private readonly DispatcherTimer _timer;

		private int _currentLine;
		private LogSourceSection _currentlyVisibleSection;
		private LogLineIndex _firstSelection;
		private LogLineIndex _lastSelection;
		private ILogSource _logSource;
		private double _xOffset;
		private double _yOffset;
		private bool _colorByLevel;
		private ILogSourceSearch _search;
		private int _selectedSearchResultIndex;
		private bool _requiresFurtherUpdate;
		private DateTime _updateStart;

		public TextCanvas(ScrollBar horizontalScrollBar, ScrollBar verticalScrollBar, TextSettings textSettings)
		{
			_horizontalScrollBar = horizontalScrollBar;
			_horizontalScrollBar.ValueChanged += HorizontalScrollBarOnScroll;

			_verticalScrollBar = verticalScrollBar;
			_textSettings = textSettings;
			_verticalScrollBar.ValueChanged += VerticalScrollBarOnValueChanged;

			_selectedIndices = new HashSet<LogLineIndex>();
			_hoveredIndices = new HashSet<LogLineIndex>();
			_visibleTextLines = new List<TextLine>();
			_visibleBufferBuffer = new LogBufferList(Columns.Index, Columns.LogEntryIndex, PageBufferedLogSource.RetrievalState, Columns.LogLevel, Columns.RawContent);
			_searchResults = new DispatchedSearchResults();
			_timer = new DispatcherTimer();
			_timer.Tick += OnUpdate;
			_timer.Interval = TimeSpan.FromMilliseconds(1000.0/60);

			InputBindings.Add(new KeyBinding(new DelegateCommand(OnCopyToClipboard), Key.C, ModifierKeys.Control));
			InputBindings.Add(new KeyBinding(new DelegateCommand(OnSelectUp), Key.Up, ModifierKeys.Shift));
			InputBindings.Add(new KeyBinding(new DelegateCommand(OnSelectDown), Key.Down, ModifierKeys.Shift));
			InputBindings.Add(new KeyBinding(new DelegateCommand(OnMoveDown), Key.Down, ModifierKeys.None));
			InputBindings.Add(new KeyBinding(new DelegateCommand(OnMoveUp), Key.Up, ModifierKeys.None));
			InputBindings.Add(new KeyBinding(new DelegateCommand(OnMovePageDown), Key.PageDown, ModifierKeys.None));
			InputBindings.Add(new KeyBinding(new DelegateCommand(OnMovePageUp), Key.PageUp, ModifierKeys.None));
			InputBindings.Add(new KeyBinding(new DelegateCommand(OnMoveStart), Key.Home, ModifierKeys.Control));
			InputBindings.Add(new KeyBinding(new DelegateCommand(OnMoveEnd), Key.End, ModifierKeys.Control));
			InputBindings.Add(new MouseBinding(new DelegateCommand(OnMouseWheelUp), MouseWheelGesture.WheelUp));
			InputBindings.Add(new MouseBinding(new DelegateCommand(OnMouseWheelDown), MouseWheelGesture.WheelDown));

			SizeChanged += OnSizeChanged;
			GotFocus += OnGotFocus;
			LostFocus += OnLostFocus;
			Loaded += OnLoaded;
			Unloaded += OnUnloaded;

			Focusable = true;
			ClipToBounds = true;
			FocusVisualStyle = null;
		}

		public bool RequiresFurtherUpdate
		{
			get { return _requiresFurtherUpdate; }
		}

		public LogSourceSection CurrentlyVisibleSection
		{
			get { return _currentlyVisibleSection; }
			set
			{
				if (value == _currentlyVisibleSection)
					return;

				_currentlyVisibleSection = value;
				Action<LogSourceSection> fn = VisibleSectionChanged;
				fn?.Invoke(value);
			}
		}

		public ILogSource LogSource
		{
			get { return _logSource; }
			set
			{
				_logSource = value;
				_visibleTextLines.Clear();

				_currentLine = 0;
				_lastSelection = 0;
			}
		}

		public int CurrentLine
		{
			get { return _currentLine; }
			set { _currentLine = value; }
		}

		public List<TextLine> VisibleTextLines => _visibleTextLines;

		public IEnumerable<LogLineIndex> SelectedIndices
		{
			get { return _selectedIndices; }
			set
			{
				if (_selectedIndices.HasEqualContent(value))
					return;

				_selectedIndices.Clear();
				if (value != null)
				{
					_selectedIndices.AddRange(value);
					if (_selectedIndices.Any())
					{
						_firstSelection = _selectedIndices.Min();
						_lastSelection = _selectedIndices.Max();
					}
					else
					{
						_firstSelection = _lastSelection = LogLineIndex.Invalid;
					}
				}
				else
				{
					_firstSelection = _lastSelection = LogLineIndex.Invalid;
				}

				UpdateVisibleLines();
			}
		}

		private LogMatch? SelectedSearchResult
		{
			get
			{
				var index = _selectedSearchResultIndex;
				if (index >= 0 && index < _searchResults.Matches.Count)
					return _searchResults.Matches[index];

				return null;
			}
		}

		public double YOffset
		{
			get { return _yOffset; }
		}

		public bool ColorByLevel
		{
			get { return _colorByLevel; }
			set
			{
				if (value == _colorByLevel)
					return;

				_colorByLevel = value;
				UpdateVisibleLines();
			}
		}

	public ILogSourceSearch Search
	{
		get { return _search; }
		set
		{
			_search?.RemoveListener(_searchResults);
			_search = value;
			if (_search != null)
				Search.AddListener(_searchResults);
		}
	}

	public List<HighlightFilter> HighlightFilters
	{
		get { return _highlightFilters; }
		set
		{
			_highlightFilters = value;
			UpdateVisibleLines();
		}
	}

		public int SelectedSearchResultIndex
		{
			get { return _selectedSearchResultIndex; }
			set
			{
				if (value == _selectedSearchResultIndex)
					return;

				_selectedSearchResultIndex = value;
				var result = SelectedSearchResult;
				if (result != null)
				{
					var index = result.Value.Index;
					RequestBringIntoView(index, result.Value.Match);
					SetSelected(index, SelectMode.Replace);
					InvalidateVisual();
				}
			}
		}

		public void ChangeTextSettings(TextSettings textSettings, TextBrushes textBrushes)
		{
			_textSettings = textSettings;
			_textBrushes = textBrushes;
		}

		public event Action<LogSourceSection> VisibleSectionChanged;

		public void UpdateVisibleSection()
		{
			_currentlyVisibleSection = CalculateVisibleSection();
		}

		private void OnGotFocus(object sender, RoutedEventArgs routedEventArgs)
		{
			UpdateVisibleLines();
		}

		private void OnLostFocus(object sender, RoutedEventArgs routedEventArgs)
		{
			UpdateVisibleLines();
		}

		private void OnUpdate(object sender, EventArgs e)
		{
			var result = SelectedSearchResult;
			if (_searchResults.Update())
			{
				var currentResult = SelectedSearchResult;
				if (!Equals(result, currentResult) && currentResult != null)
				{
					ChangeSelectionAndBringIntoView(currentResult.Value.Index);
				}

				// The search has been modified and we need to
				// check which lines have a match in them...
				UpdateVisibleLines();
			}
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			_timer.Start();
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			_timer.Stop();
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			var rect = new Rect(0, 0, ActualWidth, ActualHeight);
			drawingContext.DrawRectangle(Brushes.White, null, rect);

			double x = _xOffset;
			double y = _yOffset;
			foreach (TextLine textLine in _visibleTextLines)
			{
				textLine.Render(drawingContext, x, y, ActualWidth, ColorByLevel);
				y += _textSettings.LineHeight;
			}
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
		{
			OnSizeChanged();
		}

		public void OnSizeChanged()
		{
			DetermineVerticalOffset();
			_currentlyVisibleSection = CalculateVisibleSection();
			UpdateVisibleLines();
		}

		public void DetermineVerticalOffset()
		{
			double value = _verticalScrollBar.Value;
			var lineBeginning = (int) (Math.Floor(value/_textSettings.LineHeight)*_textSettings.LineHeight);
			_yOffset = lineBeginning - value;
		}

		public void UpdateVisibleLines()
		{
			_visibleTextLines.Clear();
			if (_logSource == null)
				return;

			try
			{
				_visibleBufferBuffer.Clear();
				_visibleBufferBuffer.Resize(_currentlyVisibleSection.Count);
				if (_currentlyVisibleSection.Count > 0)
				{
					// We don't want to block the UI thread for very long at all so we instruct the log source to only
					// fetch data from the cache, but to fetch missing data for later (from the source).
					var queryOptions = new LogSourceQueryOptions(LogSourceQueryMode.FromCache | LogSourceQueryMode.FetchForLater, TimeSpan.Zero);
					_logSource.GetEntries(_currentlyVisibleSection, _visibleBufferBuffer, 0, queryOptions);

					// Now comes the fun part. We need to detect if we could fetch *all* the data.
					// This is done by inspecting the BufferedLogSource.RetrievalState - if we encounter any NotCached value,
					// then the entry is part of the source, but was not cached at the time of trying to access it.
					// If that's the case, we will instruct this canvas to re-fetch the once more in a bit. This loop will terminate once the
					// cache has managed to fetch the desired data which should happen some time...
					if (_visibleBufferBuffer.ContainsAny(PageBufferedLogSource.RetrievalState,
					                                     RetrievalState.NotCached,
					                                     new Int32Range(offset: 0, _currentlyVisibleSection.Count)))
					{
						if (!_requiresFurtherUpdate)
						{
							Log.DebugFormat("Requires further update (at least one entry is not in cache)");
							_requiresFurtherUpdate = true;
							_updateStart = DateTime.Now;
						}
					}
					else
					{
						if (_requiresFurtherUpdate)
						{
							var elapsed = DateTime.Now - _updateStart;
							Log.DebugFormat("No longer requires further update (all retrieved log entries are in cache), took {0:F1}ms", elapsed.TotalMilliseconds); 
							_requiresFurtherUpdate = false;
						}
					}

				for (int i = 0; i < _currentlyVisibleSection.Count; ++i)
				{
					var line = new TextLine(_visibleBufferBuffer[i], _hoveredIndices, _selectedIndices,
					                        _colorByLevel, _textSettings, _textBrushes)
					{
						IsFocused = IsFocused,
						SearchResults = _searchResults,
						HighlightFilters = _highlightFilters
					};
					_visibleTextLines.Add(line);
				}
				}

				Action fn = VisibleLinesChanged;
				fn?.Invoke();

				InvalidateVisual();
			}
			catch (ArgumentOutOfRangeException e)
			{
				Log.DebugFormat("Caught exception while trying to update text: {0}", e);
			}
			catch (IndexOutOfRangeException e)
			{
				Log.DebugFormat("Caught exception while trying to update text: {0}", e);
			}
		}

		public event Action VisibleLinesChanged;

		private bool SetHovered(LogLineIndex index, SelectMode selectMode)
		{
			return Set(_hoveredIndices, index, selectMode);
		}

		public bool SetSelected(LogLineIndex index, SelectMode selectMode)
		{
			bool changed = Set(_selectedIndices, index, selectMode);
			_firstSelection = _lastSelection = index;

			if (changed)
			{
				var fn = OnSelectionChanged;
				fn?.Invoke(_selectedIndices);
			}

			return changed;
		}

		public void SetSelected(IEnumerable<LogLineIndex> indices, SelectMode selectMode)
		{
			if (selectMode == SelectMode.Replace)
			{
				_selectedIndices.Clear();
				if (indices != null)
				{
					foreach (LogLineIndex index in indices)
					{
						_selectedIndices.Add(index);
					}
				}

				var fn = OnSelectionChanged;
				fn?.Invoke(_selectedIndices);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public event Action<HashSet<LogLineIndex>> OnSelectionChanged;

		private static bool Set(HashSet<LogLineIndex> indices, LogLineIndex index, SelectMode selectMode)
		{
			if (selectMode == SelectMode.Replace)
			{
				if (indices.Count != 1 ||
				    !indices.Contains(index))
				{
					indices.Clear();
					indices.Add(index);
					return true;
				}
			}
			else if (indices.Add(index))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		///     The section of the log file that is currently visible.
		/// </summary>
		[Pure]
		public LogSourceSection CalculateVisibleSection()
		{
			if (_logSource == null)
				return new LogSourceSection(0, 0);

			double maxLinesInViewport = (ActualHeight - _yOffset)/_textSettings.LineHeight;
			var maxCount = (int) Math.Ceiling(maxLinesInViewport);
			var logLineCount = LogSource.GetProperty(Properties.LogEntryCount);
			// Somebody may have specified that he wants to view line X, but if the source
			// doesn't offer this line (yet), then we must show something else...
			var actualCurrentLine = _currentLine >= logLineCount ? Math.Max(0, logLineCount - maxCount) : _currentLine;
			int linesLeft = logLineCount - actualCurrentLine;
			int count = Math.Min(linesLeft, maxCount);
			if (count < 0)
				return new LogSourceSection();

			return new LogSourceSection(actualCurrentLine, count);
		}

		public void OnMouseMove()
		{
			Point relativePos = Mouse.GetPosition(this);
			if (InputHitTest(relativePos) == this)
				OnMouseMove(relativePos);
		}

		private void OnMouseMove(Point relativePos)
		{
			double y = relativePos.Y - _yOffset;
			var visibleLineIndex = (int) Math.Floor(y/_textSettings.LineHeight);
			if (visibleLineIndex >= 0 && visibleLineIndex < _visibleTextLines.Count)
			{
				var lineIndex = _visibleTextLines[visibleLineIndex].LogEntry.Index;
				if (SetHovered(lineIndex, SelectMode.Replace))
					InvalidateVisual();
			}
		}

		private void VerticalScrollBarOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
		{
			double pos = args.NewValue;
			var currentLine = (int) Math.Floor(pos/_textSettings.LineHeight);

			DetermineVerticalOffset();
			_currentLine = currentLine;
			CurrentlyVisibleSection = CalculateVisibleSection();
			UpdateVisibleLines();
		}

		private void HorizontalScrollBarOnScroll(object sender, RoutedPropertyChangedEventArgs<double> args)
		{
			// A value 0 zero means that the leftmost character shall be visible.
			// A value of MaxValue means that the rightmost character shall be visible.
			// This we need to offset each line's position by -value
			_xOffset = -args.NewValue;

			InvalidateVisual();
		}

		private void ChangeSelectionAndBringIntoView(LogLineIndex newIndex)
		{
			if (SetSelected(newIndex, SelectMode.Replace))
			{
				var fn = RequestBringIntoView;
				fn?.Invoke(newIndex, new LogLineMatch());

				InvalidateVisual();
			}
		}

		public void OnMovePageUp()
		{
			try
			{
				if (_selectedIndices.Count > 0 && _lastSelection > 0)
				{
					LogLineIndex newIndex;
					int maxDelta = _currentlyVisibleSection.Count;
					if (maxDelta > _lastSelection)
						newIndex = 0;
					else
						newIndex = _lastSelection - maxDelta;

					ChangeSelectionAndBringIntoView(newIndex);
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		public void OnMovePageDown()
		{
			try
			{
				int count = _logSource.GetProperty(Properties.LogEntryCount);
				if (_selectedIndices.Count > 0 && _lastSelection < count - 1)
				{
					LogLineIndex newIndex;
					int maxDelta = _currentlyVisibleSection.Count;
					if (maxDelta + _lastSelection >= count)
						newIndex = count - 1;
					else
						newIndex = _lastSelection + maxDelta;

					ChangeSelectionAndBringIntoView(newIndex);
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		public void OnMoveUp()
		{
			try
			{
				if (_selectedIndices.Count > 0 && _lastSelection > 0)
				{
					int newIndex = _lastSelection - 1;
					ChangeSelectionAndBringIntoView(newIndex);
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		private void OnSelectUp()
		{
			var logFile = _logSource;
			var first = _firstSelection;
			var last = _lastSelection;
			if (logFile == null)
				return;
			if (first.IsInvalid)
				return;
			if (last.IsInvalid)
				return;

			var next = last - 1;
			if (next < 0)
				return;

			var indices = LogLineIndex.Range(first, next);
			SetSelected(indices, SelectMode.Replace);
			_firstSelection = first;
			_lastSelection = next;

			var fn = RequestBringIntoView;
			fn?.Invoke(next, new LogLineMatch());

			InvalidateVisual();
		}

		private void OnSelectDown()
		{
			var logFile = _logSource;
			var first = _firstSelection;
			var last = _lastSelection;
			if (logFile == null)
				return;
			if (first.IsInvalid)
				return;
			if (last.IsInvalid)
				return;

			var next = last + 1;
			if (next >= _logSource.GetProperty(Properties.LogEntryCount))
				return;

			var indices = LogLineIndex.Range(first, next);
			SetSelected(indices, SelectMode.Replace);
			_firstSelection = first;
			_lastSelection = next;

			var fn = RequestBringIntoView;
			fn?.Invoke(next, new LogLineMatch());

			InvalidateVisual();
		}

		private void OnMoveDown()
		{
			try
			{
				if (_selectedIndices.Count > 0 && _lastSelection < _logSource.GetProperty(Properties.LogEntryCount) - 1)
				{
					LogLineIndex newIndex = _lastSelection + 1;
					ChangeSelectionAndBringIntoView(newIndex);
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		private void OnMoveStart()
		{
			try
			{
				var newIndex = new LogLineIndex(0);
				ChangeSelectionAndBringIntoView(newIndex);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		private void OnMoveEnd()
		{
			try
			{
				ILogSource logSource = _logSource;
				if (logSource != null)
				{
					var count = logSource.GetProperty(Properties.LogEntryCount);
					if (count > 0)
					{
						var newIndex = new LogLineIndex(count - 1);
						ChangeSelectionAndBringIntoView(newIndex);
					}
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		public new event Action<LogLineIndex, LogLineMatch> RequestBringIntoView;

	public void OnCopyToClipboard()
	{
		try
		{
			ILogSource logSource = _logSource;
			if (logSource == null)
				return;

			var sortedIndices = new List<LogLineIndex>(_selectedIndices);
			sortedIndices.Sort();

			// Fetch raw entries
			var buffer = new LogBufferArray(_selectedIndices.Count,
				Columns.Index, Columns.LogEntryIndex, PageBufferedLogSource.RetrievalState,
				Columns.LogLevel, Columns.RawContent);
			logSource.GetEntries(sortedIndices, buffer);

			// Build TextLine objects with highlight filters so segments carry color info
			var emptyHovered = new HashSet<LogLineIndex>();
			var emptySelected = new HashSet<LogLineIndex>();
			var textLines = new List<TextLine>(sortedIndices.Count);
			for (int i = 0; i < sortedIndices.Count; ++i)
			{
				var line = new TextLine(buffer[i], emptyHovered, emptySelected,
					_colorByLevel, _textSettings, _textBrushes)
				{
					IsFocused = false,
					HighlightFilters = _highlightFilters
				};
				textLines.Add(line);
			}

			// Extract colored spans and build plain text simultaneously
			var coloredLines = RichClipboardHelper.ExtractLines(textLines);

			// Plain text (always provided as fallback)
			var plainText = new StringBuilder();
			for (int i = 0; i < coloredLines.Count; i++)
			{
				foreach (var span in coloredLines[i].Spans)
					plainText.Append(span.Text);
				if (i < coloredLines.Count - 1)
					plainText.AppendLine();
			}

			string rtf  = RichClipboardHelper.BuildRtf(coloredLines);
			string html = RichClipboardHelper.BuildHtml(coloredLines);

			// RTF must be placed on the clipboard as a raw ANSI byte stream, not a .NET string.
			// WPF's DataObject.SetData(DataFormats.Rtf, string) has a known encoding bug: it
			// serialises the string as UTF-16, which Word's RTF parser cannot read (RTF is 7-bit
			// ASCII/ANSI). Passing a MemoryStream bypasses WPF's encoding and gives Windows the
			// raw bytes, exactly as Word expects.
			System.IO.MemoryStream rtfStream = RichClipboardHelper.BuildRtfStream(coloredLines);

			var dataObject = new DataObject();
			dataObject.SetData(DataFormats.Rtf,         rtfStream);
			dataObject.SetData(DataFormats.Html,        html);
			dataObject.SetData(DataFormats.UnicodeText, plainText.ToString());
			dataObject.SetData(DataFormats.Text,        plainText.ToString());
		Clipboard.SetDataObject(dataObject, copy: true);
			RichCopyHint?.Invoke();
		}
		catch (Exception e)
		{
			Log.ErrorFormat("Caught unexpected exception: {0}", e);
		}
	}

	/// <summary>
	///     Fired after a rich (RTF/HTML) copy-to-clipboard so the UI can inform the user
	///     that "Keep Source Formatting" is needed when pasting into Word.
	/// </summary>
	public event Action RichCopyHint;

	/// <summary>
	/// Invoked when the user double-clicks on a log line.
	/// </summary>
	public event Action<IReadOnlyLogEntry> LogLineDoubleClicked;

	#region Mouse Events

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			Point relativePos = Mouse.GetPosition(this);
			OnMouseMove(relativePos);
		}

		public event Action MouseWheelDown;
		public event Action MouseWheelUp;

		private void OnMouseWheelDown()
		{
			Action fn = MouseWheelDown;
			fn?.Invoke();

			OnMouseMove();
		}

		private void OnMouseWheelUp()
		{
			Action fn = MouseWheelUp;
			fn?.Invoke();

			OnMouseMove();
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			if (_hoveredIndices.Count > 0)
			{
				_hoveredIndices.Clear();
				InvalidateVisual();
			}
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			Log.DebugFormat("OnMouseLeftButtonDown: ClickCount={0}, _hoveredIndices.Count={1}, ButtonState={2}", 
				e.ClickCount, _hoveredIndices.Count, e.ButtonState);
			
			if (_hoveredIndices.Count > 0)
			{
				LogLineIndex index = _hoveredIndices.First();

				// Check for double-click - need ClickCount == 2 AND ButtonState == Pressed
				if (e.ClickCount == 2 && e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
				{
					Log.DebugFormat("Double-click detected on line {0}", index);
					OnMouseDoubleClick(index);
					e.Handled = true; // Mark event as handled to prevent further processing
					return;
				}

				SelectMode selectMode = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)
					                        ? SelectMode.Add
					                        : SelectMode.Replace;
				if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
				{
					if (SetSelected(_lastSelection, index, selectMode))
						InvalidateVisual();
				}
				else
				{
					if (SetSelected(index, selectMode))
						InvalidateVisual();
				}
			}

			Focus();
			base.OnMouseLeftButtonDown(e);
		}

		private void OnMouseDoubleClick(LogLineIndex index)
		{
			Log.DebugFormat("OnMouseDoubleClick called for line {0}, _logSource is {1}", index, _logSource != null ? "not null" : "null");
			
			if (_logSource != null)
			{
				try
				{
					// Fetch the log entry for the double-clicked line
					// Request only the columns that are commonly available
					var buffer = new LogBufferArray(1, Columns.Index, Columns.RawContent);
					_logSource.GetEntries(new[] { index }, buffer);
					
					Log.DebugFormat("Buffer count: {0}", buffer.Count);
					
					if (buffer.Count > 0)
					{
						IReadOnlyLogEntry entry = buffer[0];
						Log.DebugFormat("Invoking LogLineDoubleClicked event");
						LogLineDoubleClicked?.Invoke(entry);
					}
				}
				catch (Exception ex)
				{
					Log.ErrorFormat("Error retrieving log entry for double-click: {0}", ex);
				}
			}
		}

		private bool SetSelected(LogLineIndex from, LogLineIndex to, SelectMode selectMode)
		{
			bool changed = false;
			if (selectMode == SelectMode.Replace)
			{
				if (_hoveredIndices.Count > 0)
					changed = true;

				_hoveredIndices.Clear();
			}

			LogLineIndex min = LogLineIndex.Min(from, to);
			LogLineIndex max = LogLineIndex.Max(from, to);
			int count = max - min;
			for (int i = 0; i <= count /* we want to select everything including 'to' */; ++i)
			{
				changed |= _selectedIndices.Add(min + i);
			}

			if (changed)
			{
				var fn = OnSelectionChanged;
				fn?.Invoke(_selectedIndices);
			}

			return changed;
		}

		#endregion Mouse Events
	}
}