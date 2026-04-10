using System.Windows;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace Tailviewer.Ui
{
	/// <summary>
	///     Provides WPF <see cref="Geometry"/> icons from MahApps.Metro.IconPacks.Material.
	///     Replaces Metrolib.Icons.
	/// </summary>
	public static class Icons
	{
		public static Geometry Alert => GetGeometry(PackIconMaterialKind.Alert);
		public static Geometry AlertCircleOutline => GetGeometry(PackIconMaterialKind.AlertCircleOutline);
		public static Geometry Bookmark => GetGeometry(PackIconMaterialKind.Bookmark);
		public static Geometry BookmarkPlusOutline => GetGeometry(PackIconMaterialKind.BookmarkPlusOutline);
		public static Geometry BookmarkRemoveOutline => GetGeometry(PackIconMaterialKind.BookmarkRemoveOutline);
		public static Geometry ChatOutline => GetGeometry(PackIconMaterialKind.ChatOutline);
		public static Geometry ChatQuestionOutline => GetGeometry(PackIconMaterialKind.ChatQuestionOutline);
		public static Geometry Check => GetGeometry(PackIconMaterialKind.Check);
		public static Geometry CogOutline => GetGeometry(PackIconMaterialKind.CogOutline);
		public static Geometry Database => GetGeometry(PackIconMaterialKind.Database);
		public static Geometry File => GetGeometry(PackIconMaterialKind.File);
		public static Geometry FileDocumentOutline => GetGeometry(PackIconMaterialKind.FileDocumentOutline);
		public static Geometry FileOutline => GetGeometry(PackIconMaterialKind.FileOutline);
		public static Geometry FileSearch => GetGeometry(PackIconMaterialKind.FileSearch);
		public static Geometry Filter => GetGeometry(PackIconMaterialKind.Filter);
		public static Geometry FolderOpen => GetGeometry(PackIconMaterialKind.FolderOpen);
		public static Geometry Marker => GetGeometry(PackIconMaterialKind.Marker);
		public static Geometry PlaylistRemove => GetGeometry(PackIconMaterialKind.PlaylistRemove);
		public static Geometry PuzzleOutline => GetGeometry(PackIconMaterialKind.PuzzleOutline);
		public static Geometry Wrench => GetGeometry(PackIconMaterialKind.Wrench);

		private static Geometry GetGeometry(PackIconMaterialKind kind)
		{
			var icon = new PackIconMaterial { Kind = kind };
			var data = icon.Data;
			if (string.IsNullOrEmpty(data))
				return Geometry.Empty;

			var geometry = Geometry.Parse(data);
			geometry.Freeze();
			return geometry;
		}
	}
}
