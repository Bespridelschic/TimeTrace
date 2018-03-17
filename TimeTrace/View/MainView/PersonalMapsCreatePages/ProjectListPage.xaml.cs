using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TimeTrace.Model.Events;
using TimeTrace.ViewModel.MainViewModel.MapEventsViewModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TimeTrace.View.MainView.PersonalMapsCreatePages
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class ProjectListPage : Page
	{
		public ProjectListPage()
		{
			this.InitializeComponent();

			ViewModel = new ProjectViewModel();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (e != null)
			{
				ViewModel.CurrentArea = (Area)e.Parameter;
				ViewModel.Proj.AreaId = ViewModel.CurrentArea.Id;
			}
		}

		private ProjectViewModel ViewModel { get; set; }
	}
}
