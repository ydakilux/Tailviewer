using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Tailviewer.Ui.About
{
	/// <summary>
	/// Interaction logic for TailviewerControl.xaml
	/// </summary>
	public partial class TailviewerControl : UserControl
	{
		public TailviewerControl()
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
