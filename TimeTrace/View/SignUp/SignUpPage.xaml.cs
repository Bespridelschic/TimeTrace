using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TimeTrace.View.SignUp;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace TimeTrace.View
{
	/// <summary>
	/// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
	/// </summary>
	public sealed partial class SignUpPage : Page
	{
		public SignUpPage()
		{
			this.InitializeComponent();

			var color = (Color)this.Resources["SystemAccentColor"];
			SolidColorBrush brush = new SolidColorBrush(color);
			SignUpButton.BorderBrush = brush;
			HeaderText.Foreground = brush;
			SignUpProgressRing.Foreground = brush;
		}

		/// <summary>
		/// Переход на предыдущую страницу
		/// </summary>
		/// <param name="sender">Объект отправитель</param>
		/// <param name="e">Параметр события</param>
		private async void SignUpButton_Click(object sender, RoutedEventArgs e)
		{
			SignUpProgressRing.IsActive = true;
			await Task.Delay(100);
			Frame.Navigate(typeof(SignUpExtendPage), EmailTextBox.Text.ToString());
			Frame.BackStack.RemoveAt(Frame.BackStackDepth - 1);
		}
	}
}
