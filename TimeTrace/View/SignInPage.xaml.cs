using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI;
using Newtonsoft.Json;
using System.Xml.Serialization;
using TimeTrace.Model;
using TimeTrace.View;
using System.Threading.Tasks;
using System.Net.Sockets;
using Windows.Storage;
using System.Security.Cryptography;
using Windows.ApplicationModel.Core;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace TimeTrace.View
{
	/// <summary>
	/// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
	/// </summary>
	public sealed partial class SignInPage : Page
	{
		public SignInPage()
		{
			this.InitializeComponent();

			ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(385, 800));

			// Получение системного цвета и установка цвета не автоматическим элементам
			var color = (Color)this.Resources["SystemAccentColor"];
			SolidColorBrush brush = new SolidColorBrush(color);
			SignInButton.BorderBrush = brush;
			HeaderText.Foreground = brush;

			// Получение логина из файла
			//Task<string> task = GetPasswordFromLocal();
			//LoginTextBox.Text = task.Result;

			// Лямбда обработки клавиши Enter на главном экране
			KeyUp += (sender, e) =>
				{
					if (e.Key == Windows.System.VirtualKey.Enter)
					{
						SignInButton_Click(this, e);
					}
				};
		}

		/// <summary>
		/// Собитие при входе в систему
		/// </summary>
		private async void SignInButton_Click(object sender, RoutedEventArgs e)
		{
			if (EmailTextBox.Text == string.Empty || PasswordTextBox.Password == string.Empty)
			{
				await (new MessageDialog("Одно из полей не задано", "Ошибка входа")).ShowAsync();

				if (EmailTextBox.Text == string.Empty)
				{
					EmailTextBox.Focus(FocusState.Keyboard);
				}
				else
				{
					PasswordTextBox.Focus(FocusState.Keyboard);
				}
				return;
			}

			User user = new User(EmailTextBox.Text, PasswordTextBox.Password);
			var res = user.SaveUserToFile();
			await (new MessageDialog($"{res.Result}", "Зашифрованный")).ShowAsync();

			res = user.LoadUserFromFile();
			await (new MessageDialog($"{res.Result}", "Расшифрованный")).ShowAsync();

			if (user.Email == "BadEmail")
			{
				await (new MessageDialog("Не правильно введен Email", "Ошибка входа")).ShowAsync();
				return;
			}

			await (new MessageDialog($"В сеть {user.GetHashEncrypt()}", $"Локально {user.SaveUserToFile()}")).ShowAsync();

			if (SavePasswordChBox.IsChecked == true)
			{
				StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
				StorageFile storageFile = await storageFolder.CreateFileAsync("PSF.bin", CreationCollisionOption.ReplaceExisting);

				await FileIO.WriteTextAsync(storageFile, PasswordTextBox.Password);
			}

			// Read from file

			/*StorageFolder storageFolder1 = ApplicationData.Current.LocalFolder;
			StorageFile storageFile1 = await storageFolder1.GetFileAsync("PSF.bin");

			await (new MessageDialog($"{FileIO.ReadTextAsync(storageFile1)}", "Зашифрованный пароль")).ShowAsync();*/

			//return;

			//try
			//{
			// http://o129pak8.beget.tech/site/hello

			/*WebRequest request = WebRequest.Create("http://o129pak8.beget.tech/site/signup");
			request.Method = "POST"; // для отправки используется метод Post
									 // данные для отправки
			string data = user.JsonSerialize();
			// преобразуем данные в массив байтов
			byte[] byteArray = Encoding.UTF8.GetBytes(data);
			// устанавливаем тип содержимого - параметр ContentType
			request.ContentType = "application/x-www-form-urlencoded";
			// Устанавливаем заголовок Content-Length запроса - свойство ContentLength
			//request.ContentLength = byteArray.Length;

			//записываем данные в поток запроса
			using (Stream dataStream = request.GetRequestStreamAsync())
			{
				dataStream.Write(byteArray, 0, byteArray.Length);
			}

			WebResponse response = await request.GetResponseAsync();
			using (Stream stream = response.GetResponseStream())
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					Console.WriteLine(reader.ReadToEnd());
				}
			}*/
			//response.Close();

			//	WebRequest request = WebRequest.Create("http://o129pak8.beget.tech/site/signup");
			//	WebResponse response = await request.GetResponseAsync();

			//	Stream webStream = response.GetResponseStream();
			//	string responseMessage = string.Empty;

			//	using (StreamReader sr = new StreamReader(webStream))
			//	{
			//		string line = "";
			//		while ((line = sr.ReadLine()) != null)
			//		{
			//			responseMessage += line;
			//		}
			//	}

			//	await (new MessageDialog($"{responseMessage}", "Результат запроса")).ShowAsync();
			//}
			//catch (Exception ex)
			//{
			//	await (new MessageDialog($"{ex.Message}", "Ошибка запроса!")).ShowAsync();
		}

		private void SignUpButton_Click(object sender, RoutedEventArgs e)
		{
			Frame.Navigate(typeof(SignUpPage), EmailTextBox.Text.ToString());
		}
	}
}
