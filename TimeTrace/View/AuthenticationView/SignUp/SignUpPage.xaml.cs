using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using TimeTrace.Model;

namespace TimeTrace.View.AuthenticationView.SignUp
{
	/// <summary>
	/// Sign up code behind
	/// </summary>
	public sealed partial class SignUpPage : Page
	{
		public SignUpPage()
		{
			this.InitializeComponent();

			ViewModel = new ViewModel.AuthenticationViewModel.SignUpViewModel();
		}

		public ViewModel.AuthenticationViewModel.SignUpViewModel ViewModel { get; private set; }

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (e.Parameter != null)
			{
				ViewModel.CurrentUser = (User)e.Parameter;
			}
		}

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
