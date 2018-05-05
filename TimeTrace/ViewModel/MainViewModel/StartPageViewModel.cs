using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TimeTrace.Model.DBContext;
using TimeTrace.View.AuthenticationView;
using TimeTrace.View.MainView;
using TimeTrace.View.MainView.ContactPages;
using TimeTrace.View.MainView.PersonalMapsCreatePages;
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
		public string CurrentUserName { get; private set; }

		#endregion

		private static readonly Lazy<StartPageViewModel> instance = new Lazy<StartPageViewModel>(() => new StartPageViewModel());
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
		/// Standart constructor
		/// </summary>
		private StartPageViewModel()
		{
			SetHeader(Headers.Home);
			MapEventsSuggestList = new ObservableCollection<string>();

			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
			var currentUser = new Model.User((string)localSettings.Values["email"] ?? "Неизвестный@gmail.com", null);
			CurrentUserName = currentUser.Email[0].ToString().ToUpper() + currentUser.Email.Substring(1, currentUser.Email.IndexOf('@') - 1);
		}

		/// <summary>
		/// Navigation menu
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="args">Parameter</param>
		public async void NavigatingThroughTheMainMenu(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
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

					case "scheduleSync":
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

						break;

					case "help":
						sender.Header = "Помощь";
						await new MessageDialog("Помощь пользователя находится в разработке").ShowAsync();
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
					PageTitle = "Домашняя страница";
					break;
				case Headers.Shedule:
					PageTitle = "Расписание";
					break;
				case Headers.Contacts:
					PageTitle = "Контакты";
					break;
				case Headers.MapEvents:
					PageTitle = "Персональные карты";
					break;
				case Headers.Settings:
					PageTitle = "Настройки";
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
			string message = res == 0 ? "Синхронизация завершена успешно" : "Ошибка во время синхронизации. Попробуйте перезайти в аккаунт";

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
								Text = "Синхронизация событий",
								HintMaxLines = 1
							},
							new AdaptiveText()
							{
								Text = $"Начало синхронизации: {DateTime.Now.ToShortTimeString()}"
							},
							new AdaptiveText()
							{
								Text = message
							}
						},
						AppLogoOverride = new ToastGenericAppLogo()
						{
							//Source = "https://picsum.photos/48?image=883",
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
		public async void Feedback(object sender, RoutedEventArgs e)
		{
			var message = new TextBox()
			{
				Header = "Расскажите, с какими проблемами вы столкнулись ?",
				PlaceholderText = "Опишите проблему...",
				Height = 200,
				Width = 350,
				TextWrapping = TextWrapping.Wrap,
				AcceptsReturn = true,
				IsSpellCheckEnabled = true
			};

			ContentDialog dialog = new ContentDialog
			{
				Title = "Обратная связь",
				Content = message,
				PrimaryButtonText = "Отправить",
				CloseButtonText = "Отмена",
				DefaultButton = ContentDialogButton.Primary,
			};

			if (await dialog.ShowAsync() == ContentDialogResult.Primary)
			{
				if (string.IsNullOrEmpty(message.Text))
				{
					await (new MessageDialog("Вы не можете отправить пустое сообщение!", "Ошибка")).ShowAsync();

					Feedback(sender, e);
				}
				else
				{
					await (new MessageDialog("Благодарим Вас за вашу поддержку", "Сообщение успешно отправлено")).ShowAsync();
				}
			}
		}

		/// <summary>
		/// Sign out from system
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="e">Parameter</param>
		public async void SignOut(object sender, RoutedEventArgs e)
		{
			ContentDialog dialog = new ContentDialog
			{
				Title = "Подтверждение",
				Content = "Вы уверены что хотите выйти из системы?",
				PrimaryButtonText = "Выйти",
				CloseButtonText = "Отмена",
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
					foreach (var i in db.MapEvents.Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete).Select(i => i))
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
						.Where(i => (i.Name.ToLowerInvariant().Contains(sender.Text.ToLowerInvariant())) &&
									i.EmailOfOwner == (string)localSettings.Values["email"] &&
									!i.IsDelete)
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