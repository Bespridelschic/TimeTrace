using Windows.ApplicationModel.Activation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using TimeTrace.Model;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
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
				// Pattern matching for sign in after restart, because e.Parameter is string after that
				if (e.Parameter is User sended)
				{
					ViewModel.CurrentUser = sended;
				}
			}

			if (Window.Current.Content is Frame frame)
			{
				(Application.Current as App).AppFrame = frame;
				(Application.Current as App).AppFrame.BackStack.Clear();

				(Application.Current as App).AppFrame.Navigated += (s, args) =>
				{
					if ((Application.Current as App).AppFrame.CanGoBack)
					{
						SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
							AppViewBackButtonVisibility.Visible;
					}
					else
					{
						SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
							AppViewBackButtonVisibility.Collapsed;
					}
				};
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
