using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeTrace.Model;
using TimeTrace.View;
using TimeTrace.View.SignUp;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TimeTrace.ViewModel
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

		public ICommand SignUpCommand { get; set; }

		public SignUpViewModel()
		{
			CurrentUser = new User();
			this.SignUpCommand = new SignUpExtendCommand();
		}

		public event PropertyChangedEventHandler PropertyChanged = delegate { };
		public void OnPropertyChanged([CallerMemberName]string prop = "")
		{
			PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}

		/// <summary>
		/// Класс команды регистрации нового пользователя
		/// </summary>
		public class SignUpExtendCommand : ICommand
		{
			public string ConfirmPassword { get; set; }

			public event EventHandler CanExecuteChanged;

			public bool CanExecute(object parameter)
			{
				if (parameter is User user)
				{
					if (user.Password != ConfirmPassword)
					{
						(new MessageDialog("Введенные пароли не совпадают", "Ошибка регистрации")).ShowAsync();
						return false;
					}

					if (user.PasswordSecurityCheck() < User.PasswordScore.Weak || user.Password.Length < 8)
					{
						(new MessageDialog($"Ваш пароль не соответствует критериям безопасности\n" +
							$"Обратите внимание, пароль должен быть не менее 8 символов", "Ошибка регистрации")).ShowAsync();
						return false;
					}

					if (!user.EmailCorrectChech())
					{
						(new MessageDialog("Ошибка в записи электронной почты. Проверьте корректность", "Ошибка регистрации")).ShowAsync();
						return false;
					}

					return true;
				}
				return false;
			}

			public void Execute(object parameter)
			{
				if (parameter is User user)
				{
					Frame frame = Window.Current.Content as Frame;
					frame.Navigate(typeof(SignUpExtendPage), user);

					return;
				}
			}
		}
	}
}

