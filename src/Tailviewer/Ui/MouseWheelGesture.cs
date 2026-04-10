using System.Windows.Input;

namespace Tailviewer.Ui
{
	/// <summary>
	/// Provides mouse wheel input gestures.
	/// Replaces Metrolib.Controls.MouseWheelGesture.
	/// </summary>
	public sealed class MouseWheelGesture : MouseGesture
	{
		/// <summary>Gets a gesture that matches a wheel scroll upward (positive delta).</summary>
		public static MouseWheelGesture WheelUp { get; } = new MouseWheelGesture(WheelDirection.Up);

		/// <summary>Gets a gesture that matches a wheel scroll downward (negative delta).</summary>
		public static MouseWheelGesture WheelDown { get; } = new MouseWheelGesture(WheelDirection.Down);

		private readonly WheelDirection _direction;

		private MouseWheelGesture(WheelDirection direction)
			: base(MouseAction.WheelClick)
		{
			_direction = direction;
		}

		/// <inheritdoc />
		public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
		{
			var args = inputEventArgs as MouseWheelEventArgs;
			if (args == null)
				return false;

			if (!base.Matches(targetElement, inputEventArgs))
				return false;

			return _direction == WheelDirection.Up ? args.Delta > 0 : args.Delta < 0;
		}

		private enum WheelDirection
		{
			Up,
			Down
		}
	}
}
