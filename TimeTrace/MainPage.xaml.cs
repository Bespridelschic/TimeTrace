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

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace TimeTrace
{
	/// <summary>
	/// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();

			ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(400, 700));

			// Получение системного цвета и установка цвета не автоматическим элементам
			var color = (Color)this.Resources["SystemAccentColor"];
			SolidColorBrush brush = new SolidColorBrush(color);
			LoginButton.BorderBrush = brush;
			HeaderText.Foreground = brush;
		}

		/// <summary>
		/// Собитие при входе в систему
		/// </summary>
		private async void LoginInSystemButton(object sender, RoutedEventArgs e)
		{
			try
			{
				WebRequest request = WebRequest.Create("http://o129pak8.beget.tech/site/hello");
				WebResponse response = await request.GetResponseAsync();

				Stream webStream = response.GetResponseStream();
				string responseMessage = string.Empty;

				using (StreamReader sr = new StreamReader(webStream))
				{
					string line = "";
					while ((line = sr.ReadLine()) != null)
					{
						responseMessage += line;
					}
				}

				await (new MessageDialog($"{responseMessage}", "Результат запроса")).ShowAsync();
			}
			catch (Exception ex)
			{
				await (new MessageDialog($"{ex.Message}", "Ошибка запроса!")).ShowAsync();
			}
		}
	}
}
