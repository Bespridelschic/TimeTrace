using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeTrace.Model;
using TimeTrace.View;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace TimeTrace.ViewModel
{
	/// <summary>
	/// ViewModel для входа в систему
	/// </summary>
	public class SignInViewModel : INotifyPropertyChanged
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

		// Команды входа / регистрации
		public ICommand SignInCommand { get; set; }
		public ICommand SignUpCommand { get; set; }

		// Состояние ProgressRing
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

		// Начальная позиция курсора текста
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

		// Состояние флага сохранения пароля локально
		private bool isPasswordSave;
		public bool IsPasswordSave
		{
			get { return isPasswordSave; }
			set
			{
				isPasswordSave = value;
				OnPropertyChanged("IsPasswordSave");
			}
		}

		public SignInViewModel()
		{
			this.SignInCommand = new SignInCommand(this);
			this.SignUpCommand = new SignUpCommand();

			CurrentUser = new User();
			CurrentUser.LoadUserFromFile();

			Processing = false;
			IsPasswordSave = true;

			SelectionStart = CurrentUser.Email.Length;
		}

		public static void ShowMessage()
		{
			(new MessageDialog("From VM", "Ошибка входа")).ShowAsync();
		}

		// Диалоговое окно подтверждения аккаунта
		public async Task<string> ConfirmAccountDialogAsync()
		{
			TextBox inputTextBox = new TextBox
			{
				AcceptsReturn = false,
				Height = 32,
				Width = 300,
				PlaceholderText = "Ввести полученный код"
			};

			ContentDialog dialog = new ContentDialog
			{
				Content = inputTextBox,
				Title = "Подтверждение аккаунта",
				PrimaryButtonText = "Активировать",
				SecondaryButtonText = "Получить код",
				//CloseButtonText = "Позже",
				DefaultButton = ContentDialogButton.Primary,
			};

			if (await dialog.ShowAsync() == ContentDialogResult.Primary)
			{
				return inputTextBox.Text;
			}

			if (await dialog.ShowAsync() == ContentDialogResult.Secondary)
			{

			}

			return string.Empty;
		}

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged = delegate { };
		private void OnPropertyChanged([CallerMemberName]string prop = "")
		{
			PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}

		#endregion
	}

	// TODO: имплементировать фокус на незаполненные поля, реагирование на Enter


	/// <summary>
	/// Класс команды входа в систему
	/// </summary>
	public class SignInCommand : ICommand
	{
		public event EventHandler CanExecuteChanged;

		public SignInViewModel CurrentUserViewModel { get; set; }

		public SignInCommand(SignInViewModel userViewModel)
		{
			CurrentUserViewModel = userViewModel;
		}

		public bool CanExecute(object parameter)
		{
			if (parameter is User user)
			{
				if (user.Email.Length == 0 || user.Password.Length == 0)
				{
					(new MessageDialog("Заполните все поля", "Ошибка входа")).ShowAsync();
					return false;
				}

				if (!user.EmailCorrectChech())
				{
					(new MessageDialog("Не корректно введён адрес электронной почты. Проверьте корректность", "Ошибка входа")).ShowAsync();
					return false;
				}

				if (user.Password.Length < 8)
				{
					(new MessageDialog("Пароль должен составлять минимум 8 символов", "Ошибка входа")).ShowAsync();
					return false;
				}
			}

			return true;
		}

		public async void Execute(object parameter)
		{
			CurrentUserViewModel.Processing = true;

			try
			{
				if (parameter is User user)
				{
					var requestResult = await user.SignInPostRequestAsync();

					switch (requestResult)
					{
						case 0:
							{
								await (new MessageDialog("Вы успешно вошли в систему", "Успех")).ShowAsync();
								//await CurrentUserViewModel.CurrentUser.SaveUserToFile();

								break;
							}
						case 1:
							{
								await (new MessageDialog($"Не найдено совпадений с существующими аккаунтами." +
									$"Проверьте корректность введенных данных, или зарегистрируйте новый аккаунт", "Ошибка входа")).ShowAsync();

								break;
							}
						case 2:
							{
								await CurrentUserViewModel.ConfirmAccountDialogAsync();
								//await CurrentUserViewModel.CurrentUser.SaveUserToFile();

								break;
							}
						case -1:
							{
								await (new MessageDialog("Ошибка входа, удаленный сервер не доступен. Повторите попытку позже", "Ошибка входа")).ShowAsync();

								break;
							}
						default:
							{
								await (new MessageDialog("Непредвиденная ошибка. Обратитесь к разработчику программного обеспечения", "Ошибка входа")).ShowAsync();

								break;
							}
					}
				}
			}
			catch (Exception ex)
			{
				await (new MessageDialog($"{ex.Message}", "Ошибка входа")).ShowAsync();
			}

			CurrentUserViewModel.Processing = false;
		}
	}

	/// <summary>
	/// Класс команды регистрации нового пользователя
	/// </summary>
	public class SignUpCommand : ICommand
	{
		public event EventHandler CanExecuteChanged;

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public async void Execute(object parameter)
		{
			if (parameter is User user)
			{
				/*Frame frame = Window.Current.Content as Frame;

				if (frame == null)
				{
					await(new MessageDialog("Frame is null", "Отказ")).ShowAsync();
				}

				if (user.Email.Length > 0)
				{
					frame.Navigate(typeof(SignUpPage), user.Email);
				}
				else
				{
					frame.Navigate(typeof(SignUpPage));
				}*/
			}
		}
	}
}
