using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TimeTrace.Model;
using Windows.UI.Xaml.Media;
using Windows.UI;
using TimeTrace.View.SignUp;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
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

			ViewModel = new ViewModel.SignInViewModel();

			KeyDown += (sender, e) =>
			{
				if (e.Key == Windows.System.VirtualKey.Enter)
				{
					ViewModel.AppSignIn();
				}
			};
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (e.Parameter != null && e.Parameter.ToString() != "")
			{
				ViewModel.CurrentUser = (User)e.Parameter;

				if (Frame.BackStack.Count > 0)
				{
					Frame.BackStack.Clear();
				}
			}
		}

		// Привязка ViewModel
		public ViewModel.SignInViewModel ViewModel { get; private set; }

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
	}
}
