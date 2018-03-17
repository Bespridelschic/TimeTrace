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
			get { return currentUser; }
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
			get { return processing; }
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
			get { return confirmPassword; }
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
			get { return selectionStart; }
			set
			{
				selectionStart = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Maximum updateAt - current updateAt
		/// </summary>
		public DateTime MaxDate { get; set; }

		private DateTimeOffset? selectedDate;
		/// <summary>
		/// Selected calendar updateAt
		/// </summary>
		public DateTimeOffset? SelectedDate
		{
			get { return selectedDate; }
			set
			{
				if (value != null)
				{
					selectedDate = value;
					CurrentUser.Birthday = $"{selectedDate.Value.Year}-{selectedDate.Value.Month}-{selectedDate.Value.Day}";
					OnPropertyChanged();
				}
				else
				{
					CurrentUser.Birthday = string.Empty;
				}
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
			SelectedDate = null;
			MaxDate = DateTime.Today;
			ControlEnable = true;
		}

		/// <summary>
		/// Navigate to page of registration ending
		/// </summary>
		public void SignUpContinue()
		{
			if (Window.Current.Content is Frame frame)
			{
				frame.Navigate(typeof(SignUpPage));
			}
		}

		/// <summary>
		/// Check fields correction
		/// </summary>
		/// <returns>Is fields correct</returns>
		private async Task<bool> CanSignUp()
		{
			if (string.IsNullOrEmpty(CurrentUser.Email) || string.IsNullOrEmpty(CurrentUser.Password) || string.IsNullOrEmpty(ConfirmPassword))
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
			var CanSignUpResult = await CanSignUp();

			if (!CanSignUpResult)
			{
				return;
			}

			Processing = true;
			ControlEnable = false;

			try
			{
				var requestResult = await UserRequests.PostRequestAsync(UserRequests.PostRequestDestination.SignUp, CurrentUser);

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