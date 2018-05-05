using System;
using System.Diagnostics;
using System.Linq;
using TimeTrace.Model;
using TimeTrace.Model.DBContext;
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

		private string nearEvent;
		/// <summary>
		/// Number of contacts
		/// </summary>
		public string NearEvent
		{
			get => nearEvent;
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
			get => numEvents;
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
			get => currentUser;
			set
			{
				currentUser = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Current user email for home page without '@' char
		/// </summary>
		public string CurrentUserEmail { get; set; }

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		public HomeViewModel()
		{
			StartPageViewModel.Instance.SetHeader(StartPageViewModel.Headers.Home);

			CurrentUser = GetUserInfo();
			CurrentUserEmail = CurrentUser.Email[0].ToString().ToUpper() + CurrentUser.Email.Substring(1, CurrentUser.Email.IndexOf('@') - 1);

			NearEvent = "Совсем скоро";

			using (MainDatabaseContext db = new MainDatabaseContext())
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
				(string)localSettings.Values["email"] ?? "Неизвестный@gmail.com",
				null
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
