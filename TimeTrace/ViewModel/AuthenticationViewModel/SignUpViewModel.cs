using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeTrace.Model;
using TimeTrace.View.AuthenticationView;
using TimeTrace.View.AuthenticationView.SignUp;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace TimeTrace.ViewModel.AuthenticationViewModel
{
	public class SignUpViewModel : INotifyPropertyChanged
	{
		private User currentUser;
		public User CurrentUser
		{
			get { return currentUser; }
			set
			{
				currentUser = value;
				OnPropertyChanged("CurrentUser");
			}
		}

		#region Свойства

		/// <summary>
		/// Состояние ProgressRing
		/// </summary>
		private bool processing;
		public bool Processing
		{
			get { return processing; }
			set
			{
				processing = value;
				OnPropertyChanged("Processing");
			}
		}

		/// <summary>
		/// Поля подтверждающего пароля
		/// </summary>
		private string confirmPassword;
		public string ConfirmPassword
		{
			get { return confirmPassword; }
			set
			{
				confirmPassword = value;
				OnPropertyChanged("ConfirmPassword");
			}
		}

		/// <summary>
		/// Начальная позиция курсора текста
		/// </summary>
		private int selectionStart;
		public int SelectionStart
		{
			get { return selectionStart; }
			set
			{
				selectionStart = value;
				OnPropertyChanged("StartSelect");
			}
		}

		/// <summary>
		/// Максимальная дата - текущий день
		/// </summary>
		public DateTime MaxDate { get; set; }

		/// <summary>
		/// Поля календаря
		/// </summary>
		private DateTimeOffset selectedDate;
		public DateTimeOffset SelectedDate
		{
			get { return selectedDate; }
			set
			{
				selectedDate = value;
				CurrentUser.Birthday = $"{selectedDate.Year}-{selectedDate.Month}-{selectedDate.Day}";
				OnPropertyChanged("SelectedDate");
			}
		}

		/// <summary>
		/// Состояние флага доступности элементов управления графического интерфейса
		/// </summary>
		private bool controlEnable;
		public bool ControlEnable
		{
			get { return controlEnable; }
			set
			{
				controlEnable = value;
				OnPropertyChanged("ControlEnable");
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

			SelectedDate = new DateTimeOffset(new DateTime(2000, 1, 1));
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
				var requestResult = await UserRequest.SignUpPostRequestAsync(CurrentUser);

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
					case -1:
						{
							await (new MessageDialog("Не предвиденная ошибка. Обратитесь к разработчику", "Ошибка входа")).ShowAsync();

							break;
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

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged = delegate { };
		public void OnPropertyChanged([CallerMemberName]string prop = "")
		{
			PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}

		#endregion
	}
}

