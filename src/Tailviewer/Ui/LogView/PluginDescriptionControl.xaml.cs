using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Tailviewer.Ui.LogView
{
	/// <summary>
	/// Interaction logic for PluginDescriptionControl.xaml
	/// </summary>
	public partial class PluginDescriptionControl : UserControl
	{
		public PluginDescriptionControl()
		{
			InitializeComponent();
		}

		private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
			e.Handled = true;
		}
	}
}
