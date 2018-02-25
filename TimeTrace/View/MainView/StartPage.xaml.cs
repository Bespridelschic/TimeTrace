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
using TimeTrace.View.AuthenticationView;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace TimeTrace.View.MainView
{
	/// <summary>
	/// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
	/// </summary>
	public sealed partial class StartPage : Page
	{
		public StartPage()
		{
			this.InitializeComponent();

			Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(650, 800));
			ContentFrame.Navigate(typeof(HomePage), ContentFrame);

			ViewModel = new ViewModel.MainViewModel.SettingsViewModel();
		}

		public ViewModel.MainViewModel.SettingsViewModel ViewModel;

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

					case "personalMaps":
						ContentFrame.Navigate(typeof(CategorySelectPage), ContentFrame);
						NavHeader.Header = "Интеллект-карты пользователя";
						break;

					case "scheduleSync":
						ShowToastNotification();
						break;

					default:
						NavHeader.Header = "Time Tracking";
						break;
				}
			}
		}

		private void ShowToastNotification()
		{
			var toastContent = new ToastContent()
			{
				Visual = new ToastVisual()
				{
					BindingGeneric = new ToastBindingGeneric()
					{
						Children =
						{
							new AdaptiveText()
							{
								Text = "Напоминание о событии",
								HintMaxLines = 1
							},
							new AdaptiveText()
							{
								Text = "Имя события"
							},
							new AdaptiveText()
							{
								Text = $"Начало в {DateTime.Now.ToShortTimeString()}"
							}
						},
						AppLogoOverride = new ToastGenericAppLogo()
						{
							//Source = "https://picsum.photos/48?image=883",
							Source = @"Assets/user-48.png",
							HintCrop = ToastGenericAppLogoCrop.Circle
						}
					}
				},
				Launch = "app-defined-string"
			};

			// Create the toast notification
			var toastNotif = new ToastNotification(toastContent.GetXml());

			// And send the notification
			ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
		}

		private async void Feedback(object sender, RoutedEventArgs e)
		{
			var message = new TextBox()
			{
				Header = "Расскажите, с какими проблемами вы столкнулись ?",
				PlaceholderText = "Опишите проблему...",
				Height = 200,
				Width = 400,
				TextWrapping = TextWrapping.Wrap,
				AcceptsReturn = true,
				IsSpellCheckEnabled = true
			};

			ContentDialog dialog = new ContentDialog
			{
				Title = "Обратная связь",
				Content = message,
				PrimaryButtonText = "Отправить",
				CloseButtonText = "Отмена",
				DefaultButton = ContentDialogButton.Primary,
			};

			if (await dialog.ShowAsync() == ContentDialogResult.Primary)
			{
				if (string.IsNullOrEmpty(message.Text))
				{
					await (new MessageDialog("Вы не можете отправить пустое сообщение!", "Ошибка")).ShowAsync();

					Feedback(sender, e);
				}
				else
				{
					await (new MessageDialog("Благодарим Вас за вашу поддержку", "Сообщение успешно отправлено")).ShowAsync();
				}
			}
		}

		private async void SignOut(object sender, RoutedEventArgs e)
		{
			ContentDialog dialog = new ContentDialog
			{
				Title = "Подтверждение",
				Content = "Вы уверены что хотите выйти из системы?",
				PrimaryButtonText = "Выйти",
				CloseButtonText = "Отмена",
				DefaultButton = ContentDialogButton.Close
			};

			if (await dialog.ShowAsync() == ContentDialogResult.Primary)
			{
				if (Window.Current.Content is Frame frame)
				{
					frame.Navigate(typeof(SignInPage));
				}
			}
		}
	}
}
