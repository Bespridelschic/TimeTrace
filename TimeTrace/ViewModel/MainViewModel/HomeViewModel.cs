using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TimeTrace.View.MainView;
using TimeTrace.View.MainView.PersonalMapsCreatePages;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace TimeTrace.ViewModel.MainViewModel
{
	public class HomeViewModel : BaseViewModel
	{
		#region Properties

		private string experience;
		/// <summary>
		/// Total user experience time
		/// </summary>
		public string Experience
		{
			get { return experience; }
			set
			{
				experience = value;
				OnPropertyChanged();
			}
		}

		private string userName;
		/// <summary>
		/// Home Username view
		/// </summary>
		public string UserName
		{
			get { return userName; }
			set
			{
				userName = value;
				OnPropertyChanged();
			}
		}

		private int numContacts;
		/// <summary>
		/// Number of contacts
		/// </summary>
		public int NumContacts
		{
			get { return numContacts; }
			set
			{
				numContacts = value;
				OnPropertyChanged();
			}
		}

		private int numEvents;
		/// <summary>
		/// Total number of created events
		/// </summary>
		public int NumEvents
		{
			get { return numEvents; }
			set
			{
				numEvents = value;
				OnPropertyChanged();
			}
		}

		public Frame Frame { get; set; }

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		public HomeViewModel()
		{
			NumContacts = 0;
			NumEvents = 0;
			Experience = "0";

			GetUserInfo();
		}

		/// <summary>
		/// Deserialize json string to user and read the information
		/// </summary>
		public async void GetUserInfo()
		{
			//Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en-US";
			try
			{
				var userData = await Model.UserRequests.PostRequestAsync();
				JObject jsonString = JObject.Parse(userData);

				NumContacts = (int?)jsonString["status"] ?? 0;
				var customer = (string)jsonString["customer"];

				JObject customerString = JObject.Parse(customer);

				Experience = $"{(int)DateTime.Now.Subtract((DateTime)customerString["created_at"]).TotalDays} дней";
				UserName = (string)customerString["firstName"] ?? ((string)customerString["email"]).Substring(0, ((string)customerString["email"]).IndexOf('@'));
			}
			catch (Exception ex)
			{
				await (new MessageDialog($"{ex.Message}\nОшибка доступа к серверу", "Проблема соединения")).ShowAsync();
			}
			finally
			{
				if (string.IsNullOrEmpty(UserName))
				{
					Experience = "0 дней";
					UserName = "Unknown user";
				}
			}
		}

		/// <summary>
		/// Navigate to schedule page
		/// </summary>
		public void SchedulePageChoice()
		{
			Frame.Navigate(typeof(SchedulePage));
		}

		/// <summary>
		/// Navigate to personal maps create page
		/// </summary>
		public void PersonalMapsPageChoice()
		{
			Frame.Navigate(typeof(PersonalMapsPage), Frame);
		}

		/// <summary>
		/// Navigate to settings page
		/// </summary>
		public void SettingsPageChoice()
		{
			Frame.Navigate(typeof(SettingsPage));
		}
	}
}
