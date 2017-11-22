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
using TimeTrace.View.MainView;
using TimeTrace.View.SignUp;
using Windows.Storage;
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

		#endregion

		/// <summary>
		/// Конструктор инициализирующий новый объект <see cref="User"/> и пытающийся считать данные с файла
		/// </summary>
		public SignInViewModel()
		{
			//AppSignInWithToken().Wait();
			/*var res = AppSignInWithToken();
			if (res.Result == 0)
			{
				(new MessageDialog("Можно войти с помощью токена")).ShowAsync();
		}*/

			CurrentUser = new User();
			UserFileWorker.LoadUserFromFileAsync(CurrentUser).Wait();

			Processing = false;
			IsPasswordSave = true;

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

			Processing = false;
		}

		/// <summary>
		/// Переход на страницу регистрации
		/// </summary>
		public void SignUp()
		{
			if (Window.Current.Content is Frame frame)
			{
				Processing = false;
				frame.Navigate(typeof(SignUpExtendPage), CurrentUser.Email);
			}
		}

		/// <summary>
		/// Восстановление пароля
		/// </summary>
		public async void UserPasswordRecovery()
		{
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
						CurrentUser.Password = string.Empty;

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
		/// Попытка входа в систему с помощью токена
		/// </summary>
		private async Task<int> AppSignInWithToken()
		{
			var res = await UserFileWorker.LoadUserEmailAndTokenFromFile();

			if (string.IsNullOrEmpty(res.email) || string.IsNullOrEmpty(res.token))
			{
				// Не трогать! 1 - не удачная попытка входа в приложение с помощью токена
				return 1;
			}

			try
			{
				var requestResult = await UserRequest.SignInWithTokenPostRequestAsync(res.email, res.token);

				switch (requestResult)
				{
					case 0:
						{
							await (new MessageDialog("Вы успешно вошли в систему", "Успех")).ShowAsync();


							break;
						}
				}
			}
			catch (Exception ex)
			{
				await (new MessageDialog($"{ex.Message}\n" +
					$"Ошибка входа, удаленный сервер не доступен. Повторите попытку позже", "Ошибка входа")).ShowAsync();
			}

			// Не трогать! 1 - не удачная попытка входа в приложение с помощью токена
			return 1;
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