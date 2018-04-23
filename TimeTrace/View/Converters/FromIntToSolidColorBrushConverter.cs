using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace TimeTrace.View.Converters
{
	public class FromIntToSolidColorBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var ColorsTable = new Dictionary<int, string>()
			{
				{ 0, "#d50000" },
				{ 1, "#E67C73" },
				{ 2, "#F4511E"},
				{ 3, "#F6BF26" },
				{ 4, "#33B679" },
				{ 5, "#0B8043" },
				{ 6, "#039BE5" },
				{ 7, "#3F51B5" },
				{ 8, "#7986CB" },
				{ 9, "#8E24AA" },
				{ 10, "#616161" }
			};

			int currentColorIndex = (int)value;
			string currentColor = ColorsTable[currentColorIndex - 1];

			return GetColorFromString(currentColor);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return 1;
		}

		/// <summary>
		/// Get the color brush from string
		/// </summary>
		/// <param name="color">Input string color</param>
		/// <returns>New color <see cref="Brush"/></returns>
		private SolidColorBrush GetColorFromString(string color)
		{
			var colorString = color.Replace("#", string.Empty);

			var r = byte.Parse(colorString.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
			var g = byte.Parse(colorString.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
			var b = byte.Parse(colorString.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

			Windows.UI.Color localColor = Windows.UI.Color.FromArgb(255, r, g, b);
			return new SolidColorBrush(localColor);
		}
	}
}
