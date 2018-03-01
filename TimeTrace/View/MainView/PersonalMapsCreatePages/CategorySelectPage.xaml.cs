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
			
			ViewModel = new PersonalEventCreateViewModel(null, mainGridPanel);

			using (MapEventContext db = new MapEventContext())
			{
				var res = db.Areas.Select(i => i);

				foreach (var i in res)
				{
					Button button = new Button()
					{
						Tag = i.Id,
						BorderBrush = GetColorFromString(i.Color),
						Content = i,
					};

					mainGridPanel.Children.Add(button);
				}
			}
		}

		private SolidColorBrush GetColorFromString(string color)
		{
			var colorString = color.Replace("#", string.Empty);

			var r = byte.Parse(colorString.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
			var g = byte.Parse(colorString.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
			var b = byte.Parse(colorString.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

			Color localColor = Color.FromArgb(255, r, g, b);
			return new SolidColorBrush(localColor);
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
