using System;
using TimeTrace.Model;
using Windows.UI.Xaml.Data;

namespace TimeTrace.View.Converters
{
	public class FromRepeatModeToIntConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			RepeatMode repeatMode = (RepeatMode)value;

			return (int)repeatMode;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			int? selectedValue = (int)value;
			if (selectedValue.HasValue)
			{
				switch (selectedValue)
				{
					case 0: return RepeatMode.NotRepeat;
					case 1: return RepeatMode.EveryDay;
					case 2: return RepeatMode.AfterOneDay;
					case 3: return RepeatMode.WeekOnce;
					case 4: return RepeatMode.MonthOnce;
					case 5: return RepeatMode.YearOnce;
					case 6: return RepeatMode.Custom;

					default:
						break;
				}
			}

			return RepeatMode.NotRepeat;
		}
	}
}
