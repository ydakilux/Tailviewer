using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Tailviewer.Ui
{
	/// <summary>
	/// Dispatcher abstraction interface.
	/// Replaces Metrolib.IDispatcher.
	/// </summary>
	public interface IDispatcher
	{
		bool CheckAccess();
		void BeginInvoke(Action action);
		Task BeginInvokeAsync(Action action);
		void Invoke(Action action);
	}

	/// <summary>
	/// WPF Dispatcher wrapper implementation.
	/// Replaces Metrolib.UiDispatcher.
	/// </summary>
	public sealed class UiDispatcher : IDispatcher
	{
		private readonly Dispatcher _dispatcher;

		public UiDispatcher(Dispatcher dispatcher)
		{
			_dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
		}

		public bool CheckAccess()
		{
			return _dispatcher.CheckAccess();
		}

		public void BeginInvoke(Action action)
		{
			_dispatcher.BeginInvoke(action);
		}

		public Task BeginInvokeAsync(Action action)
		{
			return _dispatcher.InvokeAsync(action).Task;
		}

		public void Invoke(Action action)
		{
			_dispatcher.Invoke(action);
		}
	}
}
