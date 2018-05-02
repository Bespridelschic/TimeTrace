using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TimeTrace.ViewModel.MainViewModel
{
	public class SettingsViewModel : BaseViewModel
	{
		#region Properties

		private int selectedLanguage;
		/// <summary>
		/// Selected UI language
		/// </summary>
		public int SelectedLanguage
		{
			get => selectedLanguage;
			set
			{
				// If selected language isn't current
				if (!(((Application.Current as App).AppFrame.Language.ToLowerInvariant().Contains("ru") && value == 0)
					|| (Application.Current as App).AppFrame.Language.ToLowerInvariant().Contains("en") && value == 1))
				{
					selectedLanguage = value;

					switch (value)
					{
						case 0:
							{
								Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "ru-RU";
								break;
							}
						case 1:
							{
								Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en-US";
								break;
							}
						default:
							{
								throw new IndexOutOfRangeException("Selected language is not defined");
							}
					}
				}
			}
		}

		public string CurrentLanguage { get; set; }

		#endregion

		public SettingsViewModel()
		{
			if ((Application.Current as App).AppFrame.Language.ToLowerInvariant().Contains("ru"))
			{
				CurrentLanguage = "Русский язык";
				selectedLanguage = 0;
			}

			if ((Application.Current as App).AppFrame.Language.ToLowerInvariant().Contains("en"))
			{
				CurrentLanguage = "Английский язык";
				selectedLanguage = 1;
			}
		}

		/// <summary>
		/// Show information about application
		/// </summary>
		public async void AppInfo()
		{
			TextBlock title = new TextBlock()
			{
				Text = "Информация о приложении",
				TextWrapping = TextWrapping.WrapWholeWords,
				HorizontalAlignment = HorizontalAlignment.Left,
				FontSize = 35,
				FontWeight = new Windows.UI.Text.FontWeight() { Weight = 9 },
				Margin = new Thickness(0, 0, 0, 10),
			};

			TextBlock mainContent = new TextBlock()
			{
				Text = "Дипломный проект\nРаспространяется под лицензией Freeware\n(c) Все права защищены 2018",
				TextWrapping = TextWrapping.WrapWholeWords,
				HorizontalAlignment = HorizontalAlignment.Left,
				FontSize = 18,
				FontWeight = new Windows.UI.Text.FontWeight() { Weight = 5 },
			};

			StackPanel mainPanel = new StackPanel();
			mainPanel.Children.Add(title);
			mainPanel.Children.Add(mainContent);

			ContentDialog information = new ContentDialog()
			{
				Content = mainPanel,
				CloseButtonText = "Закрыть",
				DefaultButton = ContentDialogButton.Close,
			};

			await information.ShowAsync();
		}

		/// <summary>
		/// Restart the app
		/// </summary>
		public async void AppRestart()
		{
			Windows.ApplicationModel.Core.AppRestartFailureReason result =
				await Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync("Changing global properties");
		}
	}
}
