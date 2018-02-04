using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrace.Model
{
	/// <summary>
	/// Global settings
	/// </summary>
    internal static class GlobalProperties
    {
		/// <summary>
		/// Application colors
		/// </summary>
		public enum Color
		{
			System,
			Custom
		}

		/// <summary>
		/// Current application color
		/// </summary>
		public static Color AppColor { get; set; }

		/// <summary>
		/// Initializing global properties
		/// </summary>
		static GlobalProperties()
		{
			AppColor = Color.System;
		}
    }
}
