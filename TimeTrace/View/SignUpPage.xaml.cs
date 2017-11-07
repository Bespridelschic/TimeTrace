﻿using System;
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

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace TimeTrace.View
{
	/// <summary>
	/// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
	/// </summary>
	public sealed partial class SignUpPage : Page
	{
		public SignUpPage()
		{
			this.InitializeComponent();

			SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
			SystemNavigationManager.GetForCurrentView().BackRequested += (sender, e) =>
			{
				if (Window.Current.Content is Frame frame)
				{
					if (frame.CanGoBack)
					{
						frame.GoBack();
						e.Handled = true;
					}
				}
			};

			var color = (Color)this.Resources["SystemAccentColor"];
			SolidColorBrush brush = new SolidColorBrush(color);

		}
	}
}
