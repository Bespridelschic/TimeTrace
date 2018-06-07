using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace TimeTrace.View.Converters
{
	public class FromIntToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			int? temp = (int)value;
			if (temp.HasValue)
			{
				if (temp.Value == 0)
				{
					return Visibility.Collapsed;
				}
				else
				{
					return Visibility.Visible;
				}
			}

			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
