using System.Windows;
using MahApps.Metro.IconPacks;
using Tailviewer.Api;

namespace Tailviewer.Ui.LogView
{
	/// <summary>
	///     Quickly filters / shows log entries with the specified log level.
	/// </summary>
	public sealed class LogLevelToggleButton
		: ToolbarToggleButton
	{
		public static readonly DependencyProperty LogLevelProperty = DependencyProperty.Register(
		 "LogLevel", typeof(LevelFlags), typeof(LogLevelToggleButton),
		 new PropertyMetadata(default(LevelFlags), OnLogLevelChanged));

		static LogLevelToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LogLevelToggleButton),
			                                         new FrameworkPropertyMetadata(typeof(LogLevelToggleButton)));
		}

		public LevelFlags LogLevel
		{
			get { return (LevelFlags) GetValue(LogLevelProperty); }
			set { SetValue(LogLevelProperty, value); }
		}

		private static void OnLogLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((LogLevelToggleButton) d).OnLogLevelChanged((LevelFlags) e.NewValue);
		}

		private void OnLogLevelChanged(LevelFlags logLevel)
		{
			CheckedIcon = GetCheckedIcon(logLevel);
			UncheckedIcon = GetUncheckedIcon(logLevel);
		}

		private PackIconMaterialKind GetCheckedIcon(LevelFlags logLevel)
		{
			switch (logLevel)
			{
				case LevelFlags.Other: return PackIconMaterialKind.DotsHorizontal;
				case LevelFlags.Trace: return PackIconMaterialKind.MessageTextOutline;
				case LevelFlags.Debug: return PackIconMaterialKind.BugOutline;
				case LevelFlags.Info: return PackIconMaterialKind.InformationOutline;
				case LevelFlags.Warning: return PackIconMaterialKind.AlertOutline;
				case LevelFlags.Error: return PackIconMaterialKind.AlertCircleOutline;
				case LevelFlags.Fatal: return PackIconMaterialKind.AlertOctagonOutline;
				case LevelFlags.All: return PackIconMaterialKind.CheckAll;
				default:
					return PackIconMaterialKind.None;
			}
		}

		private PackIconMaterialKind GetUncheckedIcon(LevelFlags logLevel)
		{
			switch (logLevel)
			{
				case LevelFlags.Other: return PackIconMaterialKind.DotsHorizontal;
				case LevelFlags.Trace: return PackIconMaterialKind.MessageOff;
				case LevelFlags.Debug: return PackIconMaterialKind.BugCheck;
				case LevelFlags.Info: return PackIconMaterialKind.InformationOff;
				case LevelFlags.Warning: return PackIconMaterialKind.AlertRemove;
				case LevelFlags.Error: return PackIconMaterialKind.CloseCircleOutline;
				case LevelFlags.Fatal: return PackIconMaterialKind.CloseOctagonOutline;
				case LevelFlags.All: return PackIconMaterialKind.CloseBox;
				default:
					return PackIconMaterialKind.None;
			}
		}
	}
}