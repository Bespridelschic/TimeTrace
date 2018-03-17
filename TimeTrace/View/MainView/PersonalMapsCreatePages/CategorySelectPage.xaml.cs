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
using TimeTrace.Model.Events.DBContext;
using Windows.UI;
using TimeTrace.ViewModel.MainViewModel.MapEventsViewModel;
using Windows.UI.Popups;
using System.Diagnostics;

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

			ViewModel = new CategoryViewModel();
		}

		private CategoryViewModel ViewModel { get; set; }
	}
}
