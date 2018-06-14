using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TimeTrace.Model.Events;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TimeTrace.View.MainView
{
	/// <summary>
	/// Schedule code behind
	/// </summary>
	public sealed partial class SchedulePage : Page
	{
		public SchedulePage()
		{
			this.InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (e.Parameter is string requestedMapEvents)
			{
				ViewModel = new ViewModel.MainViewModel.ScheduleViewModel(requestedMapEvents);
			}
			else
			{
				ViewModel = new ViewModel.MainViewModel.ScheduleViewModel();
			}
		}

		public ViewModel.MainViewModel.ScheduleViewModel ViewModel { get; set; }

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
