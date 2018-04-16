using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using TimeTrace.Model;
using TimeTrace.ViewModel.MainViewModel.ContactsViewModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TimeTrace.View.MainView.ContactPages
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class ChatPage : Page
	{
		public ChatPage()
		{
			this.InitializeComponent();

			ViewModel = new ChatViewModel();
		}

		public ChatViewModel ViewModel { get; set; }

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			ViewModel.Interlocutor = (Contact) e.Parameter;
		}
	}
}
