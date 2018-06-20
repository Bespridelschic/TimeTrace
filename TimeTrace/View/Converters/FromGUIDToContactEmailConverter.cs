using System;
using System.Linq;
using TimeTrace.Model.DBContext;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace TimeTrace.View.Converters
{
	public class FromGUIDToContactEmailConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is string email)
			{
				using (var db = new MainDatabaseContext())
				{
					var contact = db.Contacts.FirstOrDefault(i => i.Id == email);
					if (contact != null)
					{
						return contact.Email;
					}
				}
			}

			ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("FromGUIDToContactEmailConverter");
			return resourceLoader.GetString("Undefined");
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
