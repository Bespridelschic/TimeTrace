using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace TimeTrace.View.Converters
{
	/// <summary>
	/// List converter for int to int?
	/// </summary>
	public class FromNullableIntToIntConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			int? targer = (int?)value;
			if (targer.HasValue)
			{
				return targer.Value;
			}
			else
			{
				return -1;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			int? targer = (int?)value;
			if (targer.HasValue)
			{
				return targer.Value;
			}
			else
			{
				return -1;
			}
		}
	}
}
