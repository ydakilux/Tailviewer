using System.Windows;
using System.Xml;
using Tailviewer.Core;

namespace Tailviewer.Settings
{
	/// <summary>
	///     Persists and restores the position/size/state of a WPF window.
	///     Replaces the Metrolib.WindowSettings class.
	/// </summary>
	public sealed class WindowSettings
	{
		public double Left { get; set; }
		public double Top { get; set; }
		public double Width { get; set; } = 800;
		public double Height { get; set; } = 600;
		public WindowState State { get; set; } = WindowState.Normal;

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeDouble("left", Left);
			writer.WriteAttributeDouble("top", Top);
			writer.WriteAttributeDouble("width", Width);
			writer.WriteAttributeDouble("height", Height);
			writer.WriteAttributeEnum("state", State);
		}

		public void Restore(XmlReader reader)
		{
			for (var i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "left":
						Left = reader.ReadContentAsDouble();
						break;
					case "top":
						Top = reader.ReadContentAsDouble();
						break;
					case "width":
						Width = reader.ReadContentAsDouble();
						break;
					case "height":
						Height = reader.ReadContentAsDouble();
						break;
					case "state":
						State = reader.ReadContentAsEnum<WindowState>();
						break;
				}
			}
		}

		public void UpdateFrom(Window window)
		{
			Left = window.Left;
			Top = window.Top;
			Width = window.Width;
			Height = window.Height;
			State = window.WindowState;
		}

		public void RestoreTo(Window window)
		{
			window.Left = Left;
			window.Top = Top;
			window.Width = Width;
			window.Height = Height;
			window.WindowState = State;
		}

		public WindowSettings Clone()
		{
			return new WindowSettings
			{
				Left = Left,
				Top = Top,
				Width = Width,
				Height = Height,
				State = State
			};
		}
	}
}
