using System.Windows;
using System.Windows.Controls;

namespace Tailviewer.Ui.QuickNavigation
{
	/// <summary>
	///     The popup which hosts a fancy text-box which displays a list of data sources
	///     which match the entered term.
	/// </summary>
	public sealed class QuickNavigationPopup
		: AutoPopup<TextBox>
	{
		static QuickNavigationPopup()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(QuickNavigationPopup),
			                                         new FrameworkPropertyMetadata(typeof(QuickNavigationPopup)));
		}
	}
}
