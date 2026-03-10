using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tailviewer.Api;
using Tailviewer.Core;

namespace Tailviewer.Ui.LogView
{
	/// <summary>
	/// ViewModel for displaying JSON-formatted log entry details in a flyout.
	/// </summary>
	public sealed class JsonDetailsFlyoutViewModel
		: IFlyoutViewModel
	{
		private readonly ILogSource _logSource;
		private LogLineIndex _currentLogLineIndex;
		private string _rawContent;
		private string _formattedJson;
		private bool _isValidJson;
		private string _errorMessage;
		private TextDocument _jsonDocument;
		private int? _lineNumber;
		private DateTime? _timestamp;
		private LevelFlags _logLevel;

		public JsonDetailsFlyoutViewModel(ILogSource logSource, IReadOnlyLogEntry logEntry)
		{
			if (logSource == null)
				throw new ArgumentNullException(nameof(logSource));
			if (logEntry == null)
				throw new ArgumentNullException(nameof(logEntry));

			_logSource = logSource;
			_currentLogLineIndex = logEntry.Index;

			LoadLogEntry(logEntry);
		}

		/// <summary>
		/// The line number of the log entry.
		/// </summary>
		public int? LineNumber
		{
			get { return _lineNumber; }
			private set
			{
				if (value == _lineNumber)
					return;

				_lineNumber = value;
				EmitPropertyChanged();
			}
		}

		/// <summary>
		/// The timestamp of the log entry.
		/// </summary>
		public DateTime? Timestamp
		{
			get { return _timestamp; }
			private set
			{
				if (value == _timestamp)
					return;

				_timestamp = value;
				EmitPropertyChanged();
			}
		}

		/// <summary>
		/// The log level of the entry.
		/// </summary>
		public LevelFlags LogLevel
		{
			get { return _logLevel; }
			private set
			{
				if (value == _logLevel)
					return;

				_logLevel = value;
				EmitPropertyChanged();
			}
		}

		/// <summary>
		/// The original raw content of the log entry.
		/// </summary>
		public string RawContent => _rawContent;

		/// <summary>
		/// The formatted JSON content (if valid JSON).
		/// </summary>
		public string FormattedJson
		{
			get { return _formattedJson; }
			private set
			{
				if (value == _formattedJson)
					return;

				_formattedJson = value;
				EmitPropertyChanged();
			}
		}

		/// <summary>
		/// True if the content is valid JSON, false otherwise.
		/// </summary>
		public bool IsValidJson
		{
			get { return _isValidJson; }
			private set
			{
				if (value == _isValidJson)
					return;

				_isValidJson = value;
				EmitPropertyChanged();
			}
		}

		/// <summary>
		/// Error message if JSON parsing failed.
		/// </summary>
		public string ErrorMessage
		{
			get { return _errorMessage; }
			private set
			{
				if (value == _errorMessage)
					return;

				_errorMessage = value;
				EmitPropertyChanged();
			}
		}

		/// <summary>
		/// The text document for AvalonEdit with syntax highlighting and search.
		/// </summary>
		public TextDocument JsonDocument
		{
			get { return _jsonDocument; }
			private set
			{
				if (value == _jsonDocument)
					return;

				_jsonDocument = value;
				EmitPropertyChanged();
			}
		}

		public string Name => "Log Entry Details";

		public event PropertyChangedEventHandler PropertyChanged;

		public void Update()
		{
			// Nothing to update
		}

		/// <summary>
		/// Navigate to the previous log line.
		/// </summary>
		public void NavigateToPreviousLine()
		{
			var previousIndex = new LogLineIndex(_currentLogLineIndex.Value - 1);
			if (previousIndex.Value < 0)
				return;

			NavigateToLine(previousIndex);
		}

		/// <summary>
		/// Navigate to the next log line.
		/// </summary>
		public void NavigateToNextLine()
		{
			var nextIndex = new LogLineIndex(_currentLogLineIndex.Value + 1);
			NavigateToLine(nextIndex);
		}

	private void NavigateToLine(LogLineIndex lineIndex)
	{
		try
		{
			// Request only commonly available columns
			var buffer = new LogBufferArray(1, Columns.Index, Columns.RawContent);
			_logSource.GetEntries(new[] { lineIndex }, buffer);

			if (buffer.Count > 0)
			{
				var entry = buffer[0];
				_currentLogLineIndex = lineIndex;
				LoadLogEntry(entry);
			}
		}
		catch (Exception)
		{
			// Ignore errors when navigating beyond bounds
		}
	}

	private void LoadLogEntry(IReadOnlyLogEntry logEntry)
	{
		// Safely retrieve RawContent (always available)
		_rawContent = logEntry.RawContent;
		
		// Try to retrieve optional columns
		try
		{
			LineNumber = logEntry.LineNumber;
		}
		catch (ColumnNotRetrievedException)
		{
			LineNumber = null;
		}
		
		try
		{
			Timestamp = logEntry.Timestamp;
		}
		catch (ColumnNotRetrievedException)
		{
			Timestamp = null;
		}
		
		try
		{
			LogLevel = logEntry.LogLevel;
		}
		catch (ColumnNotRetrievedException)
		{
			LogLevel = LevelFlags.None;
		}

		ParseJson();
	}

		private void ParseJson()
		{
			if (string.IsNullOrWhiteSpace(_rawContent))
			{
				IsValidJson = false;
				ErrorMessage = "No content to parse";
				FormattedJson = string.Empty;
				JsonDocument = new TextDocument(string.Empty);
				return;
			}

			try
			{
				// Try to parse as JSON
				JToken token = JToken.Parse(_rawContent);
				FormattedJson = token.ToString(Formatting.Indented);
				IsValidJson = true;
				ErrorMessage = null;

				// Create TextDocument for AvalonEdit
				JsonDocument = new TextDocument(FormattedJson);
			}
			catch (JsonReaderException ex)
			{
				IsValidJson = false;
				ErrorMessage = $"Invalid JSON: {ex.Message}";
				FormattedJson = _rawContent;
				JsonDocument = new TextDocument(_rawContent);
			}
			catch (Exception ex)
			{
				IsValidJson = false;
				ErrorMessage = $"Error parsing content: {ex.Message}";
				FormattedJson = _rawContent;
				JsonDocument = new TextDocument(_rawContent);
			}
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
