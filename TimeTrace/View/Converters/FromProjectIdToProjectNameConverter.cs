using System;
using System.Linq;
using TimeTrace.Model.DBContext;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace TimeTrace.View.Converters
{
	public class FromProjectIdToProjectNameConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is string projectId)
			{
				using (var db = new MainDatabaseContext())
				{
					var project = db.Projects.FirstOrDefault(i => i.Id == projectId);
					if (project != null)
					{
						return project.Name;
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
