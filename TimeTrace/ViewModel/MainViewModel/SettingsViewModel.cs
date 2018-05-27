using System;
using Windows.ApplicationModel.Resources;
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
								AppRestart("Changing global properties");

								break;
							}
						case 1:
							{
								Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en-US";
								AppRestart("Changing global properties");

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

		/// <summary>
		/// Current language used in application
		/// </summary>
		public string CurrentLanguage { get; set; }

		/// <summary>
		/// Localization resource loader
		/// </summary>
		public ResourceLoader ResourceLoader { get; set; }

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		public SettingsViewModel()
		{
			ResourceLoader = ResourceLoader.GetForCurrentView("HomeVM");
			StartPageViewModel.Instance.SetHeader(StartPageViewModel.Headers.Settings);

			if ((Application.Current as App).AppFrame.Language.ToLowerInvariant().Contains("ru"))
			{
				CurrentLanguage = "Русский язык";
				selectedLanguage = 0;
			}

			if ((Application.Current as App).AppFrame.Language.ToLowerInvariant().Contains("en"))
			{
				CurrentLanguage = "English";
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
				Text = ResourceLoader.GetString("/SettingsVM/AboutApplication"),
				TextWrapping = TextWrapping.WrapWholeWords,
				HorizontalAlignment = HorizontalAlignment.Left,
				FontSize = 35,
				FontWeight = new Windows.UI.Text.FontWeight() { Weight = 9 },
				Margin = new Thickness(0, 0, 0, 10),
			};

			TextBlock mainContent = new TextBlock()
			{
				Text = ResourceLoader.GetString("/SettingsVM/AboutApplicationType"),
				TextWrapping = TextWrapping.WrapWholeWords,
				HorizontalAlignment = HorizontalAlignment.Left,
				FontSize = 18,
				FontWeight = new Windows.UI.Text.FontWeight() { Weight = 5 },
			};

			TextBlock licenceInfo = new TextBlock()
			{
				Text = ResourceLoader.GetString("/SettingsVM/AboutApplicationLicence"),
				TextWrapping = TextWrapping.WrapWholeWords,
				HorizontalAlignment = HorizontalAlignment.Left,
				FontSize = 18,
				FontWeight = new Windows.UI.Text.FontWeight() { Weight = 5 },
			};

			TextBlock rightsInfo = new TextBlock()
			{
				Text = ResourceLoader.GetString("/SettingsVM/AboutApplicationRights"),
				TextWrapping = TextWrapping.WrapWholeWords,
				HorizontalAlignment = HorizontalAlignment.Left,
				FontSize = 18,
				FontWeight = new Windows.UI.Text.FontWeight() { Weight = 5 },
			};

			StackPanel mainPanel = new StackPanel();
			mainPanel.Children.Add(title);
			mainPanel.Children.Add(mainContent);
			mainPanel.Children.Add(licenceInfo);
			mainPanel.Children.Add(rightsInfo);

			ContentDialog information = new ContentDialog()
			{
				Content = mainPanel,
				CloseButtonText = ResourceLoader.GetString("/SettingsVM/Close"),
				DefaultButton = ContentDialogButton.Close,
			};

			await information.ShowAsync();
		}

		/// <summary>
		/// Restart the app
		/// </summary>
		public async void AppRestart(string reason)
		{
			StackPanel mainText = new StackPanel();
			mainText.Children.Add(new TextBlock() { Text = ResourceLoader.GetString("/SettingsVM/RestartMessage") });
			mainText.Children.Add(new TextBlock() { Text = ResourceLoader.GetString("/SettingsVM/RestartMessageEnd"), Margin = new Thickness(0, 0, 0, 10) });
			mainText.Children.Add(new TextBlock() { Text = ResourceLoader.GetString("/SettingsVM/RestartQuestion") });

			ContentDialog appRestartResolution = new ContentDialog()
			{
				Title = ResourceLoader.GetString("/SettingsVM/RestartConfirm"),
				Content = mainText,
				PrimaryButtonText = ResourceLoader.GetString("/SettingsVM/Restart"),
				CloseButtonText = ResourceLoader.GetString("/SettingsVM/Later"),
				DefaultButton = ContentDialogButton.Primary
			};

			if ((await appRestartResolution.ShowAsync()) == ContentDialogResult.Primary)
			{
				Windows.ApplicationModel.Core.AppRestartFailureReason result =
					await Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync(reason);
			}
		}
	}
}