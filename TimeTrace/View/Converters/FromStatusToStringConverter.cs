using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace TimeTrace.View.Converters
{
	public class FromStatusToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			int code = (int)value;
			ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("FromStatusToStringConverter");

			switch (code)
			{
				case 0: return resourceLoader.GetString("NotViewed");
				case 1: return resourceLoader.GetString("Accepted");
				case 2: return resourceLoader.GetString("Rejected");

				default: return resourceLoader.GetString("Undefined");
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
