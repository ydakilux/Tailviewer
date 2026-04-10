using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using log4net;
using Tailviewer.Api;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.Core;

namespace Tailviewer.Ui.QuickFilter
{
	/// <summary>
	///     The view model for a quick filter editor: New filters can be added,
	///     existing filters modified and removed.
	///     Is used in the "quick filters" side panel as well as in various
	///     widget editors.
	/// </summary>
	public sealed class QuickFiltersViewModel
		: INotifyPropertyChanged
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IQuickFilters _quickFilters;
		private readonly ObservableCollection<QuickFilterViewModel> _viewModels;
		private IDataSource _currentDataSource;
		private bool _isChangingCurrentDataSource;

		public QuickFiltersViewModel(IQuickFilters quickFilters)
		{
			if (quickFilters == null) throw new ArgumentNullException(nameof(quickFilters));

			_quickFilters = quickFilters;
			AddCommand = new DelegateCommand(() => AddQuickFilter());
			_viewModels = new ObservableCollection<QuickFilterViewModel>();
			foreach (var filter in quickFilters.Filters)
				CreateAndAddViewModel(filter);
		}

		public ICommand AddCommand { get; }

		public IEnumerable<QuickFilterViewModel> QuickFilters => _viewModels;

		/// <summary>
		///     Gets or sets the filter combine mode (AND vs OR).
		/// </summary>
		public FilterCombineMode FilterCombineMode
		{
			get { return _quickFilters.FilterCombineMode; }
			set
			{
				if (_quickFilters.FilterCombineMode != value)
				{
					_quickFilters.FilterCombineMode = value;
					EmitPropertyChanged();
					OnFiltersChanged?.Invoke();
				}
			}
		}

		public IDataSource CurrentDataSource
		{
			get { return _currentDataSource; }
			set
			{
				if (value == _currentDataSource)
					return;

				try
				{
					_isChangingCurrentDataSource = true;

					_currentDataSource = value;
					foreach (var viewModel in _viewModels)
						viewModel.CurrentDataSource = value;

					OnFiltersChanged?.Invoke();
				}
				finally
				{
					_isChangingCurrentDataSource = false;
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		///     This event is fired whenever a new filter has been added.
		/// </summary>
		public event Action OnFilterAdded;

		/// <summary>
		///     This event is fired whenever a filter has been removed.
		/// </summary>
		public event Action OnFilterRemoved;

		/// <summary>
		///     This event is fired when a filter has been changed so the filtered contents MIGHT change.
		///     This includes: activate/deactive, changing the filter value, etc...
		/// </summary>
		public event Action OnFiltersChanged;

		public QuickFilterViewModel AddQuickFilter()
		{
			var quickFilter = _quickFilters.AddQuickFilter();
			var viewModel = CreateAndAddViewModel(quickFilter);
			OnFilterAdded?.Invoke();
			return viewModel;
		}

		private QuickFilterViewModel CreateAndAddViewModel(BusinessLogic.Filters.QuickFilter quickFilter)
		{
			var viewModel = new QuickFilterViewModel(quickFilter, OnRemoveQuickFilter)
			{
				CurrentDataSource = _currentDataSource
			};
			viewModel.PropertyChanged += QuickFilterOnPropertyChanged;
			_viewModels.Add(viewModel);
			return viewModel;
		}

		public List<ILogEntryFilter> CreateFilterChain()
		{
			var filters = new List<ILogEntryFilter>(_viewModels.Count);
		// ReSharper disable LoopCanBeConvertedToQuery
		foreach (var quickFilter in _viewModels)
			// ReSharper restore LoopCanBeConvertedToQuery
		{
			// Only include filters in "hide mode" (not highlight-only)
			if (quickFilter.IsHighlightOnly)
				continue;

				ILogEntryFilter filter = null;
				try
				{
					filter = quickFilter.CreateFilter();
				}
				catch (Exception e)
				{
					Log.DebugFormat("Caught exception while creating quick filter: {0}", e);
				}

				if (filter != null)
					filters.Add(filter);
			}

			if (filters.Count == 0)
				return null;

			return filters;
		}

	/// <summary>
	///     Returns all active highlight-only filters with their colors.
	///     These filters should be used for visual highlighting only, not for hiding lines.
	/// </summary>
	public List<HighlightFilter> GetHighlightFilters()
	{
		var highlightFilters = new List<HighlightFilter>();
		
		foreach (var quickFilter in _viewModels)
		{
			// Include highlight-only filters, AND hide-mode filters that have a highlight color set
			bool shouldHighlight = quickFilter.IsActive &&
			                       (quickFilter.IsHighlightOnly || quickFilter.HighlightColor.HasValue);
			if (!shouldHighlight)
			{
				continue;
			}

			ILogEntryFilter filter = null;
			try
			{
				filter = quickFilter.CreateFilter();
			}
			catch (Exception e)
			{
				Log.DebugFormat("Caught exception while creating quick filter: {0}", e);
			}

			if (filter != null)
			{
				highlightFilters.Add(new HighlightFilter
				{
					Filter = filter,
					HighlightColor = quickFilter.HighlightColor,
					ForegroundColor = quickFilter.ForegroundColor
				});
			}
		}

		return highlightFilters;
	}

	private void QuickFilterOnPropertyChanged(object sender, PropertyChangedEventArgs args)
	{
		var model = sender as QuickFilterViewModel;
		if (model == null)
			return;

		switch (args.PropertyName)
		{
			case "Value":
			case "IsActive":
			case "IsInverted":
			case "DropType":
			case "MatchType":
			case "IsHighlightOnly":
			case "HighlightColor":
			case "ForegroundColor":
				if (!_isChangingCurrentDataSource)
					OnFiltersChanged?.Invoke();
				break;
		}
	}

		private void OnRemoveQuickFilter(QuickFilterViewModel viewModel)
		{
			_viewModels.Remove(viewModel);
			_quickFilters.Remove(viewModel.Id);
			viewModel.PropertyChanged -= QuickFilterOnPropertyChanged;

			OnFilterRemoved?.Invoke();

			if (viewModel.IsActive)
				OnFiltersChanged?.Invoke();
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}