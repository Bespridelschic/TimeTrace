using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TimeTrace.Model;
using TimeTrace.View.MainView;
using TimeTrace.View.MainView.PersonalMapsCreatePages;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace TimeTrace.ViewModel.MainViewModel
{
	public class HomeViewModel : BaseViewModel
	{
		#region Properties

		public string NullableString = "Unknown";

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

		private ServerUser currentUser;
		/// <summary>
		/// Current using user
		/// </summary>
		public ServerUser CurrentUser
		{
			get { return currentUser; }
			set
			{
				currentUser = value;
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

			CurrentUser = new ServerUser();
			GetUserInfo();
		}

		/// <summary>
		/// Deserialize json string to user and read the information
		/// </summary>
		public async void GetUserInfo()
		{
			try
			{
				CurrentUser = await UserRequests.PostRequestAsync();

				Experience = $"{(int)DateTime.Now.Subtract(DateTime.Parse(CurrentUser.Birthday)).TotalDays} дней";
			}
			catch (Exception ex)
			{
				await (new MessageDialog($"{ex.Message}\nОшибка доступа к серверу", "Проблема соединения")).ShowAsync();
			}
			finally
			{
				if (string.IsNullOrEmpty(CurrentUser.FirstName))
				{
					CurrentUser.FirstName = "Unknown";
					CurrentUser.LastName = "Unknown";
					CurrentUser.MiddleName = "Unknown";
					CurrentUser.Birthday = "01-01-01";
					currentUser.Created_at = DateTime.Parse("01-01-01");
				}
			}
		}

		/// <summary>
		/// Find new avatar in local
		/// </summary>
		public async void AvatarChange()
		{
			var picker = new Windows.Storage.Pickers.FileOpenPicker();
			picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
			picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
			picker.FileTypeFilter.Add(".jpg");
			picker.FileTypeFilter.Add(".jpeg");
			picker.FileTypeFilter.Add(".png");

			Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();

			if (file != null)
			{
				await (new MessageDialog($"Picked photo: {file.Path}").ShowAsync());
			}
			else
			{
				await (new MessageDialog("Operation cancelled").ShowAsync());
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
