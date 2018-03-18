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
using TimeTrace.ViewModel.MainViewModel;

namespace TimeTrace.View.MainView
{
	/// <summary>
	/// Home code behind
	/// </summary>
	public sealed partial class HomePage : Page
	{
		public HomeViewModel ViewModel { get; private set; }

		public HomePage()
		{
			this.InitializeComponent();
			ViewModel = new HomeViewModel();
		}

		
	}
}
