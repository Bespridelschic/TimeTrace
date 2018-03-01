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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;

namespace TimeTrace.View.MainView
{
	/// <summary>
	/// Start page code behind
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
		/// Navigation menu
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="args">Parameter</param>
		private async void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
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
						break;

					case "schedule":
						ContentFrame.Navigate(typeof(SchedulePage));
						break;

					case "personalMaps":
						ContentFrame.Navigate(typeof(CategorySelectPage), ContentFrame);
						break;

					case "scheduleSync":
						ShowToastNotification();
						break;

					case "help":
						await (new MessageDialog("Помощь пользователя находится в разработке").ShowAsync());
						break;

					default:
						break;
				}
			}
		}

		/// <summary>
		/// Send push-notification to windows
		/// </summary>
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

		/// <summary>
		/// Sending message to developer
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="e">Parameter</param>
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

		/// <summary>
		/// Sign out from system
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="e">Parameter</param>
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
				// Remove local user data settings
				ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
				localSettings.Values.Remove("email");
				localSettings.Values.Remove("lastName");
				localSettings.Values.Remove("firstName");
				localSettings.Values.Remove("middleName");
				localSettings.Values.Remove("birthday");

				if (Window.Current.Content is Frame frame)
				{
					frame.Navigate(typeof(SignInPage));
				}
			}
		}
	}
}