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
			get { return currentUser; }
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
			get { return processing; }
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
			get { return selectionStart; }
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
			get { return isPasswordSave; }
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
			get { return controlEnable; }
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
			UserFileWorker.LoadUserFromFileAsync(CurrentUser).GetAwaiter();

			Processing = false;
			IsPasswordSave = true;
			ControlEnable = true;

			SelectionStart = CurrentUser.Email.Length;
		}

		/// <summary>
		/// Check fields correct
		/// </summary>
		/// <returns>Удовлетворяют ли поля бизнес-логике</returns>
		private async Task<bool> CanAppSignIn()
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
		public async void AppSignIn()
		{
			var CanAppSignInResult = await CanAppSignIn();

			if (!CanAppSignInResult)
			{
				return;
			}
			
			ControlEnable = false;
			Processing = true;

			try
			{
				var requestResult = await UserRequests.PostRequestAsync(UserRequests.PostRequestDestination.SignIn, CurrentUser);

				switch (requestResult)
				{
					case 0:
						{
							if (IsPasswordSave)
							{
								await UserFileWorker.SaveUserToFileAsync(CurrentUser);
							}
							else
							{
								await UserFileWorker.RemoveUserDataFromFilesAsync();
							}

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
								await UserFileWorker.SaveUserToFileAsync(CurrentUser);
							}
							else
							{
								await UserFileWorker.RemoveUserDataFromFilesAsync();
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
				frame.Navigate(typeof(SignUpExtendPage), CurrentUser.Email);
			}
		}

		/// <summary>
		/// Password recovery operation
		/// </summary>
		public async void UserPasswordRecovery()
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

					var CanAppSignInResult = await CanAppSignIn();
					if (!CanAppSignInResult)
					{
						CurrentUser.Password = currentPassword;
						CurrentUser.Email = currentEmail;
						return;
					}

					var requestResult = await UserRequests.PostRequestAsync(UserRequests.PostRequestDestination.PasswordReset, CurrentUser);

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
					var requestResult = await UserRequests.PostRequestAsync(UserRequests.PostRequestDestination.AccountActivation, CurrentUser);

					switch (requestResult)
					{
						case 0:
							{
								await (new MessageDialog("ActivationInstructionText",
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