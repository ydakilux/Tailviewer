using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Tailviewer.Ui
{
	/// <summary>
	/// Extension methods for finding children in the WPF visual tree.
	/// Replaces Metrolib visual tree helpers.
	/// </summary>
	public static class VisualTreeExtensions
	{
		/// <summary>
		/// Finds all children of a specific type in the visual tree.
		/// </summary>
		public static IEnumerable<T> FindChildrenOfType<T>(this DependencyObject parent) where T : DependencyObject
		{
			if (parent == null)
				yield break;

			var childCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < childCount; i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				
				if (child is T typedChild)
					yield return typedChild;

				foreach (var descendant in FindChildrenOfType<T>(child))
					yield return descendant;
			}
		}

		/// <summary>
		/// Finds the first child of a specific type in the visual tree.
		/// </summary>
		public static T FindChildOfType<T>(this DependencyObject parent) where T : DependencyObject
		{
			if (parent == null)
				return null;

			var childCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < childCount; i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				
				if (child is T typedChild)
					return typedChild;

				var descendant = FindChildOfType<T>(child);
				if (descendant != null)
					return descendant;
			}

			return null;
		}
	}
}
