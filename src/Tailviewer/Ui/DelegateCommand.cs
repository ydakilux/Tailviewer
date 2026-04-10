using System;
using System.Windows.Input;

namespace Tailviewer.Ui
{
	/// <summary>
	/// Simple ICommand implementation that delegates to Action methods.
	/// Replaces Metrolib.DelegateCommand2.
	/// </summary>
	public sealed class DelegateCommand : ICommand
	{
		private readonly Action _execute;
		private readonly Func<bool> _canExecute;
		private bool? _canBeExecuted;

		public DelegateCommand(Action execute, Func<bool> canExecute = null)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		/// <summary>
		/// When set, overrides the canExecute delegate and directly controls whether the command can execute.
		/// Replaces Metrolib.DelegateCommand2.CanBeExecuted.
		/// </summary>
		public bool CanBeExecuted
		{
			get => _canBeExecuted ?? (_canExecute == null || _canExecute());
			set
			{
				_canBeExecuted = value;
				RaiseCanExecuteChanged();
			}
		}

		public bool CanExecute(object parameter)
		{
			if (_canBeExecuted.HasValue)
				return _canBeExecuted.Value;
			return _canExecute == null || _canExecute();
		}

		public void Execute(object parameter)
		{
			_execute();
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void RaiseCanExecuteChanged()
		{
			CommandManager.InvalidateRequerySuggested();
		}
	}

	/// <summary>
	/// ICommand implementation that accepts a parameter.
	/// Replaces Metrolib.DelegateCommand2&lt;T&gt;.
	/// </summary>
	public sealed class DelegateCommand<T> : ICommand
	{
		private readonly Action<T> _execute;
		private readonly Func<T, bool> _canExecute;

		public DelegateCommand(Action<T> execute, Func<T, bool> canExecute = null)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			if (_canExecute == null)
				return true;

			if (parameter == null && typeof(T).IsValueType)
				return false;

			return _canExecute((T)parameter);
		}

		public void Execute(object parameter)
		{
			_execute((T)parameter);
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void RaiseCanExecuteChanged()
		{
			CommandManager.InvalidateRequerySuggested();
		}
	}
}
