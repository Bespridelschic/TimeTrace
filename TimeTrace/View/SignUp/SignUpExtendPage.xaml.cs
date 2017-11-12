using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace TimeTrace.View.SignUp
{
	/// <summary>
	/// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
	/// </summary>
	public sealed partial class SignUpExtendPage : Page
	{
		public SignUpExtendPage()
		{
			this.InitializeComponent();

			var color = (Color)this.Resources["SystemAccentColor"];
			SolidColorBrush brush = new SolidColorBrush(color);
			SignUpCompleteButton.BorderBrush = brush;
			HeaderText.Foreground = brush;
			SignUpProgressRing.Foreground = brush;
		}

		private async void SignUpCompleteButton_Click(object sender, RoutedEventArgs e)
		{
			/*Frame frame = Window.Current.Content as Frame;

			SignUpProgressRing.IsActive = true;

			await Task.Delay(100);

			List<PageStackEntry> pageEntries = frame.BackStack.ToList();
			if (pageEntries.Count > 0)
				frame.Navigate(pageEntries[0].SourcePageType);

			SignUpProgressRing.IsActive = false;*/

			Frame.Navigate(typeof(SignInPage));
		}
	}
}
