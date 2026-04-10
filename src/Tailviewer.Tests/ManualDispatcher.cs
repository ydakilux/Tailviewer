using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Metrolib;

namespace Tailviewer.Test
{
	public sealed class ManualDispatcher
		: IDispatcher
	{
		private readonly SortedDictionary<DispatcherPriority, List<Action>> _pendingInvokes;

		public ManualDispatcher()
		{
			_pendingInvokes = new SortedDictionary<DispatcherPriority, List<Action>>();
		}

		public bool HasAccess => true;

		public void BeginInvoke(Action fn)
		{
			BeginInvoke(fn, DispatcherPriority.Normal);
		}

		public void BeginInvoke(Action fn, DispatcherPriority priority)
		{
			lock (_pendingInvokes)
			{
				List<Action> invokes;
				if (!_pendingInvokes.TryGetValue(priority, out invokes))
				{
					invokes = new List<Action>();
					_pendingInvokes.Add(priority, invokes);
				}

				invokes.Add(fn);
			}
		}

		public Task BeginInvokeAsync(Action fn)
		{
			return BeginInvokeAsync(fn, DispatcherPriority.Normal);
		}

		public Task BeginInvokeAsync(Action fn, DispatcherPriority priority)
		{
			BeginInvoke(fn, priority);
			return Task.CompletedTask;
		}

		public void InvokeAll()
		{
			List<KeyValuePair<DispatcherPriority, List<Action>>> pendingInvokes;

			lock (_pendingInvokes)
			{
				pendingInvokes = _pendingInvokes.Select(x =>
				                                        new KeyValuePair<DispatcherPriority, List<Action>>(
					                                        x.Key, x.Value.ToList()
					                                        )).ToList();
				_pendingInvokes.Clear();
			}

			foreach (var pair in pendingInvokes)
			{
				List<Action> invokes = pair.Value;
				foreach (Action invoke in invokes)
				{
					invoke();
				}
			}
		}
	}
}