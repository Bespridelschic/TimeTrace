using System;
using Windows.UI.Xaml.Data;

namespace TimeTrace.View.Converters
{
	/// <summary>
	/// Converter from int to bool value
	/// </summary>
	public class FromIntToBoolConverter : IValueConverter
	{
		/// <summary>
		/// Converting from <see langword="int"/> to <see langword="bool"/>
		/// </summary>
		/// <param name="value">Convertible value</param>
		/// <param name="targetType">Target</param>
		/// <param name="parameter">Sended parameter</param>
		/// <param name="language">Current language</param>
		/// <returns>If value is 0 - false, else true</returns>
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is int temp)
			{
				if (temp != 0)
				{
					return true;
				}
			}

			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
