using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace TimeTrace.View.Converters
{
	/// <summary>
	/// A class for returning an empty string value as "missing"
	/// </summary>
	public class FromEmptyStringToTextConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			string text = (string)value;
			if (string.IsNullOrEmpty(text))
			{
				ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("FromEmptyStringToTextConverter");
				return resourceLoader.GetString("Absent");
			}

			return text;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
