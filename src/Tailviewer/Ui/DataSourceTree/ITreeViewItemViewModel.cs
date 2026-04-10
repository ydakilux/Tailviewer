namespace Tailviewer.Ui.DataSourceTree
{
	/// <summary>
	///     Represents a single item in a tree view that can be selected and expanded.
	///     Replaces Metrolib.ITreeViewItemViewModel.
	/// </summary>
	public interface ITreeViewItemViewModel
	{
		/// <summary>Whether this item is currently selected.</summary>
		bool IsSelected { get; set; }

		/// <summary>Whether this item is currently expanded.</summary>
		bool IsExpanded { get; set; }
	}
}
