using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tailviewer.Ui
{
	/// <summary>
	/// Manual dispatcher for testing purposes.
	/// Replaces Metrolib.ManualDispatcher.
	/// </summary>
	public sealed class ManualDispatcher : IDispatcher
	{
		private readonly Queue<Action> _pendingActions = new Queue<Action>();

		public bool CheckAccess()
		{
			return true;
		}

		public void BeginInvoke(Action action)
		{
			_pendingActions.Enqueue(action);
		}

		public Task BeginInvokeAsync(Action action)
		{
			_pendingActions.Enqueue(action);
			return Task.CompletedTask;
		}

		public void Invoke(Action action)
		{
			action();
		}

		/// <summary>
		/// Executes all pending actions.
		/// </summary>
		public void ExecutePendingActions()
		{
			while (_pendingActions.Count > 0)
			{
				var action = _pendingActions.Dequeue();
				action();
			}
		}

		/// <summary>
		/// Alias for ExecutePendingActions() to match Metrolib API.
		/// </summary>
		public void InvokeAll()
		{
			ExecutePendingActions();
		}

		/// <summary>
		/// Gets the number of pending actions.
		/// </summary>
		public int PendingActionCount => _pendingActions.Count;
	}
}
