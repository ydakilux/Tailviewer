using Tailviewer.Api;
using Tailviewer.Ui.LogView;

namespace Tailviewer.Ui
{
	internal sealed class NavigationService
		: INavigationService
	{
		public LogViewMainPanelViewModel LogViewer;
		public MainWindowViewModel MainWindow;

		#region Implementation of INavigationService

		public bool NavigateTo(LogLineIndex line)
		{
			if (MainWindow == null)
				return false;

			if (LogViewer == null)
				return false;

			return LogViewer.RequestBringIntoView(line);
		}

		public bool NavigateTo(DataSourceId dataSource, LogLineIndex line)
		{
			if (MainWindow == null)
				return false;

			if (LogViewer == null)
				return false;

			return LogViewer.RequestBringIntoView(dataSource, line);
		}

		#endregion

		/// <summary>
		/// Shows a flyout in the main window.
		/// </summary>
		/// <param name="flyout">The flyout view model to display</param>
		public void ShowFlyout(IFlyoutViewModel flyout)
		{
			if (MainWindow != null)
			{
				MainWindow.CurrentFlyout = flyout;
			}
		}
	}
}

