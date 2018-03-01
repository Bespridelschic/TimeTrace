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

		public int selectedLanguage;
		/// <summary>
		/// Selected UI language
		/// </summary>
		public int SelectedLanguage
		{
			get { return selectedLanguage; }
			set
			{
				selectedLanguage = value;
				string currentLanguage;

				if (Windows.UI.Xaml.Window.Current.Content is Windows.UI.Xaml.Controls.Frame frame)
				{
					currentLanguage = frame.Language;

					switch (value)
					{
						case 0:
							{

								break;
							}
						case 1:
							{

								break;
							}
						case 2:
							{
								Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "ru-RU";
								if (currentLanguage != "ru")
								{
									AppRestart();
								}

								break;
							}
						case 3:
							{
								Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en-US";
								if (currentLanguage != "en-US")
								{
									AppRestart();
								}

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

		private List<string> fonts;
		/// <summary>
		/// Available fonts
		/// </summary>
		public List<string> Fonts
		{
			get { return fonts; }
			set
			{
				fonts = value;
			}
		}

		#endregion

		public SettingsViewModel()
		{
			Fonts = new List<string>()
			{
				"Текущий", "Comic Sans MC", "Courier New", "Segoe UI", "Timew New Roman"
			};
		}

		public async void Foo()
		{
			if (Window.Current.Content is Frame frame)
			{
				await (new MessageDialog($"{frame.Language}")).ShowAsync();
			}
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
