using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TimeTrace.Model.Events;
using TimeTrace.ViewModel.MainViewModel;
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

namespace TimeTrace.View.MainView.PersonalMapsCreatePages
{
	/// <summary>
	/// Event create code behind
	/// </summary>
	public sealed partial class PersonalEventCreatePage : Page
	{
		private PersonalEventCreateViewModel ViewModel { get; set; }
		private ScheduleViewModel ScheduleViewModel { get; set; }

		public PersonalEventCreatePage()
		{
			this.InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (e != null)
			{
				Project proj = (Project) e.Parameter;

				ViewModel = new PersonalEventCreateViewModel();
				ScheduleViewModel = new ScheduleViewModel(proj.Id);

				ViewModel.CurrentMapEvent.ProjectId = proj.Id;
				ViewModel.CurrentMapEvent.Color = proj.Color;
			}
		}
	}
}
