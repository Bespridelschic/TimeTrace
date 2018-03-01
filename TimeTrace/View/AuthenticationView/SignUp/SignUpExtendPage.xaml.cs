using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TimeTrace.View.AuthenticationView.SignUp
{
	/// <summary>
	/// Sign up extend code behind
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
		/// System color
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
