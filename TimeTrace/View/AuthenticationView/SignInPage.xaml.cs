using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using TimeTrace.Model;
using Windows.UI.Xaml.Media;
using Windows.UI;
using TimeTrace.ViewModel.AuthenticationViewModel;
using Windows.UI.Xaml.Navigation;

namespace TimeTrace.View.AuthenticationView
{
	/// <summary>
	/// Sign in code behind
	/// </summary>
	public sealed partial class SignInPage : Page
	{
		public SignInPage()
		{
			this.InitializeComponent();

			// Size of page
			ApplicationView.GetForCurrentView().SetPreferredMinSize(new Windows.Foundation.Size(385, 800));

			ViewModel = new SignInViewModel();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (e.Parameter != null && e.Parameter.ToString() != "")
			{
				ViewModel.CurrentUser = (User)e.Parameter;
			}

			if (Frame.BackStack.Count > 0)
			{
				Frame.BackStack.Clear();
			}
		}

		public SignInViewModel ViewModel { get; private set; }

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
