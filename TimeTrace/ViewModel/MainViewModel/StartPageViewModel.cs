using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TimeTrace.Model.DBContext;
using TimeTrace.Model.Requests;
using TimeTrace.View.AuthenticationView;
using TimeTrace.View.MainView;
using TimeTrace.View.MainView.ContactPages;
using TimeTrace.View.MainView.PersonalMapsCreatePages;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TimeTrace.ViewModel.MainViewModel
{
	/// <summary>
	/// Singleton of start page ViewModel
	/// </summary>
	public sealed class StartPageViewModel : BaseViewModel
	{
		#region Properties

		private ObservableCollection<string> mapEventsSuggestList;
		/// <summary>
		/// Filter tips
		/// </summary>
		public ObservableCollection<string> MapEventsSuggestList
		{
			get => mapEventsSuggestList;
			set
			{
				mapEventsSuggestList = value;
				OnPropertyChanged();
			}
		}

		private string pageTitle;
		/// <summary>
		/// Title of current page
		/// </summary>
		public string PageTitle
		{
			get { return pageTitle; }
			set
			{
				pageTitle = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Username in command bar
		/// </summary>
		public string CurrentUserName { get; set; }

		/// <summary>
		/// If offine sign in, block internet features
		/// </summary>
		public bool InternetFeaturesEnable { get; set; }

		/// <summary>
		/// Localization resource loader
		/// </summary>
		public ResourceLoader ResourceLoader { get; set; }

		#endregion

		private static readonly Lazy<StartPageViewModel> instance;
		/// <summary>
		/// Getting singleton instance
		/// </summary>
		public static StartPageViewModel Instance
		{
			get
			{
				return instance.Value;
			}
		}

		/// <summary>
		/// Standart static constructor
		/// </summary>
		static StartPageViewModel()
		{
			instance = new Lazy<StartPageViewModel>(() => new StartPageViewModel());
		}

		/// <summary>
		/// Standart constructor
		/// </summary>
		private StartPageViewModel()
		{
			ResourceLoader = ResourceLoader.GetForCurrentView("HomeVM");

			SetHeader(Headers.Home);
			MapEventsSuggestList = new ObservableCollection<string>();

			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
			var currentUser = new Model.User((string)localSettings.Values["email"] ?? ResourceLoader.GetString("/HomeVM/UnknownEmail"), null);
			CurrentUserName = currentUser.Email[0].ToString().ToUpper() + currentUser.Email.Substring(1, currentUser.Email.IndexOf('@') - 1);

			InternetFeaturesEnable = true;
		}

		/// <summary>
		/// Getting last data from server
		/// </summary>
		/// <returns>Result of synchronization</returns>
		public async Task ServerDataSynchronization()
		{
			if (Instance.InternetFeaturesEnable)
			{
				// Contacts synchronization
				await new ContactsViewModel.ContactsViewModel().RefreshContactsAsync();
				Debug.WriteLine("Контакты синхронизированы с сервером");

				// Calendars, projects and map events synchronization
				try
				{
					var resultOfSynchronization = await Model.Requests.InternetRequests.SynchronizationRequestAsync();
					if (resultOfSynchronization == 0)
					{
						ShowToastNotification(0);
					}

					if (resultOfSynchronization == 1)
					{
						ShowToastNotification(1);
					}
				}
				catch (Exception)
				{
					ShowToastNotification(1);
				}

				Debug.WriteLine("Календари, проекты и события синхронизированы с сервером");
			}
		}

		/// <summary>
		/// Regular synchronization of calendars, projects and map events
		/// </summary>
		/// <returns>Result of synchronization</returns>
		public async Task CategoriesSynchronization()
		{
			// Start synchronization if internet connection enabled
			if (InternetRequests.CheckForInternetConnection())
			{
				try
				{
					// Synchronization updated categories
					if (await InternetRequests.SynchronizationRequestAsync() == 1)
					{
						ShowToastNotification(1);
					}
				}
				catch (Exception)
				{
					ShowToastNotification(1);
				}
			}
		}

		/// <summary>
		/// Navigation menu
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="args">Parameter</param>
		public void NavigatingThroughTheMainMenu(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
		{
			if (args.IsSettingsSelected)
			{
				SetHeader(Headers.Settings);
				(Application.Current as App).AppFrame.Navigate(typeof(SettingsPage));
			}

			else
			{
				NavigationViewItem item = args.SelectedItem as NavigationViewItem;

				switch (item.Tag)
				{
					case "home":
						SetHeader(Headers.Home);
						(Application.Current as App).AppFrame.Navigate(typeof(HomePage));
						break;

					case "schedule":
						SetHeader(Headers.Shedule);
						(Application.Current as App).AppFrame.Navigate(typeof(SchedulePage));
						break;

					case "contacts":
						SetHeader(Headers.Contacts);
						(Application.Current as App).AppFrame.Navigate(typeof(ContactsPage));
						break;

					case "personalMaps":
						SetHeader(Headers.MapEvents);
						(Application.Current as App).AppFrame.Navigate(typeof(CategorySelectPage));
						break;

					default:
						break;
				}
			}
		}

		/// <summary>
		/// Target pages
		/// </summary>
		public enum Headers
		{
			Home, Shedule, Contacts, MapEvents, Settings
		}

		/// <summary>
		/// Set NavigationView title
		/// </summary>
		/// <param name="menu">Sender <see cref="NavigationView"/></param>
		/// <param name="header">Target page</param>
		public void SetHeader(Headers header)
		{
			switch (header)
			{
				case Headers.Home:
					PageTitle = ResourceLoader.GetString("/StartVM/Homepage");
					break;
				case Headers.Shedule:
					PageTitle = ResourceLoader.GetString("/StartVM/Shedule");
					break;
				case Headers.Contacts:
					PageTitle = ResourceLoader.GetString("/StartVM/Contacts");
					break;
				case Headers.MapEvents:
					PageTitle = ResourceLoader.GetString("/StartVM/PersonalEvents");
					break;
				case Headers.Settings:
					PageTitle = ResourceLoader.GetString("/StartVM/Settings");
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Send push-notification to windows
		/// </summary>
		private void ShowToastNotification(int res)
		{
			string message = res == 0 ? ResourceLoader.GetString("/StartVM/SynchronizationSuccess") : ResourceLoader.GetString("/StartVM/SynchronizationError");

			var toastContent = new ToastContent()
			{
				Visual = new ToastVisual()
				{
					BindingGeneric = new ToastBindingGeneric()
					{
						Children =
						{
							new AdaptiveText()
							{
								Text = ResourceLoader.GetString("/StartVM/SynchronizationSuccess"),
								HintMaxLines = 1
							},
							new AdaptiveText()
							{
								Text = $"{ResourceLoader.GetString("/StartVM/SynchronizationStart")}: {DateTime.Now.ToShortTimeString()}"
							},
							new AdaptiveText()
							{
								Text = message
							}
						},
						AppLogoOverride = new ToastGenericAppLogo()
						{
							Source = @"Assets/user-48.png",
							HintCrop = ToastGenericAppLogoCrop.Circle
						}
					}
				},
				Launch = "app-defined-string"
			};

			// Create the toast notification
			var toastNotif = new ToastNotification(toastContent.GetXml());

			// And send the notification
			ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
		}

		/// <summary>
		/// Sending message to developer
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="e">Parameter</param>
		public async void Feedback()
		{
			var message = new TextBox()
			{
				Header = ResourceLoader.GetString("/StartVM/FeedbackHeader"),
				PlaceholderText = ResourceLoader.GetString("/StartVM/FeedbackPlaceholder"),
				Height = 200,
				Width = 350,
				TextWrapping = TextWrapping.Wrap,
				AcceptsReturn = true,
				IsSpellCheckEnabled = true
			};

			ContentDialog dialog = new ContentDialog
			{
				Title = ResourceLoader.GetString("/StartVM/FeedbackTitle"),
				Content = message,
				PrimaryButtonText = ResourceLoader.GetString("/StartVM/FeedbackSend"),
				CloseButtonText = ResourceLoader.GetString("/StartVM/FeedbackLater"),
				DefaultButton = ContentDialogButton.Primary,
			};

			if (await dialog.ShowAsync() == ContentDialogResult.Primary)
			{
				if (string.IsNullOrEmpty(message.Text))
				{
					await (new MessageDialog(ResourceLoader.GetString("/StartVM/FeedbackProblem"), ResourceLoader.GetString("/StartVM/FeedbackError"))).ShowAsync();

					Feedback();
				}
				else
				{
					await (new MessageDialog(ResourceLoader.GetString("/StartVM/FeedbackMessage"), ResourceLoader.GetString("/StartVM/FeedbackSuccess"))).ShowAsync();
				}
			}
		}

		/// <summary>
		/// Sign out from system
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="e">Parameter</param>
		public async void SignOut()
		{
			ContentDialog dialog = new ContentDialog
			{
				Title = ResourceLoader.GetString("/StartVM/SignOutHeader"),
				Content = ResourceLoader.GetString("/StartVM/SignOutMessage"),
				PrimaryButtonText = ResourceLoader.GetString("/StartVM/SignOutExit"),
				CloseButtonText = ResourceLoader.GetString("/StartVM/SignOutCancel"),
				DefaultButton = ContentDialogButton.Close
			};

			if (await dialog.ShowAsync() == ContentDialogResult.Primary)
			{
				// Remove local user data settings
				ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
				localSettings.Values.Remove("email");
				localSettings.Values.Remove("lastName");
				localSettings.Values.Remove("firstName");
				localSettings.Values.Remove("middleName");
				localSettings.Values.Remove("birthday");

				if (Window.Current.Content is Frame frame)
				{
					frame.Navigate(typeof(SignInPage));
				}
			}
		}

		#region Searching map events

		/// <summary>
		/// Filtration of input filters
		/// </summary>
		/// <param name="sender">Input filter</param>
		/// <param name="args">Event args</param>
		public void MapEventsFilter(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (args.CheckCurrent())
			{
				MapEventsSuggestList.Clear();
			}

			// Remove all contacts for adding relevant filter
			MapEventsSuggestList.Clear();

			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

			// Select all contacts
			if (string.IsNullOrEmpty(sender.Text))
			{
				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					foreach (var i in db.MapEvents
						.Where(i => i.EmailOfOwner == (string)localSettings.Values["email"]
							&& !i.IsDelete
							&& i.End >= DateTime.UtcNow)
						.Select(i => i))
					{
						MapEventsSuggestList.Add(i.Name);
					}
				}
			}

			else
			{
				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					foreach (var i in db.MapEvents
						.Where(i => (i.Name.ToLowerInvariant().Contains(sender.Text.ToLowerInvariant()))
							&& i.EmailOfOwner == (string)localSettings.Values["email"]
							&& !i.IsDelete
							&& i.End >= DateTime.UtcNow)
						.Select(i => i))
					{
						if (!MapEventsSuggestList.Contains(i.Name))
						{
							MapEventsSuggestList.Add(i.Name);
						}
					}
				}
			}
		}

		/// <summary>
		/// Click on Find map event
		/// </summary>
		/// <param name="sender">Object</param>
		/// <param name="args">Args</param>
		public void MapEventsFilterQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			// Remove term from search
			sender.Text = string.Empty;

			MapEventsSuggestList.Clear();

			if (string.IsNullOrEmpty(args.QueryText))
			{
				return;
			}

			if (MapEventsSuggestList != null)
			{
				MapEventsSuggestList.Clear();

				(Application.Current as App).AppFrame.Navigate(typeof(SchedulePage), args.QueryText.ToLowerInvariant());
			}
		}

		#endregion
	}
}