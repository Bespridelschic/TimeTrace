using System;
using Windows.UI.Xaml.Data;

namespace TimeTrace.View.Converters
{
	public class FromUtcToLocalTimeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is DateTime utcDateTime)
			{
				return utcDateTime.ToLocalTime().ToString();
			}

			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (value is DateTime localDateTime)
			{
				return localDateTime.ToUniversalTime().ToString();
			}

			return value;
		}
	}
}
