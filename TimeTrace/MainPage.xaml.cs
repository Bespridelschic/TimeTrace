using System;
using System.Collections.Generic;
using System.IO;
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

			//var color = this.Resources["SystemAccentColor"];

			//LoginButton.BorderBrush = color;
		}

		private async void LoginInSystemButton(object sender, RoutedEventArgs e)
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
	}
}
