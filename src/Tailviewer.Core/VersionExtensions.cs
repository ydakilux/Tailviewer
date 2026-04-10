using System;

namespace Tailviewer.Core
{
	/// <summary>
	///    Extension methods for <see cref="System.Version" />.
	/// </summary>
	public static class VersionExtensions
	{
		/// <summary>
		///    Formats the version as a 3-component string (major.minor.build).
		/// </summary>
		/// <param name="version">The version to format.</param>
		/// <returns>A string of the form "major.minor.build", or "0.0.0" if <paramref name="version" /> is null.</returns>
		public static string Format(this Version version)
		{
			if (version == null)
				return "0.0.0";

			return version.ToString(3);
		}
	}
}
