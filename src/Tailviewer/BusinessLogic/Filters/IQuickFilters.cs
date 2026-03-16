using System.Collections.Generic;
using Tailviewer.Core;

namespace Tailviewer.BusinessLogic.Filters
{


	public interface IQuickFilters
	{
		IEnumerable<QuickFilter> Filters { get; }

		/// <summary>
		/// 
		/// </summary>
		TimeFilter TimeFilter { get; }

		/// <summary>
		///     Gets or sets how multiple filters are combined (AND vs OR).
		/// </summary>
		FilterCombineMode FilterCombineMode { get; set; }

		/// <summary>
		///     Adds a new quickfilter.
		/// </summary>
		/// <returns></returns>
		QuickFilter AddQuickFilter();

		void Remove(QuickFilterId id);
	}
}