using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TimeTrace.Model;
using TimeTrace.View.AuthenticationView;
using TimeTrace.View.AuthenticationView.SignUp;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TimeTrace.ViewModel.AuthenticationViewModel
{
	public class SignUpViewModel : BaseViewModel
	{
		#region Свойства

		private User currentUser;
		/// <summary>
		/// Текущий используемый пользователь
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
		/// Состояние ProgressRing
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
		/// Поля подтверждающего пароля
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
		/// Начальная позиция курсора текста
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
		/// Максимальная дата - текущий день
		/// </summary>
		public DateTime MaxDate { get; set; }
		
		private DateTimeOffset? selectedDate;
		/// <summary>
		/// Поля календаря
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
		/// Состояние флага доступности элементов управления графического интерфейса
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

		#endregion

		/// <summary>
		/// Стандартный конструктор
		/// </summary>
		public SignUpViewModel()
		{
			ControlEnable = false;

			CurrentUser = new User();

			Processing = false;
			ConfirmPassword = "";
			SelectionStart = CurrentUser.Email.Length;
			SelectedDate = null;
			MaxDate = DateTime.Today;

			ControlEnable = true;
		}

		/// <summary>
		/// Переход на страницу завершения регистрации
		/// </summary>
		public void SignUpContinue()
		{
			if (Window.Current.Content is Frame frame)
			{
				frame.Navigate(typeof(SignUpPage), CurrentUser);
			}
		}

		/// <summary>
		/// Проверка полей на корректность
		/// </summary>
		/// <returns>Удовлетворяют ли поля бизнес-логике</returns>
		private async Task<bool> CanSignUp()
		{
			if (CurrentUser.Email == "" || CurrentUser.Password == "" || ConfirmPassword == "")
			{
				await new MessageDialog("Необходимо заполнить все поля", "Проблема регистрации").ShowAsync();
				return false;
			}

			if (CurrentUser.Password.Length < 8)
			{
				await new MessageDialog("Минимальная длина пароля составляет 8 символов", "Проблема регистрации").ShowAsync();
				return false;
			}

			if (CurrentUser.Password != ConfirmPassword)
			{
				await new MessageDialog("Введенные пароли не совпадают", "Проблема регистрации").ShowAsync();

				return false;
			}

			if (!CurrentUser.EmailCorrectChech())
			{
				await new MessageDialog("Проверьте корректность введенного адреса электронной почты", "Проблема регистрации").ShowAsync();
				return false;
			}

			return true;
		}

		/// <summary>
		/// Попытка зарегистрировать аккаунт
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
				var requestResult = await UserRequest.PostRequestAsync(UserRequest.PostRequestDestination.SignUp, CurrentUser);

				switch (requestResult)
				{
					case 0:
						{
							await (new MessageDialog("Аккаунт успешно зарегистирован", "Успех")).ShowAsync();
							await UserFileWorker.SaveUserToFileAsync(CurrentUser);

							break;
						}
					case 1:
						{
							await (new MessageDialog("Пользователь с таким Email уже зарегистрирован", "Ошибка регистрации")).ShowAsync();

							ControlEnable = true;
							Processing = false;
							return;
						}
					default:
						{
							await (new MessageDialog("Ошибка регистрации, удаленный сервер не доступен. Повторите попытку позже", "Ошибка входа")).ShowAsync();

							break;
						}
				}
			}
			catch (Exception ex)
			{
				await (new MessageDialog($"{ex.Message}\n" +
					$"Ошибка регистрации, удаленный сервер не доступен. Повторите попытку позже", "Ошибка входа")).ShowAsync();
			}

			if (Window.Current.Content is Frame frame)
			{
				Processing = false;
				frame.Navigate(typeof(SignInPage), CurrentUser);
			}
		}
	}
}

