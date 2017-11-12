using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TimeTrace.Model;
using Windows.UI.Xaml.Media;
using Windows.UI;
//using System.Drawing;

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

			// Установка размеров начального окна
			ApplicationView.GetForCurrentView().SetPreferredMinSize(new Windows.Foundation.Size(385, 800));

			// Лямбда обработки клавиши Enter на главном экране
			KeyUp += (sender, e) =>
			{
				if (e.Key == Windows.System.VirtualKey.Enter)
				{
					
				}
			};

			EmailTextBox.SelectionStart = EmailTextBox.Text.Length;
		}

		/// <summary>
		/// Получение системного цвета и установка цвета не автоматическим элементам
		/// </summary>
		private SolidColorBrush SolidBrush
		{
			get
			{
				var color = (Color)this.Resources["SystemAccentColor"];
				return new SolidColorBrush(color);
			}
		}

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
}
