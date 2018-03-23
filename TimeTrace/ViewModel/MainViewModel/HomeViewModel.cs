using System;
using System.Diagnostics;
using System.Linq;
using TimeTrace.Model;
using TimeTrace.Model.Events.DBContext;
using TimeTrace.View.MainView;
using TimeTrace.View.MainView.PersonalMapsCreatePages;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
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

		private string nearEvent;
		/// <summary>
		/// Number of contacts
		/// </summary>
		public string NearEvent
		{
			get { return nearEvent; }
			set
			{
				nearEvent = value;
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

		private int numEventsToday;
		/// <summary>
		/// Number of events today
		/// </summary>
		public int NumEventsToday
		{
			get => numEventsToday;
			set
			{
				numEventsToday = value;
				OnPropertyChanged();
			}
		}

		private User currentUser;
		/// <summary>
		/// Current using user
		/// </summary>
		public User CurrentUser
		{
			get { return currentUser; }
			set
			{
				currentUser = value;
				OnPropertyChanged();
			}
		}

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		public HomeViewModel()
		{
			CurrentUser = GetUserInfo();
			NearEvent = "Совсем скоро";
			Experience = "Удалить поле";

			using (MapEventContext db = new MapEventContext())
			{
				NumEvents = db.MapEvents.Count(mapEvent => !mapEvent.IsDelete && mapEvent.EmailOfOwner == CurrentUser.Email);
				NumEventsToday = db.MapEvents.
					Count(mapEvent => mapEvent.Start.Date <= DateTime.Today && mapEvent.End.Date >= DateTime.Today && !mapEvent.IsDelete && mapEvent.EmailOfOwner == CurrentUser.Email);

				NearEvent = 
					db.MapEvents.
						Where(i => !i.IsDelete && i.EmailOfOwner == CurrentUser.Email).
						FirstOrDefault(i => i.Start >= DateTime.Now)?.Start.Subtract(DateTime.Now).ToString("g").Split(',')[0]
						?? "Нет ближайших";
			}
		}

		/// <summary>
		/// Read user information from local setting
		/// </summary>
		public User GetUserInfo()
		{
			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

			return new User(
				(string)localSettings.Values["email"] ?? "Неизвестный",
				null,
				(string)localSettings.Values["firstName"] ?? "Не указано",
				(string)localSettings.Values["lastName"] ?? "Не указано",
				(string)localSettings.Values["middleName"] ?? "Не указано",
				(!string.IsNullOrEmpty((string)localSettings.Values["birthday"]))
						? DateTime.Parse((string)localSettings.Values["birthday"]).ToLongDateString()
						: "Не известно"
				);
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

			StorageFile file = await picker.PickSingleFileAsync();

			if (file != null)
			{
				await (new MessageDialog($"Picked photo: {file.Path}").ShowAsync());
			}
		}

		/// <summary>
		/// Navigate to schedule page
		/// </summary>
		public void SchedulePageChoice()
		{
			(Application.Current as App).AppFrame.Navigate(typeof(SchedulePage));
		}

		/// <summary>
		/// Navigate to personal maps create page
		/// </summary>
		public void PersonalMapsPageChoice()
		{
			(Application.Current as App).AppFrame.Navigate(typeof(CategorySelectPage));
		}

		/// <summary>
		/// Navigate to settings page
		/// </summary>
		public void SettingsPageChoice()
		{
			(Application.Current as App).AppFrame.Navigate(typeof(SettingsPage));
		}
	}
}
