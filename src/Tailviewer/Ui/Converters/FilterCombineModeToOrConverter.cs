using System;
using System.Globalization;
using System.Windows.Data;
using Tailviewer.Core;

namespace Tailviewer.Ui.Converters
{
	/// <summary>
	///     Converts FilterCombineMode to bool (true = OR, false = AND).
	/// </summary>
	public sealed class FilterCombineModeToOrConverter
		: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is FilterCombineMode mode)
			{
				return mode == FilterCombineMode.Or;
			}
			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool isOr)
			{
				return isOr ? FilterCombineMode.Or : FilterCombineMode.And;
			}
			return FilterCombineMode.Or;
		}
	}
}
