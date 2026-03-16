using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using log4net;
using Metrolib;

namespace Tailviewer.Ui.Controls
{
	/// <summary>
	///     A simple color picker control with preset colors.
	/// </summary>
	public partial class SimpleColorPicker
		: UserControl
		, INotifyPropertyChanged
	{
	private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

	public static readonly DependencyProperty SelectedColorProperty =
		DependencyProperty.Register(nameof(SelectedColor), typeof(Color?), typeof(SimpleColorPicker),
			new PropertyMetadata(null, OnSelectedColorChanged));

	public static readonly DependencyProperty SelectColorCommandProperty =
		DependencyProperty.Register(nameof(SelectColorCommand), typeof(ICommand), typeof(SimpleColorPicker),
			new PropertyMetadata(null));

	public static readonly DependencyProperty ClearColorCommandProperty =
		DependencyProperty.Register(nameof(ClearColorCommand), typeof(ICommand), typeof(SimpleColorPicker),
			new PropertyMetadata(null));

	private SolidColorBrush _selectedColorBrush;

	public SimpleColorPicker()
	{
		InitializeComponent();
		PresetColors = CreatePresetColors();
		SelectColorCommand = new DelegateCommand<object>(param => OnColorButtonClick((Color)param));
		ClearColorCommand = new DelegateCommand(OnClearButtonClick);
		UpdateSelectedColorBrush();
		DataContext = this;
	}

	private void OnColorButtonClick(Color color)
	{
		SelectedColor = color;
	}

	private void OnClearButtonClick()
	{
		SelectedColor = null;
	}

	public Color? SelectedColor
	{
		get { return (Color?)GetValue(SelectedColorProperty); }
		set { SetValue(SelectedColorProperty, value); }
	}

	public ICommand SelectColorCommand
	{
		get { return (ICommand)GetValue(SelectColorCommandProperty); }
		set { SetValue(SelectColorCommandProperty, value); }
	}

	public ICommand ClearColorCommand
	{
		get { return (ICommand)GetValue(ClearColorCommandProperty); }
		set { SetValue(ClearColorCommandProperty, value); }
	}

		public SolidColorBrush SelectedColorBrush
		{
			get { return _selectedColorBrush; }
			private set
			{
				if (Equals(_selectedColorBrush, value))
					return;

				_selectedColorBrush = value;
				EmitPropertyChanged();
			}
		}

	public List<PresetColor> PresetColors { get; }

	public event PropertyChangedEventHandler PropertyChanged;

	private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var picker = d as SimpleColorPicker;
		if (picker != null)
			picker.UpdateSelectedColorBrush();
	}

		private void UpdateSelectedColorBrush()
		{
			SelectedColorBrush = SelectedColor.HasValue
			? new SolidColorBrush(SelectedColor.Value)
			: new SolidColorBrush(Colors.Transparent);
	}

	private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private static List<PresetColor> CreatePresetColors()
		{
			return new List<PresetColor>
			{
				new PresetColor("Red", Color.FromRgb(255, 100, 100)),
				new PresetColor("Orange", Color.FromRgb(255, 180, 100)),
				new PresetColor("Yellow", Color.FromRgb(255, 255, 150)),
				new PresetColor("Green", Color.FromRgb(150, 255, 150)),
				new PresetColor("Blue", Color.FromRgb(150, 200, 255)),
				new PresetColor("Purple", Color.FromRgb(200, 150, 255)),
				new PresetColor("Pink", Color.FromRgb(255, 150, 200)),
				new PresetColor("Gray", Color.FromRgb(200, 200, 200))
			};
		}
	}

	public sealed class PresetColor
	{
		public PresetColor(string name, Color color)
		{
			Name = name;
			Color = color;
			Brush = new SolidColorBrush(color);
		}

		public string Name { get; }
		public Color Color { get; }
		public SolidColorBrush Brush { get; }
	}
}
