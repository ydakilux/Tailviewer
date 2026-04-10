using System.Windows;
using System.Windows.Controls.Primitives;
using MahApps.Metro.IconPacks;

namespace Tailviewer.Ui.LogView
{
	public class ToolbarToggleButton
		: ToggleButton
	{
		public static readonly DependencyProperty CheckedIconProperty =
			DependencyProperty.Register("CheckedIcon", typeof(PackIconMaterialKind), typeof(ToolbarToggleButton),
			                            new PropertyMetadata(PackIconMaterialKind.None));

		public static readonly DependencyProperty UncheckedIconProperty = DependencyProperty.Register(
		 "UncheckedIcon", typeof(PackIconMaterialKind), typeof(ToolbarToggleButton), new PropertyMetadata(PackIconMaterialKind.None));

		static ToolbarToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolbarToggleButton),
			                                         new FrameworkPropertyMetadata(typeof(ToolbarToggleButton)));
		}

		public PackIconMaterialKind UncheckedIcon
		{
			get { return (PackIconMaterialKind) GetValue(UncheckedIconProperty); }
			set { SetValue(UncheckedIconProperty, value); }
		}

		public PackIconMaterialKind CheckedIcon

		{
			get { return (PackIconMaterialKind) GetValue(CheckedIconProperty); }
			set { SetValue(CheckedIconProperty, value); }
		}
	}
}