using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
	/// Home code behind
	/// </summary>
	public sealed partial class HomePage : Page
	{
		public HomePage()
		{
			this.InitializeComponent();
			ViewModel = new ViewModel.MainViewModel.HomeViewModel();
		}

		ViewModel.MainViewModel.HomeViewModel ViewModel { get; set; }

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (e != null)
			{
				ViewModel.Frame = (Frame)e.Parameter;
			}
		}
	}
}
