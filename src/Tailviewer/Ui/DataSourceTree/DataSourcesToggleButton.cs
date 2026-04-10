using System.Windows;
using System.Windows.Controls.Primitives;

namespace Tailviewer.Ui.DataSourceTree
{
	/// <summary>
	///     The button with which the user toggles the visibility of the <see cref="DataSourcesControl" />.
	/// </summary>
	public sealed class DataSourcesToggleButton
		: ToggleButton
	{
		static DataSourcesToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DataSourcesToggleButton),
			                                         new FrameworkPropertyMetadata(typeof(DataSourcesToggleButton)));
		}
	}
}