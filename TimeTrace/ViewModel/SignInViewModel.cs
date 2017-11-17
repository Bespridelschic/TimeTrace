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
using TimeTrace.View.SignUp;
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
		/// Конструктор инициализирующий новый объект <see cref="User"/> и пытающийся считать данные с файла
		/// </summary>
		public SignInViewModel()
		{
			CurrentUser = new User();
			CurrentUser.LoadUserFromFile();

			Processing = false;
			IsPasswordSave = true;

			SelectionStart = CurrentUser.Email.Length;
		}

		/// <summary>
		/// Проверка полей на корректность
		/// </summary>
		/// <returns>Удовлетворяют ли поля бизнес-логике</returns>
		private bool CanAppSignIn()
		{
			if (CurrentUser.Email.Length == 0 || CurrentUser.Password.Length == 0)
			{
				(new MessageDialog("Заполните все поля", "Ошибка входа")).ShowAsync();
				return false;
			}

			if (!CurrentUser.EmailCorrectChech())
			{
				(new MessageDialog("Не корректно введён адрес электронной почты. Проверьте корректность", "Ошибка входа")).ShowAsync();
				return false;
			}

			if (CurrentUser.Password.Length < 8)
			{
				(new MessageDialog("Пароль должен составлять минимум 8 символов", "Ошибка входа")).ShowAsync();
				return false;
			}

			return true;
		}

		/// <summary>
		/// Попытка входа в систему
		/// </summary>
		public async void AppSignIn()
		{
			if (!CanAppSignIn())
			{
				return;
			}

			Processing = true;

			try
			{
				var requestResult = await CurrentUser.SignInPostRequestAsync();

				await (new MessageDialog($"{requestResult}", "Успех")).ShowAsync();

				switch (requestResult)
				{
					case 0:
						{
							await (new MessageDialog("Вы успешно вошли в систему", "Успех")).ShowAsync();

							if (IsPasswordSave)
							{
								await CurrentUser.SaveUserToFile();
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
								await CurrentUser.SaveUserToFile();
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
		public async void SignUp()
		{
			if (Window.Current.Content is Frame frame)
			{
				Processing = false;
				frame.Navigate(typeof(SignUpExtendPage), CurrentUser.Email);
			}
		}

		/// <summary>
		/// Попытка входа в систему с помощью токена
		/// </summary>
		public async void AppSignInWithToken()
		{

		}

		/// <summary>
		/// Диалоговое окно подтверждения аккаунта
		/// </summary>
		/// <returns>Статус подтверждения аккаунта</returns>
		public async Task ConfirmAccountDialogAsync()
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
					var requestResult = await CurrentUser.AccountActivationPostRequestAsync();

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