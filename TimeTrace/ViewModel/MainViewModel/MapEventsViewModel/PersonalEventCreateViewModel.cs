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
		/// Event start time
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

		private int startTabIndex;
		/// <summary>
		/// Start index of pivot
		/// </summary>
		public int StartTabIndex
		{
			get => startTabIndex;
			set
			{
				startTabIndex = value;

				if (startTabIndex == 0)
				{
					Header = "Создание";
					ActionName = "Создать событие";
				}
				else
				{
					Header = "Редактирование";
					ActionName = "Изменить событие";
				}

				OnPropertyChanged();
			}
		}

		/// <summary>
		/// The title of event create or edit tab
		/// </summary>
		public string Header { get; set; }
		/// <summary>
		/// Content of create of edit map event button
		/// </summary>
		public string ActionName { get; set; }

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		public PersonalEventCreateViewModel()
		{
			StartPageViewModel.Instance.SetHeader(StartPageViewModel.Headers.MapEvents);
			StartTabIndex = 0;

			CurrentMapEvent = new MapEvent();
			StartDate = DateTime.Now;

			MinDate = DateTime.Today;
			IsNotAllDay = IsBindingForWindowsCalendar = false;
			MapEventsPersonsSuggestList = new ObservableCollection<string>();
		}

		/// <summary>
		/// Standart constructor for map event editing
		/// </summary>
		/// <param name="mapEvent"></param>
		public PersonalEventCreateViewModel(MapEvent mapEvent) : this()
		{
			// This part for correct editing. Variable assignment doesn't work here
			CurrentMapEvent.Color = mapEvent.Color;
			CurrentMapEvent.CreateAt = mapEvent.CreateAt;
			CurrentMapEvent.Description = mapEvent.Description;
			CurrentMapEvent.EmailOfOwner = mapEvent.EmailOfOwner;
			CurrentMapEvent.EventInterval = mapEvent.EventInterval;
			CurrentMapEvent.Id = mapEvent.Id;
			CurrentMapEvent.IsPublic = mapEvent.IsPublic;
			CurrentMapEvent.Location = mapEvent.Location;
			CurrentMapEvent.Name = mapEvent.Name;
			CurrentMapEvent.ProjectId = mapEvent.ProjectId;
			CurrentMapEvent.ProjectOwnerEmail = mapEvent.ProjectOwnerEmail;
			CurrentMapEvent.UpdateAt = mapEvent.UpdateAt;
			CurrentMapEvent.UserBind = mapEvent.UserBind;

			StartTabIndex = 1;

			StartDate = mapEvent.Start.ToLocalTime();
			EndDate = mapEvent.End.ToLocalTime();
			StartTime = TimeSpan.Parse(mapEvent.Start.ToLocalTime().ToShortTimeString());
			EndTime = TimeSpan.Parse(mapEvent.End.ToLocalTime().ToShortTimeString());

			if (mapEvent.Start.ToLocalTime().Date == mapEvent.End.ToLocalTime().Date
				&& mapEvent.Start.ToLocalTime().Hour == 0
				&& mapEvent.Start.ToLocalTime().Minute == 0
				&& mapEvent.End.ToLocalTime().Hour == 23
				&& mapEvent.End.ToLocalTime().Minute == 59)
			{
				IsNotAllDay = false;
			}
			else
			{
				IsNotAllDay = true;
			}
		}

		/// <summary>
		/// Creating a new event
		/// </summary>
		/// <returns>Result of event creation</returns>
		public async Task EventCreateAsync()
		{
			if (string.IsNullOrEmpty(CurrentMapEvent.Name?.Trim()) || StartDate == null || (EndDate == null && IsNotAllDay))
			{
				await (new MessageDialog("Не заполнено одно из обязательных полей",
					(StartTabIndex == 0) ? "Ошибка создания нового события" : "Ошибка изменения события")).ShowAsync();

				return;
			}

			if (CurrentMapEvent.Start > CurrentMapEvent.End)
			{
				await (new MessageDialog("Дата начала не может быть позже даты окончания события", "Ошибка создания нового события")).ShowAsync();

				return;
			}

			MapEvent savedMapEvent = new MapEvent(
				CurrentMapEvent.Name,
				CurrentMapEvent.Description,
				CurrentMapEvent.Start,
				CurrentMapEvent.End,
				CurrentMapEvent.Location,
				CurrentMapEvent.UserBind,
				"Не повторяется",
				CurrentMapEvent.IsPublic,
				CurrentMapEvent.Color,
				CurrentMapEvent.ProjectId);

			if (IsBindingForWindowsCalendar)
			{
				BindingEventToWindowsCalendarAsync(savedMapEvent);
			}


			using (MainDatabaseContext db = new MainDatabaseContext())
			{
				if (StartTabIndex == 0)
				{
					db.MapEvents.Add(savedMapEvent);
					db.SaveChanges();
				}
				else
				{
					savedMapEvent.Id = CurrentMapEvent.Id;
					savedMapEvent.ProjectId = CurrentMapEvent.ProjectId;
					savedMapEvent.CreateAt = CurrentMapEvent.CreateAt;
					savedMapEvent.UpdateAt = DateTime.UtcNow;

					db.Update(savedMapEvent);
					db.SaveChanges();
				}
			}

			NewMapEventNotification(savedMapEvent.Name, savedMapEvent.Start.ToLocalTime());
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
		private async void BindingEventToWindowsCalendarAsync(MapEvent mapEvent)
		{
			var appointment = new Windows.ApplicationModel.Appointments.Appointment();

			// StartTime
			var date = StartDate;
			var time = StartTime;
			var timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
			var startTime = new DateTimeOffset(date.Value.Year, date.Value.Month, date.Value.Day, time.Hours, time.Minutes, 0, timeZoneOffset);
			appointment.StartTime = startTime;

			// Subject
			appointment.Subject = mapEvent.Name;

			// Location
			appointment.Location = mapEvent.Location ?? "Default";

			// Details
			appointment.Details = mapEvent.Description;

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
								Text = (StartTabIndex == 0) ? "Создано новое событие" : "Событие изменено",
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