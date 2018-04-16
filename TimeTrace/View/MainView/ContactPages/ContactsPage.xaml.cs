using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using TimeTrace.ViewModel.MainViewModel.ContactsViewModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TimeTrace.View.MainView.ContactPages
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class ContactsPage : Page
	{
		public ContactsPage()
		{
			this.InitializeComponent();

			ViewModel = new ContactsViewModel();
		}

		public ContactsViewModel ViewModel { get; set; }
	}
}
