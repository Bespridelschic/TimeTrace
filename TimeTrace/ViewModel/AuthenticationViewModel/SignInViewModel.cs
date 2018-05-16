using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TimeTrace.Model;
using TimeTrace.View.MainView;
using TimeTrace.View.AuthenticationView.SignUp;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using TimeTrace.Model.Requests;

namespace TimeTrace.ViewModel.AuthenticationViewModel
{
	/// <summary>
	/// ViewModel for sign in
	/// </summary>
	public class SignInViewModel : BaseViewModel
	{
		#region Properties

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

		private bool processing;
		/// <summary>
		/// ProgressRing
		/// </summary>
		public bool Processing
		{
			get => processing;
			set
			{
				processing = value;
				OnPropertyChanged();
			}
		}

		private int selectionStart;
		/// <summary>
		/// Start cursor position in a text
		/// </summary>
		public int SelectionStart
		{
			get => selectionStart;
			set
			{
				selectionStart = value;
				OnPropertyChanged();
			}
		}

		private bool isPasswordSave;
		/// <summary>
		/// Status of the save password flag
		/// </summary>
		public bool IsPasswordSave
		{
			get => isPasswordSave;
			set
			{
				isPasswordSave = value;
				OnPropertyChanged();
			}
		}

		private bool controlEnable;
		/// <summary>
		/// Status of enabled interface controls
		/// </summary>
		public bool ControlEnable
		{
			get => controlEnable;
			set
			{
				controlEnable = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Localization resource loader
		/// </summary>
		public ResourceLoader ResourceLoader { get; set; }

		#endregion

		/// <summary>
		/// Initialize object <see cref="User"/> and try read information from local storage
		/// </summary>
		public SignInViewModel()
		{
			CurrentUser = new User();
			ResourceLoader = ResourceLoader.GetForCurrentView("SignInUp");
			CurrentUser.LoadUserFromFileAsync().GetAwaiter();

			Processing = false;
			IsPasswordSave = true;
			ControlEnable = true;

			SelectionStart = CurrentUser.Email.Length;
		}

		/// <summary>
		/// Check fields correct
		/// </summary>
		/// <returns>Do the fields satisfy business logic</returns>
		private async Task<bool> CanAppSignInAsync()
		{
			if (CurrentUser.Email.Length == 0 || CurrentUser.Password.Length == 0)
			{
				await new MessageDialog(ResourceLoader.GetString("/SignInUp/FillAllFieldsText"),
					ResourceLoader.GetString("/SignInUp/OperationFailedText")).ShowAsync();

				return false;
			}

			if (!CurrentUser.EmailCorrectCheck())
			{
				await new MessageDialog(ResourceLoader.GetString("/SignInUp/IncorrectEmailText"),
					ResourceLoader.GetString("/SignInUp/OperationFailedText")).ShowAsync();

				return false;
			}

			if (CurrentUser.Password.Length < 8)
			{
				await new MessageDialog(ResourceLoader.GetString("/SignInUp/PasswordLengthShortText"),
					ResourceLoader.GetString("/SignInUp/OperationFailedText")).ShowAsync();

				return false;
			}

			return true;
		}

		/// <summary>
		/// Sign in operation
		/// </summary>
		public async Task AppSignInAsync()
		{
			CurrentUser.Email = CurrentUser.Email.Trim();

			var canAppSignInResult = await CanAppSignInAsync();

			if (!canAppSignInResult)
			{
				return;
			}
			
			ControlEnable = false;
			Processing = true;

			try
			{
				if (!InternetRequests.CheckForInternetConnection())
				{
					if (await CurrentUser.CanAuthorizationWithoutInternetAsync())
					{
						ContentDialog signInAccept = new ContentDialog()
						{
							Title = "Хотите войти локально?",
							Content = "Мы обнаружили, что вы ранее входили под этой учётной записью с этого компьютера." +
									"\nХотите войти под своей учётной записью в автономном режиме?\n\nНекоторые функции будут недоступны!",
							PrimaryButtonText = "Войти",
							CloseButtonText = "Отмена",
							DefaultButton = ContentDialogButton.Primary
						};

						var signInAcceptResult = await signInAccept.ShowAsync();

						if (signInAcceptResult == ContentDialogResult.Primary)
						{
							// Save user local data for using after sign in
							ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
							localSettings.Values["email"] = CurrentUser.Email.ToLower();

							// Set current user name into title bar
							var tempCurrentUser = new User((string)localSettings.Values["email"] ?? "Неизвестный@gmail.com", null);
							MainViewModel.StartPageViewModel.Instance.CurrentUserName = tempCurrentUser.Email[0].ToString().ToUpper() + tempCurrentUser.Email.Substring(1, tempCurrentUser.Email.IndexOf('@') - 1);

							if (Window.Current.Content is Frame frame)
							{
								// Navigation to the start page, and disabling internet features
								frame.Navigate(typeof(StartPage), false);
							}
						}

						return;
					}

					await new MessageDialog("Вход не возможен. Проверьте своё интернет подключение", "Нет доступа к интернету").ShowAsync();
					return;
				}

				var requestResult = await InternetRequests.PostRequestAsync(InternetRequests.PostRequestDestination.SignIn, CurrentUser);

				switch (requestResult)
				{
					case 0:
						{
							// Save user hash to file
							await CurrentUser.SaveUserHashToFileAsync();

							// Save login to file
							if (IsPasswordSave)
							{
								await CurrentUser.SaveUserToFileAsync();
							}
							else
							{
								await CurrentUser.RemoveUserDataFromFilesAsync();
							}

							// Save user local data for using after sign in
							ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
							localSettings.Values["email"] = CurrentUser.Email.ToLower();

							// Set current user name into title bar
							var tempCurrentUser = new User((string)localSettings.Values["email"] ?? "Неизвестный@gmail.com", null);
							MainViewModel.StartPageViewModel.Instance.CurrentUserName = tempCurrentUser.Email[0].ToString().ToUpper() + tempCurrentUser.Email.Substring(1, tempCurrentUser.Email.IndexOf('@') - 1);

							if (Window.Current.Content is Frame frame)
							{
								frame.Navigate(typeof(StartPage));
							}

							break;
						}
					case 1:
						{
							await (new MessageDialog($"{ResourceLoader.GetString("/SignInUp/NoMatchesFoundText")}" +
								$"{ResourceLoader.GetString("/SignInUp/CheckCorrectionText")}",
								ResourceLoader.GetString("/SignInUp/SignInErrorText"))).ShowAsync();

							break;
						}
					case 2:
						{
							await ConfirmAccountDialogAsync();

							if (IsPasswordSave)
							{
								await CurrentUser.SaveUserToFileAsync();
							}
							else
							{
								await CurrentUser.RemoveUserDataFromFilesAsync();
							}

							break;
						}
					default:
						{
							await (new MessageDialog(ResourceLoader.GetString("/SignInUp/ServerConnectionProblemText"),
								ResourceLoader.GetString("/SignInUp/SignInErrorText"))).ShowAsync();

							break;
						}
				}
			}
			catch (Exception ex)
			{
				await (new MessageDialog($"{ex.Message}\n" +
					$"{ResourceLoader.GetString("/SignInUp/ServerConnectionProblemText")}",
					ResourceLoader.GetString("/SignInUp/SignInErrorText"))).ShowAsync();
			}
			finally
			{
				ControlEnable = true;
				Processing = false;
			}
		}

		/// <summary>
		/// Registration page navigate
		/// </summary>
		public void SignUp()
		{
			if (Window.Current.Content is Frame frame)
			{
				frame.Navigate(typeof(SignUpPage), CurrentUser);
			}
		}

		/// <summary>
		/// Password recovery operation
		/// </summary>
		public async void UserPasswordRecoveryAsync()
		{
			string currentPassword = CurrentUser.Password;
			string currentEmail = new string(CurrentUser.Email.ToCharArray());

			#region Dialog window

			TextBox emailTextBox = new TextBox
			{
				PlaceholderText = ResourceLoader.GetString("/SignInUp/EmailBoxRegistrationPlaceholderText"),
				Header = ResourceLoader.GetString("/SignInUp/EmailBoxRegistrationHeader"),
				Text = CurrentUser.Email
			};

			PasswordBox passwordTextBox = new PasswordBox
			{
				PlaceholderText = ResourceLoader.GetString("/SignInUp/NewPasswordPlaceholderText"),
				Header = ResourceLoader.GetString("/SignInUp/NewPasswordHeader"),
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
				Title = ResourceLoader.GetString("/SignInUp/PasswordRecoveryText"),
				Content = grid,
				PrimaryButtonText = ResourceLoader.GetString("/SignInUp/RecoveryButtonText"),
				CloseButtonText = ResourceLoader.GetString("/SignInUp/LaterButtonText"),
				DefaultButton = ContentDialogButton.Primary,
			};

			if (await dialog.ShowAsync() == ContentDialogResult.Primary)
			{
				try
				{
					CurrentUser.Email = emailTextBox.Text;
					CurrentUser.Password = passwordTextBox.Password;

					var CanAppSignInResult = await CanAppSignInAsync();
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
								await (new MessageDialog(ResourceLoader.GetString("/SignInUp/ActivationInstructionText"),
									ResourceLoader.GetString("/SignInUp/ChangePasswordText"))).ShowAsync();
								break;
							}
						case 1:
							{
								await (new MessageDialog(ResourceLoader.GetString("/SignInUp/CantChangePasswordText"),
									ResourceLoader.GetString("/SignInUp/ChangePasswordText"))).ShowAsync();
								break;
							}
						default:
							{
								await (new MessageDialog(ResourceLoader.GetString("/SignInUp/UndefinedErrorText"),
									ResourceLoader.GetString("/SignInUp/ChangePasswordText"))).ShowAsync();
								break;
							}
					}
				}
				catch (Exception ex)
				{
					await (new MessageDialog($"{ex.Message}\n" +
						$"{ResourceLoader.GetString("/SignInUp/UndefinedErrorText")}", ResourceLoader.GetString("/SignInUp/OperationFailedText"))).ShowAsync();
				}
			}
		}

		/// <summary>
		/// Account confirm dialog message
		/// </summary>
		/// <returns>Account state</returns>
		private async Task ConfirmAccountDialogAsync()
		{
			TextBox textBox = new TextBox
			{
				Text = CurrentUser.Email,
				IsReadOnly = true,
				Width = 300,
				Height = 33
			};

			ContentDialog dialog = new ContentDialog
			{
				Title = ResourceLoader.GetString("/SignInUp/AccountActivateText"),
				Content = textBox,
				PrimaryButtonText = ResourceLoader.GetString("/SignInUp/GetKeyButtonText"),
				CloseButtonText = ResourceLoader.GetString("/SignInUp/LaterButtonText"),
				DefaultButton = ContentDialogButton.Primary,
			};

			if (await dialog.ShowAsync() == ContentDialogResult.Primary)
			{
				try
				{
					var requestResult = await InternetRequests.PostRequestAsync(InternetRequests.PostRequestDestination.AccountActivation, CurrentUser);

					switch (requestResult)
					{
						case 0:
							{
								await (new MessageDialog(ResourceLoader.GetString("/SignInUp/ActivationInstructionText"),
									ResourceLoader.GetString("/SignInUp/AccountVerificationText"))).ShowAsync();
								break;
							}
						case 1:
							{
								await (new MessageDialog(ResourceLoader.GetString("/SignInUp/CantActivateAccountText"),
									ResourceLoader.GetString("/SignInUp/AccountVerificationText"))).ShowAsync();
								break;
							}
						default:
							{
								await (new MessageDialog(ResourceLoader.GetString("/SignInUp/UndefinedErrorText"),
									ResourceLoader.GetString("/SignInUp/AccountVerificationText"))).ShowAsync();
								break;
							}
					}
				}
				catch (Exception ex)
				{
					await (new MessageDialog($"{ex.Message}\n" +
						$"{ResourceLoader.GetString("/SignInUp/UndefinedErrorText")}",
						ResourceLoader.GetString("/SignInUp/SignInErrorText"))).ShowAsync();
				}
			}
		}
	}
}