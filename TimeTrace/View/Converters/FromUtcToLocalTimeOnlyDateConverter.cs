using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace TimeTrace.View.Converters
{
	/// <summary>
	/// Convert date to local and get short date string
	/// </summary>
	public class FromUtcToLocalTimeOnlyDateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			ResourceLoader ResourceLoader = ResourceLoader.GetForCurrentView("FromUtcToLocalTimeOnlyDateConverter");

			DateTime? date = (DateTime?)value;
			if (date.HasValue)
			{
				if (date.Value.ToLocalTime().Date == DateTime.Now.Date)
				{
					return ResourceLoader.GetString("Today");
				}
				else
				{
					return date.Value.ToLocalTime().Date.ToLongDateString();
				}
			}
			else
			{
				return ResourceLoader.GetString("Undefined");
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
