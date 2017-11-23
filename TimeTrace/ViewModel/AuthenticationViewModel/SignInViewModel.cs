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

namespace TimeTrace.ViewModel.AuthenticationViewModel
{
	/// <summary>
	/// ViewModel для входа в систему
	/// </summary>
	public class SignInViewModel : INotifyPropertyChanged
	{
		#region Свойства

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

		/// <summary>
		/// ProgressRing
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
		/// Состояние флага сохранения пароля локально
		/// </summary>
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
		/// Конструктор инициализирующий новый объект <see cref="User"/> и пытающийся считать данные с файла
		/// </summary>
		public SignInViewModel()
		{
			CurrentUser = new User();
			UserFileWorker.LoadUserFromFileAsync(CurrentUser).GetAwaiter();

			Processing = false;
			IsPasswordSave = true;
			ControlEnable = true;

			SelectionStart = CurrentUser.Email.Length;
		}

		/// <summary>
		/// Проверка полей на корректность
		/// </summary>
		/// <returns>Удовлетворяют ли поля бизнес-логике</returns>
		private async Task<bool> CanAppSignIn()
		{
			if (CurrentUser.Email.Length == 0 || CurrentUser.Password.Length == 0)
			{
				await new MessageDialog("Заполните все поля", "Ошибка операции").ShowAsync();
				return false;
			}

			if (!CurrentUser.EmailCorrectChech())
			{
				await new MessageDialog("Не корректно введён адрес электронной почты. Проверьте корректность", "Ошибка операции").ShowAsync();
				return false;
			}

			if (CurrentUser.Password.Length < 8)
			{
				await new MessageDialog("Пароль должен составлять минимум 8 символов", "Ошибка операции").ShowAsync();
				return false;
			}

			return true;
		}

		/// <summary>
		/// Попытка входа в систему
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
				var requestResult = await UserRequest.SignInPostRequestAsync(CurrentUser);

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
							await (new MessageDialog($"Не найдено совпадений с существующими аккаунтами." +
								$"Проверьте корректность введенных данных, или зарегистрируйте новый аккаунт", "Ошибка входа")).ShowAsync();

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
			catch (Exception ex)
			{
				await (new MessageDialog($"{ex.Message}\n" +
					$"Ошибка входа, удаленный сервер не доступен. Повторите попытку позже", "Ошибка входа")).ShowAsync();
			}

			ControlEnable = true;
			Processing = false;
		}

		/// <summary>
		/// Переход на страницу регистрации
		/// </summary>
		public void SignUp()
		{
			if (Window.Current.Content is Frame frame)
			{
				frame.Navigate(typeof(SignUpExtendPage), CurrentUser.Email);
			}
		}

		/// <summary>
		/// Восстановление пароля
		/// </summary>
		public async void UserPasswordRecovery()
		{
			string currentPassword = CurrentUser.Password;
			string currentEmail = new string(CurrentUser.Email.ToCharArray());

			#region Разметка диалогового окна

			TextBox emailTextBox = new TextBox
			{
				PlaceholderText = "Введите вашу электронную почту...",
				Header = "Адрес электронной почты:",
				Text = CurrentUser.Email
			};

			PasswordBox passwordTextBox = new PasswordBox
			{
				PlaceholderText = "Введите новый пароль...",
				Header = "Новый пароль:",
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
				Title = "Восстановление пароля",
				Content = grid,
				PrimaryButtonText = "Восстановить",
				CloseButtonText = "Отложить",
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

					var requestResult = await UserRequest.PasswordResetPostRequestAsync(CurrentUser);

					switch (requestResult)
					{
						case 0:
							{
								await (new MessageDialog("На вашу электронную почту отправлено письмо с инструкцией по активации", "Изменение пароля")).ShowAsync();
								break;
							}
						case 1:
							{
								await (new MessageDialog("Ошибка изменения пароля! Сброс пароля не возможен", "Изменение пароля")).ShowAsync();
								break;
							}
						default:
							{
								await (new MessageDialog("Не предвиденная ошибка. Обратитесь к разработчику", "Изменение пароля")).ShowAsync();
								break;
							}
					}
				}
				catch (Exception ex)
				{
					await (new MessageDialog($"{ex.Message}\n" +
						$"Не предвиденная ошибка. Обратитесь к разработчику", "Ошибка изменения пароля")).ShowAsync();
				}
			}
		}

		/// <summary>
		/// Диалоговое окно подтверждения аккаунта
		/// </summary>
		/// <returns>Статус подтверждения аккаунта</returns>
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
				Title = "Активировать аккаунт",
				Content = textBox,
				PrimaryButtonText = "Получить код",
				CloseButtonText = "Отложить",
				DefaultButton = ContentDialogButton.Primary,
			};

			if (await dialog.ShowAsync() == ContentDialogResult.Primary)
			{
				try
				{
					var requestResult = await UserRequest.AccountActivationPostRequestAsync(CurrentUser);

					switch (requestResult)
					{
						case 0:
							{
								await (new MessageDialog("На вашу электронную почту отправлено письмо с инструкцией по активации", "Подтверждение аккаунта")).ShowAsync();
								break;
							}
						case 1:
							{
								await (new MessageDialog("Не предвиденная ошибка. Повторите попытку позже", "Подтверждение аккаунта")).ShowAsync();
								break;
							}
						default:
							{
								await (new MessageDialog("Не предвиденная ошибка. Обратитесь к разработчику", "Подтверждение аккаунта")).ShowAsync();
								break;
							}
					}
				}
				catch (Exception ex)
				{
					await (new MessageDialog($"{ex.Message}\n" +
						$"Не предвиденная ошибка. Обратитесь к разработчику", "Ошибка входа")).ShowAsync();
				}
			}
		}

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged = delegate { };
		private void OnPropertyChanged([CallerMemberName]string prop = "")
		{
			PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}

		#endregion
	}
}