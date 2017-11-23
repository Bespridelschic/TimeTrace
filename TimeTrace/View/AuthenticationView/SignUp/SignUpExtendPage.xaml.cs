using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TimeTrace.View.AuthenticationView.SignUp
{
	/// <summary>
	/// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
	/// </summary>
	public sealed partial class SignUpExtendPage : Page
	{
		public SignUpExtendPage()
		{
			this.InitializeComponent();

			ViewModel = new ViewModel.AuthenticationViewModel.SignUpViewModel();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (e.Parameter != null)
			{
				ViewModel.CurrentUser.Email = e.Parameter.ToString();
			}
		}

		public ViewModel.AuthenticationViewModel.SignUpViewModel ViewModel { get; private set; }

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
