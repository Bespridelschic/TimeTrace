using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using TimeTrace.ViewModel.MainViewModel;

namespace TimeTrace.View.MainView
{
	/// <summary>
	/// Start page code behind
	/// </summary>
	public sealed partial class StartPage : Page
	{
		/// <summary>
		/// Standart constructor
		/// </summary>
		public StartPage()
		{
			this.InitializeComponent();

			Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(650, 800));

			ViewModel = StartPageViewModel.Instance;
			DataContext = ViewModel;
		}

		public StartPageViewModel ViewModel { get; private set; }

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			(Application.Current as App).AppFrame = ContentFrame;
			(Application.Current as App).AppFrame.Navigate(typeof(HomePage));
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
}