using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TimeTrace.Model;
using TimeTrace.Model.DBContext;
using TimeTrace.Model.Requests;
using TimeTrace.View.MainView;
using TimeTrace.View.MainView.PersonalMapsCreatePages;
using Windows.ApplicationModel.Resources;
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

		/// <summary>
		/// If offine sign in, block internet features
		/// </summary>
		public bool InternetFeaturesEnable { get; set; }

		/// <summary>
		/// Localization resource loader
		/// </summary>
		public ResourceLoader ResourceLoader { get; set; }

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		public HomeViewModel()
		{
			ResourceLoader = ResourceLoader.GetForCurrentView("HomeVM");

			StartPageViewModel.Instance.SetHeader(StartPageViewModel.Headers.Home);
			InternetFeaturesEnable = StartPageViewModel.Instance.InternetFeaturesEnable;

			CurrentUser = GetUserInfo();
			CurrentUserEmail = CurrentUser.Email[0].ToString().ToUpper() + CurrentUser.Email.Substring(1, CurrentUser.Email.IndexOf('@') - 1);

			NearEvent = ResourceLoader.GetString("/HomeVM/NearEvent");

			using (MainDatabaseContext db = new MainDatabaseContext())
			{
				NumEvents = db.MapEvents.Count(mapEvent => !mapEvent.IsDelete && mapEvent.EmailOfOwner == CurrentUser.Email);
				NumEventsToday = db.MapEvents.
					Count(mapEvent => mapEvent.Start.Date <= DateTime.Today && mapEvent.End.Date >= DateTime.Today && !mapEvent.IsDelete && mapEvent.EmailOfOwner == CurrentUser.Email);

				NearEvent =
					db.MapEvents.
						Where(i => !i.IsDelete && i.EmailOfOwner == CurrentUser.Email).
						FirstOrDefault(i => i.Start >= DateTime.Now)?.Start.Subtract(DateTime.Now).ToString("g").Split(',')[0]
						?? ResourceLoader.GetString("/HomeVM/NoClosest");
			}
		}

		/// <summary>
		/// Read user information from local setting
		/// </summary>
		public User GetUserInfo()
		{
			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

			return new User(
				(string)localSettings.Values["email"] ?? ResourceLoader.GetString("/HomeVM/UnknownEmail"),
				null
				);
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

		/// <summary>
		/// User feedback
		/// </summary>
		public void Feedback()
		{
			StartPageViewModel.Instance.Feedback();
		}

		/// <summary>
		/// Password change
		/// </summary>
		public async void UserPasswordResetAsync()
		{
			string currentPassword = string.Empty;
			string currentEmail = CurrentUser.Email;

			#region Dialog window

			TextBox emailTextBox = new TextBox
			{
				Header = ResourceLoader.GetString("/HomeVM/YourEmail"),
				Text = CurrentUser.Email,
				IsReadOnly = true
			};

			PasswordBox passwordTextBox = new PasswordBox
			{
				PlaceholderText = ResourceLoader.GetString("/HomeVM/NewPasswordPlaceholderText"),
				Header = ResourceLoader.GetString("/HomeVM/NewPasswordHeader"),
				MaxLength = 20,
			};

			Grid grid = new Grid();
			RowDefinition row1 = new RowDefinition();
			RowDefinition row2 = new RowDefinition();
			RowDefinition row3 = new RowDefinition();

			row1.Height = new GridLength(0, GridUnitType.Auto);
			row2.Height = new GridLength(10);
			row3.Height = new GridLength(0, GridUnitType.Auto);

			grid.RowDefinitions.Add(row1);
			grid.RowDefinitions.Add(row2);
			grid.RowDefinitions.Add(row3);

			grid.Children.Add(emailTextBox);
			grid.Children.Add(passwordTextBox);

			Grid.SetRow(emailTextBox, 0);
			Grid.SetRow(passwordTextBox, 2);

			#endregion

			ContentDialog dialog = new ContentDialog
			{
				Title = ResourceLoader.GetString("/HomeVM/ChangePassword"),
				Content = grid,
				PrimaryButtonText = ResourceLoader.GetString("/HomeVM/ApplyChanging"),
				CloseButtonText = ResourceLoader.GetString("/HomeVM/ChangeLater"),
				DefaultButton = ContentDialogButton.Primary,
			};

			if (await dialog.ShowAsync() == ContentDialogResult.Primary)
			{
				try
				{
					CurrentUser.Email = emailTextBox.Text;
					CurrentUser.Password = passwordTextBox.Password;

					var CanAppSignInResult = await CanPasswordResetAsync();
					if (!CanAppSignInResult)
					{
						CurrentUser.Password = currentPassword;
						CurrentUser.Email = currentEmail;
						return;
					}

					var requestResult = await InternetRequests.PostRequestAsync(InternetRequests.PostRequestDestination.PasswordReset, CurrentUser);

					switch (requestResult)
					{
						case 0:
							{
								await (new MessageDialog(ResourceLoader.GetString("/HomeVM/ActivationInstruction"),
									ResourceLoader.GetString("/HomeVM/ChangePassword"))).ShowAsync();
								break;
							}
						case 1:
							{
								await (new MessageDialog(ResourceLoader.GetString("/HomeVM/CantChangePassword"),
									ResourceLoader.GetString("/HomeVM/ChangePassword"))).ShowAsync();
								break;
							}
						default:
							{
								await (new MessageDialog(ResourceLoader.GetString("/HomeVM/UndefinedError"),
									ResourceLoader.GetString("/HomeVM/ChangePassword"))).ShowAsync();
								break;
							}
					}
				}
				catch (Exception ex)
				{
					await (new MessageDialog($"{ex.Message}\n" +
						$"{ResourceLoader.GetString("/HomeVM/UndefinedErrorText")}", ResourceLoader.GetString("/HomeVM/OperationFailedText"))).ShowAsync();
				}
			}
		}

		/// <summary>
		/// Check fields correct
		/// </summary>
		/// <returns>Do the fields satisfy business logic</returns>
		private async Task<bool> CanPasswordResetAsync()
		{
			if (CurrentUser.Email.Length == 0 || CurrentUser.Password.Length == 0)
			{
				await new MessageDialog(ResourceLoader.GetString("/HomeVM/FillAllFields"),
					ResourceLoader.GetString("/HomeVM/FailedText")).ShowAsync();

				return false;
			}

			if (!CurrentUser.EmailCorrectCheck())
			{
				await new MessageDialog(ResourceLoader.GetString("/HomeVM/IncorrectEmail"),
					ResourceLoader.GetString("/HomeVM/FailedText")).ShowAsync();

				return false;
			}

			if (CurrentUser.Password.Length < 8)
			{
				await new MessageDialog(ResourceLoader.GetString("/HomeVM/PasswordLengthShort"),
					ResourceLoader.GetString("/HomeVM/FailedText")).ShowAsync();

				return false;
			}

			return true;
		}
	}
}
