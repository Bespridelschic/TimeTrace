using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TimeTrace.Model;
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

namespace TimeTrace.View.MainView.PersonalMapsCreatePages
{
	/// <summary>
	/// Area code behind
	/// </summary>
	public sealed partial class CategorySelectPage : Page
	{
		public CategorySelectPage()
		{
			this.InitializeComponent();
			
			ViewModel = new PersonalEventCreateViewModel(null);
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (e != null)
			{
				ViewModel.Frame = (Frame)e.Parameter;
			}
		}

		PersonalEventCreateViewModel ViewModel { get; set; }
	}
}
