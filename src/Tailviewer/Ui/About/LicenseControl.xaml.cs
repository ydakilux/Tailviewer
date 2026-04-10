using System.Diagnostics;
using System.Windows.Navigation;

namespace Tailviewer.Ui.About
{
	public partial class LicenseControl
	{
		public LicenseControl()
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
