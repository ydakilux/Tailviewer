using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tailviewer.Ui.Converters
{
	/// <summary>
	/// Converts bool to Visibility. False → Collapsed.
	/// </summary>
	[ValueConversion(typeof(bool), typeof(Visibility))]
	public sealed class BoolFalseToCollapsedConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool boolValue)
			{
				return boolValue ? Visibility.Visible : Visibility.Collapsed;
			}
			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Visibility visibility)
			{
				return visibility == Visibility.Visible;
			}
			return false;
		}
	}

	/// <summary>
	/// Converts bool to Visibility. True → Collapsed.
	/// </summary>
	[ValueConversion(typeof(bool), typeof(Visibility))]
	public sealed class BoolTrueToCollapsedConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool boolValue)
			{
				return boolValue ? Visibility.Collapsed : Visibility.Visible;
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Visibility visibility)
			{
				return visibility != Visibility.Visible;
			}
			return true;
		}
	}

	/// <summary>
	/// Converts bool to Visibility. False → Hidden.
	/// </summary>
	[ValueConversion(typeof(bool), typeof(Visibility))]
	public sealed class BoolFalseToHiddenConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool boolValue)
			{
				return boolValue ? Visibility.Visible : Visibility.Hidden;
			}
			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Visibility visibility)
			{
				return visibility == Visibility.Visible;
			}
			return false;
		}
	}

	/// <summary>
	/// Converts bool to Visibility. True → Hidden.
	/// </summary>
	[ValueConversion(typeof(bool), typeof(Visibility))]
	public sealed class BoolTrueToHiddenConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool boolValue)
			{
				return boolValue ? Visibility.Hidden : Visibility.Visible;
			}
			return Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Visibility visibility)
			{
				return visibility != Visibility.Visible;
			}
			return true;
		}
	}

	/// <summary>
	/// Converts null to Visibility. Null → Collapsed.
	/// </summary>
	[ValueConversion(typeof(object), typeof(Visibility))]
	public sealed class NullToCollapsedConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value == null ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>
	/// Converts null to Visibility. Null → Visible.
	/// </summary>
	[ValueConversion(typeof(object), typeof(Visibility))]
	public sealed class NullToVisibleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value == null ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>
	/// Converts null to bool. Null → False.
	/// </summary>
	[ValueConversion(typeof(object), typeof(bool))]
	public sealed class NullToFalseConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>
	/// Inverts a boolean value.
	/// </summary>
	[ValueConversion(typeof(bool), typeof(bool))]
	public sealed class InvertBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool boolValue)
			{
				return !boolValue;
			}
			return true;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool boolValue)
			{
				return !boolValue;
			}
			return false;
		}
	}
}
