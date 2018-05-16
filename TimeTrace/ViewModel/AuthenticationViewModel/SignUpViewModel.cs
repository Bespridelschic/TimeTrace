using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TimeTrace.Model;
using TimeTrace.View.AuthenticationView;
using TimeTrace.View.AuthenticationView.SignUp;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TimeTrace.Model.Requests;

namespace TimeTrace.ViewModel.AuthenticationViewModel
{
	public class SignUpViewModel : BaseViewModel
	{
		#region Properties

		private User currentUser;
		/// <summary>
		/// Current user
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
		/// State of ProgressRing
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

		private string confirmPassword;
		/// <summary>
		/// Password confirm
		/// </summary>
		public string ConfirmPassword
		{
			get => confirmPassword;
			set
			{
				confirmPassword = value;
				OnPropertyChanged();
			}
		}

		private int selectionStart;
		/// <summary>
		/// Start text cursor position
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
		/// Initialization of new object
		/// </summary>
		public SignUpViewModel()
		{
			CurrentUser = new User();
			ResourceLoader = ResourceLoader.GetForCurrentView("SignInUp");

			ControlEnable = false;
			Processing = false;
			ConfirmPassword = "";
			SelectionStart = CurrentUser.Email.Length;
			ControlEnable = true;
		}

		/// <summary>
		/// Check fields correction
		/// </summary>
		/// <returns>Is fields correct</returns>
		private async Task<bool> CanSignUp()
		{
			if (string.IsNullOrEmpty(CurrentUser.Email?.Trim()) || string.IsNullOrEmpty(CurrentUser.Password) || string.IsNullOrEmpty(ConfirmPassword))
			{
				await new MessageDialog(ResourceLoader.GetString("/SignInUp/FillAllFieldsText"),
					ResourceLoader.GetString("/SignInUp/RegistrationProblemText")).ShowAsync();

				return false;
			}

			if (CurrentUser.Password.Length < 8)
			{
				await new MessageDialog(ResourceLoader.GetString("/SignInUp/PasswordLengthShortText"),
					ResourceLoader.GetString("/SignInUp/RegistrationProblemText")).ShowAsync();

				return false;
			}

			if (CurrentUser.Password != ConfirmPassword)
			{
				await new MessageDialog(ResourceLoader.GetString("/SignInUp/PasswordsDoNotMatch"),
					ResourceLoader.GetString("/SignInUp/RegistrationProblemText")).ShowAsync();

				return false;
			}

			if (!CurrentUser.EmailCorrectCheck())
			{
				await new MessageDialog(ResourceLoader.GetString("/SignInUp/IncorrectEmailText"),
					ResourceLoader.GetString("/SignInUp/RegistrationProblemText")).ShowAsync();

				return false;
			}

			return true;
		}

		/// <summary>
		/// Try to sign up
		/// </summary>
		public async void SignUpComplete()
		{
			CurrentUser.Email = CurrentUser.Email.Trim();

			var CanSignUpResult = await CanSignUp();

			if (!CanSignUpResult)
			{
				return;
			}

			if (!InternetRequests.CheckForInternetConnection())
			{
				await new MessageDialog("Проверьте своё подключение к интернету", "Ошибка регистрации").ShowAsync();

				return;
			}

			Processing = true;
			ControlEnable = false;

			try
			{
				var requestResult = await InternetRequests.PostRequestAsync(InternetRequests.PostRequestDestination.SignUp, CurrentUser);

				switch (requestResult)
				{
					case 0:
					{
						await (new MessageDialog(ResourceLoader.GetString("/SignInUp/AccountSuccessRegistredText"),
							ResourceLoader.GetString("/SignInUp/SuccessText"))).ShowAsync();
						await CurrentUser.SaveUserToFileAsync();

						break;
					}
					case 1:
					{
						await (new MessageDialog(ResourceLoader.GetString("/SignInUp/EmailAlreadyRegisteredText"),
							ResourceLoader.GetString("/SignInUp/RegistrationError"))).ShowAsync();

						return;
					}
					default:
					{
						await (new MessageDialog(ResourceLoader.GetString("/SignInUp/ServerConnectionProblemBadRegistrationText"),
							ResourceLoader.GetString("/SignInUp/SignInErrorText"))).ShowAsync();

						break;
					}
				}
			}
			catch (Exception ex)
			{
				await (new MessageDialog($"{ex.Message}\n" +
				                         $"{ResourceLoader.GetString("/SignInUp/ServerConnectionProblemBadRegistrationText")}",
					ResourceLoader.GetString("/SignInUp/SignInErrorText"))).ShowAsync();
			}
			finally
			{
				ControlEnable = true;
				Processing = false;
			}

			if (Window.Current.Content is Frame frame)
			{
				frame.Navigate(typeof(SignInPage));
			}
		}
	}
}