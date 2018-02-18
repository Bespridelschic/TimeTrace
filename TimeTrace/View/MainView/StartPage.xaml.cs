using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using TimeTrace.View.MainView.PersonalMapsCreatePages;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace TimeTrace.View.MainView
{
	/// <summary>
	/// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
	/// </summary>
	public sealed partial class StartPage : Page
	{
		public string TitleText { get; private set; } = "Стартовая страница";

		public StartPage()
		{
			this.InitializeComponent();

			Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().SetPreferredMinSize(new Windows.Foundation.Size(650, 800));
			ContentFrame.Navigate(typeof(HomePage), ContentFrame);

			ViewModel = new ViewModel.MainViewModel.SettingsViewModel();
		}

		ViewModel.MainViewModel.SettingsViewModel ViewModel;

		/// <summary>
		/// Навигация по меню приложения
		/// </summary>
		/// <param name="sender">Объект отправитель</param>
		/// <param name="args">Событие</param>
		private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
		{
			if (args.IsSettingsSelected)
			{
				ContentFrame.Navigate(typeof(SettingsPage));
			}
			else
			{
				NavigationViewItem item = args.SelectedItem as NavigationViewItem;

				switch (item.Tag)
				{
					case "home":
						ContentFrame.Navigate(typeof(HomePage), ContentFrame);
						NavHeader.Header = "Домашняя страница";
						break;

					case "schedule":
						ContentFrame.Navigate(typeof(SchedulePage));
						NavHeader.Header = "Расписание";
						break;

					case "contacts":
						ContentFrame.Navigate(typeof(ContactsPage));
						NavHeader.Header = "Контакты";
						break;

					case "personalMaps":
						ContentFrame.Navigate(typeof(CategorySelectPage), ContentFrame);
						NavHeader.Header = "Интеллект-карты пользователя";
						break;

					case "groupMaps":
						ContentFrame.Navigate(typeof(GroupMapsPage));
						NavHeader.Header = "Интеллект-карты с участием пользователя";
						break;

					default:
						NavHeader.Header = "Time Tracking";
						break;
				}
			}
		}
	}
}
