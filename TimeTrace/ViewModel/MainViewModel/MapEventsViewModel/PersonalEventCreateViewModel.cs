using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TimeTrace.Model;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TimeTrace.View.MainView.PersonalMapsCreatePages;
using Windows.UI.Popups;
using System.Xml.Linq;
using System.Diagnostics;
using System.Runtime.Serialization.Json;
using System.IO;
using TimeTrace.Model.DBContext;
using Windows.UI;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
using TimeTrace.Model.Events;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace TimeTrace.ViewModel.MainViewModel.MapEventsViewModel
{
	/// <summary>
	/// ViewModel of creation new event
	/// </summary>
	public class PersonalEventCreateViewModel : BaseViewModel
	{
		#region Properties

		private MapEvent currentMapEvent;
		/// <summary>
		/// Current event model
		/// </summary>
		public MapEvent CurrentMapEvent
		{
			get => currentMapEvent;
			set
			{
				currentMapEvent = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Min updateAt - current day
		/// </summary>
		public DateTime MinDate { get; }

		private DateTimeOffset? startDate;
		/// <summary>
		/// Event start updateAt
		/// </summary>
		public DateTimeOffset? StartDate
		{
			get => startDate;
			set
			{
				startDate = value;
				if (StartDate != null)
				{
					CurrentMapEvent.Start = StartDate.Value.Date + StartTime;
				}

				if (!isNotAllDay)
				{
					EndDate = value;
				}

				OnPropertyChanged();
			}
		}

		private TimeSpan startTime;
		/// <summary>
		/// Event start time
		/// </summary>
		public TimeSpan StartTime
		{
			get => startTime;
			set
			{
				if (startTime != value)
				{
					startTime = value;
					if (StartDate != null)
					{
						CurrentMapEvent.Start = StartDate.Value.Date + StartTime;
					}

					OnPropertyChanged();
				}
			}
		}

		private DateTimeOffset? endDate;
		/// <summary>
		/// Event end updateAt
		/// </summary>
		public DateTimeOffset? EndDate
		{
			get => endDate;
			set
			{
				endDate = value;
				if (endDate != null)
				{
					CurrentMapEvent.End = EndDate.Value.Date + EndTime;
				}

				OnPropertyChanged();
			}
		}

		private TimeSpan endTime;
		/// <summary>
		/// Event end time
		/// </summary>
		public TimeSpan EndTime
		{
			get => endTime;
			set
			{
				if (endTime != value)
				{
					endTime = value;
					if (endDate != null)
					{
						CurrentMapEvent.End = EndDate.Value.Date + EndTime;
					}

					OnPropertyChanged();
				}
			}
		}

		private bool isNotAllDay;
		/// <summary>
		/// Is all day selected
		/// </summary>
		public bool IsNotAllDay
		{
			get => isNotAllDay;
			set
			{
				isNotAllDay = value;

				if (!isNotAllDay)
				{
					StartTime = TimeSpan.Parse("00:00");
					EndTime = TimeSpan.Parse("23:59");
					EndDate = StartDate;
				}

				OnPropertyChanged();
			}
		}

		private bool isBindingForWindowsCalendar;
		/// <summary>
		/// Is binding for windows calendar selected
		/// </summary>
		public bool IsBindingForWindowsCalendar
		{
			get => isBindingForWindowsCalendar;
			set
			{
				isBindingForWindowsCalendar = value;
				OnPropertyChanged();
			}
		}

		private ObservableCollection<string> mapEventsPersonsSuggestList;
		/// <summary>
		/// Persons filter tips
		/// </summary>
		public ObservableCollection<string> MapEventsPersonsSuggestList
		{
			get => mapEventsPersonsSuggestList;
			set
			{
				mapEventsPersonsSuggestList = value;
				OnPropertyChanged();
			}
		}

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		public PersonalEventCreateViewModel()
		{
			CurrentMapEvent = new MapEvent();
			StartDate = DateTime.Now;

			MinDate = DateTime.Today;
			IsNotAllDay = IsBindingForWindowsCalendar = false;
			MapEventsPersonsSuggestList = new ObservableCollection<string>();
		}

		/// <summary>
		/// Creating a new event
		/// </summary>
		/// <returns>Result of event creation</returns>
		public async Task EventCreateAsync()
		{
			if (string.IsNullOrEmpty(CurrentMapEvent.Name?.Trim()) || StartDate == null || (EndDate == null && IsNotAllDay))
			{
				await (new MessageDialog("Не заполнено одно из обязательных полей", "Ошибка создания нового события")).ShowAsync();

				return;
			}

			if (!IsNotAllDay)
			{
				EndTime = TimeSpan.Parse("23:59");
				EndDate = StartDate;
			}

			if (CurrentMapEvent.Start > CurrentMapEvent.End)
			{
				await (new MessageDialog("Дата начала не может быть позже даты окончания события", "Ошибка создания нового события")).ShowAsync();

				return;
			}

			// Trim name and description of map event
			CurrentMapEvent.Name = CurrentMapEvent.Name.Trim();

			if (!string.IsNullOrEmpty(CurrentMapEvent.Description?.Trim()))
			{
				CurrentMapEvent.Description = CurrentMapEvent.Description.Trim();
			}
			else
			{
				CurrentMapEvent.Description = string.Empty;
			}

			// Set update date as now
			CurrentMapEvent.UpdateAt = DateTime.UtcNow;
			CurrentMapEvent.IsDelete = false;

			// Convert dates to UTC
			CurrentMapEvent.Start = CurrentMapEvent.Start.ToUniversalTime();
			CurrentMapEvent.End = CurrentMapEvent.End.ToUniversalTime();

			if (!string.IsNullOrEmpty(CurrentMapEvent.UserBind?.Trim()))
			{
				CurrentMapEvent.UserBind = CurrentMapEvent.UserBind.Trim();
			}
			else
			{
				CurrentMapEvent.UserBind = string.Empty;
			}

			if (!string.IsNullOrEmpty(CurrentMapEvent.Location?.Trim()))
			{
				CurrentMapEvent.Location = CurrentMapEvent.Location.Trim();
			}
			else
			{
				CurrentMapEvent.Location = string.Empty;
			}

			CurrentMapEvent.EventInterval = "Не повторяется";

			if (IsBindingForWindowsCalendar)
			{
				BindingEventToWindowsCalendarAsync();
			}

			using (MainDatabaseContext db = new MainDatabaseContext())
			{
				db.MapEvents.Add(CurrentMapEvent);
				db.SaveChanges();
			}

			NewMapEventNotification(CurrentMapEvent.Name, CurrentMapEvent.Start.ToLocalTime());
		}

		/// <summary>
		/// Filtration of input persons
		/// </summary>
		/// <param name="sender">Input filter</param>
		/// <param name="args">Event args</param>
		public void MapEventsPersonsFilter(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (args.CheckCurrent())
			{
				MapEventsPersonsSuggestList.Clear();
			}

			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

			if (!string.IsNullOrEmpty(sender.Text))
			{
				MapEventsPersonsSuggestList.Clear();

				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					foreach (var i in db.Contacts
						.Where(i => (i.Name.ToLowerInvariant().Contains(sender.Text.ToLowerInvariant())) &&
									i.EmailOfOwner == (string)localSettings.Values["email"] &&
									!i.IsDelete)
						.Select(i => i))
					{
						if (!MapEventsPersonsSuggestList.Contains(i.Name))
						{
							MapEventsPersonsSuggestList.Add(i.Name);
						}
					}
				}
			}
		}

		/// <summary>
		/// Open Windows 10 calendar
		/// </summary>
		private async void BindingEventToWindowsCalendarAsync()
		{
			var appointment = new Windows.ApplicationModel.Appointments.Appointment();

			// StartTime
			var date = StartDate;
			var time = StartTime;
			var timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
			var startTime = new DateTimeOffset(date.Value.Year, date.Value.Month, date.Value.Day, time.Hours, time.Minutes, 0, timeZoneOffset);
			appointment.StartTime = startTime;

			// Subject
			appointment.Subject = CurrentMapEvent.Name;

			// Location
			appointment.Location = CurrentMapEvent.Location ?? "Default";

			// Details
			appointment.Details = CurrentMapEvent.Description;

			// Duration
			appointment.Duration = EndTime - StartTime;

			// All Day
			appointment.AllDay = !IsNotAllDay;

			// Reminder
			appointment.Reminder = TimeSpan.FromMinutes(15);

			//Busy Status
			appointment.BusyStatus = Windows.ApplicationModel.Appointments.AppointmentBusyStatus.Busy;

			// Sensitivity
			appointment.Sensitivity = Windows.ApplicationModel.Appointments.AppointmentSensitivity.Public;

			// ShowAddAppointmentAsync returns an appointment id if the appointment given was added to the user's calendar.
			// This value should be stored in app data and roamed so that the appointment can be replaced or removed in the future.
			// An empty string return value indicates that the user canceled the operation before the appointment was added.
			String appointmentId = await Windows.ApplicationModel.Appointments.AppointmentManager.ShowAddAppointmentAsync(
				appointment, new Windows.Foundation.Rect(), Placement.Default);
			Debug.WriteLine(appointmentId);
		}

		/// <summary>
		/// Get color from HEX format to SolidColorBrush
		/// </summary>
		/// <param name="color">HEX string of color</param>
		/// <returns><see cref="SolidColorBrush"/> object</returns>
		private SolidColorBrush GetColorFromString(string color)
		{
			var colorString = color.Replace("#", string.Empty);

			var r = byte.Parse(colorString.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
			var g = byte.Parse(colorString.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
			var b = byte.Parse(colorString.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

			Color localColor = Color.FromArgb(255, r, g, b);
			return new SolidColorBrush(localColor);
		}

		/// <summary>
		/// Map event create message
		/// </summary>
		private void NewMapEventNotification(string name, DateTime start)
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
								Text = "Создано новое событие",
								HintMaxLines = 1
							},
							new AdaptiveText()
							{
								Text = name,
							},
							new AdaptiveText()
							{
								Text = $"Начало в {start.ToShortTimeString()}"
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
	}
}