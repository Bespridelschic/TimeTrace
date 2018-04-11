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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Core;
using Newtonsoft.Json;

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
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			(Application.Current as App).AppFrame = ContentFrame;
			(Application.Current as App).AppFrame.Navigate(typeof(HomePage));
			(Application.Current as App).AppFrame.BackStack.Clear();

			(Application.Current as App).AppFrame.Navigated += (s, args) =>
			{
				if ((Application.Current as App).AppFrame.CanGoBack)
				{
					SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
						AppViewBackButtonVisibility.Visible;
				}
				else
				{
					SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
						AppViewBackButtonVisibility.Collapsed;
				}
			};
		}

		/// <summary>
		/// Navigation menu
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="args">Parameter</param>
		private async void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
		{
			if (args.IsSettingsSelected)
			{
				(Application.Current as App).AppFrame.Navigate(typeof(SettingsPage));
			}

			else
			{
				NavigationViewItem item = args.SelectedItem as NavigationViewItem;

				switch (item.Tag)
				{
					case "home":
						(Application.Current as App).AppFrame.Navigate(typeof(HomePage));
						break;

					case "schedule":
						(Application.Current as App).AppFrame.Navigate(typeof(SchedulePage));
						break;

					case "contacts":
						(Application.Current as App).AppFrame.Navigate(typeof(ContactsPage));
						break;

					case "personalMaps":
						(Application.Current as App).AppFrame.Navigate(typeof(CategorySelectPage));
						break;

					case "scheduleSync":
						try
						{
							var resultOfSynchronization = await Model.UserRequests.SynchronizationRequestAsync();
							if (resultOfSynchronization == 0)
							{
								ShowToastNotification(0);
							}

							if (resultOfSynchronization == 1)
							{
								ShowToastNotification(1);
							}
						}
						catch (Exception)
						{
							ShowToastNotification(1);
						}

						break;

					case "help":
						await new MessageDialog("Помощь пользователя находится в разработке").ShowAsync();
						break;

					default:
						break;
				}
			}
		}

		/// <summary>
		/// Send push-notification to windows
		/// </summary>
		private void ShowToastNotification(int res)
		{
			string message = res == 0 ? "Синхронизация завершена успешно" : "Ошибка во время синхронизации";

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
								Text = "Синхронизация событий",
								HintMaxLines = 1
							},
							new AdaptiveText()
							{
								Text = $"Начало синхронизации: {DateTime.Now.ToShortTimeString()}"
							},
							new AdaptiveText()
							{
								Text = message
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
				Width = 350,
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

				//(Application.Current as App).AppFrame.Navigate(typeof(SignInPage));
				if (Window.Current.Content is Frame frame)
				{
					frame.Navigate(typeof(SignInPage));
				}
			}
		}
	}
}