using System.Windows.Controls;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;

namespace Tailviewer.Ui.LogView
{
	/// <summary>
	/// UserControl for displaying JSON log entry details with syntax highlighting and search.
	/// </summary>
	public partial class JsonDetailsFlyoutControl : UserControl
	{
	public JsonDetailsFlyoutControl()
	{
		InitializeComponent();

		// Enable JSON syntax highlighting
		JsonEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Json");

		// Install search panel (CTRL+F support with automatic highlighting)
		SearchPanel.Install(JsonEditor);

		// Add keyboard handlers for navigation using PreviewKeyDown to intercept before AvalonEdit
		this.PreviewKeyDown += OnPreviewKeyDown;
	}

	private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
	{
		// Set focus to the control so it can receive keyboard events
		this.Focus();
	}

	private void OnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		var viewModel = DataContext as JsonDetailsFlyoutViewModel;
		if (viewModel == null)
			return;

		// CTRL+Up Arrow - Previous line
		if (e.Key == Key.Up && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
		{
			viewModel.NavigateToPreviousLine();
			e.Handled = true;
		}
		// CTRL+Down Arrow - Next line
		else if (e.Key == Key.Down && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
		{
			viewModel.NavigateToNextLine();
			e.Handled = true;
		}
	}
	}
}
