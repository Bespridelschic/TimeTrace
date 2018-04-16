using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TimeTrace.Model.DBContext;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using TimeTrace.Model;
using TimeTrace.Model.Requests;
using TimeTrace.View.MainView;

namespace TimeTrace
{
	/// <summary>
	/// Обеспечивает зависящее от конкретного приложения поведение, дополняющее класс Application по умолчанию.
	/// </summary>
	sealed partial class App : Application
	{
		/// <summary>
		/// Инициализирует одноэлементный объект приложения.  Это первая выполняемая строка разрабатываемого
		/// кода; поэтому она является логическим эквивалентом main() или WinMain().
		/// </summary>
		public App()
		{
			IsIdDeviceAvailable();
			//AppSignInWithToken().GetAwaiter();

			this.InitializeComponent();
			this.Suspending += OnSuspending;

			using (var db = new MainDatabaseContext())
			{
				db.Database.Migrate();
			}
		}

		/// <summary>
		/// Global application frame
		/// </summary>
		public Frame AppFrame { get; set; }

		/// <summary>
		/// Вызывается при обычном запуске приложения пользователем.  Будут использоваться другие точки входа,
		/// например, если приложение запускается для открытия конкретного файла.
		/// </summary>
		/// <param name="e">Сведения о запросе и обработке запуска.</param>
		protected override void OnLaunched(LaunchActivatedEventArgs e)
		{
			if (!(Window.Current.Content is Frame))
			{
				AppFrame = new Frame();

				AppFrame.NavigationFailed += OnNavigationFailed;

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
				{

				}

				Window.Current.Content = AppFrame;
			}

			if (e.PrelaunchActivated == false)
			{
				if (AppFrame.Content == null)
				{
					AppFrame.Navigate(typeof(View.MainView.StartPage), e.Arguments);
					//AppFrame.Navigate(typeof(View.AuthenticationView.SignInPage), e.Arguments);
				}
				// Обеспечение активности текущего окна
				Window.Current.Activate();

				// Скрытие TitleBar
				ExtendAcrylicIntoTitleBar();

				// Активация навигации
				SystemNavigationManager.GetForCurrentView().BackRequested += ((sender, args) =>
				{
					if (AppFrame.CanGoBack)
					{
						AppFrame.GoBack();
						args.Handled = true;
					}
				});

				// Лямбда для навигации
				AppFrame.Navigated += (s, args) =>
				{
					if (AppFrame.CanGoBack) // если можно перейти назад, показываем кнопку
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
		}

		/// <summary>
		/// Скрытие TitleBar
		/// </summary>
		private void ExtendAcrylicIntoTitleBar()
		{
			CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
			ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
			titleBar.ButtonBackgroundColor = Colors.Transparent;
			titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
		}

		/// <summary>
		/// Вызывается в случае сбоя навигации на определенную страницу
		/// </summary>
		/// <param name="sender">Фрейм, для которого произошел сбой навигации</param>
		/// <param name="e">Сведения о сбое навигации</param>
		private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}

		/// <summary>
		/// Вызывается при приостановке выполнения приложения.  Состояние приложения сохраняется
		/// без учета информации о том, будет ли оно завершено или возобновлено с неизменным
		/// содержимым памяти.
		/// </summary>
		/// <param name="sender">Источник запроса приостановки.</param>
		/// <param name="e">Сведения о запросе приостановки.</param>
		private void OnSuspending(object sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();
			//TODO: Сохранить состояние приложения и остановить все фоновые операции
			deferral.Complete();
		}

		/// <summary>
		/// Попытка входа в систему с помощью токена
		/// </summary>
		private async Task AppSignInWithToken()
		{
			var res = await Model.Requests.FileSystemRequests.LoadUserEmailAndTokenFromFileAsync();

			if (string.IsNullOrEmpty(res.email) || string.IsNullOrEmpty(res.token))
			{
				return;
			}

			try
			{
				var requestResult = await Model.Requests.InternetRequests.PostRequestAsync(Model.Requests.InternetRequests.PostRequestDestination.SignInWithToken);

				if (requestResult == 0)
				{
					await InternetRequests.ContactsSynchronizationRequestAsync();

					var CurrentUser = await InternetRequests.PostRequestAsync();

					// Save user local data for using after sign in
					ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
					localSettings.Values["email"] = CurrentUser.Email.ToLower();
					localSettings.Values["lastName"] = CurrentUser.LastName;
					localSettings.Values["firstName"] = CurrentUser.FirstName;
					localSettings.Values["middleName"] = CurrentUser.MiddleName;
					localSettings.Values["birthday"] = CurrentUser.Birthday;

					if (Window.Current.Content is Frame frame)
					{
						frame.Navigate(typeof(StartPage));
					}
				}
			}
			catch (Exception ex)
			{
				await (new Windows.UI.Popups.MessageDialog($"{ex.Message}\n" +
					$"Ошибка входа, удаленный сервер не доступен. Повторите попытку позже", "Ошибка входа")).ShowAsync();
			}
		}

		/// <summary>
		/// Create id if not found
		/// </summary>
		private async void IsIdDeviceAvailable()
		{
			try
			{
				StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

				var tryGetFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("_dif.bin");
				if (tryGetFile == null)
				{
					StorageFile storageFile = await storageFolder.CreateFileAsync("_dif.bin", CreationCollisionOption.ReplaceExisting);

					string[] stringToFile = {Guid.NewGuid().ToString()};

					await FileIO.WriteLinesAsync(storageFile, stringToFile);
				}

				StorageFile deviceIdFile = await storageFolder.GetFileAsync("_dif.bin");
				var fileLines = await (FileIO.ReadLinesAsync(deviceIdFile));

				if (fileLines.Count <= 2)
				{
					ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
					localSettings.Values["DeviceId"] = fileLines[0];
				}
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}