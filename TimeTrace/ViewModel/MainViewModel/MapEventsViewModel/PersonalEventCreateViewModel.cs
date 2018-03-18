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
using TimeTrace.Model.Events.DBContext;
using Windows.UI;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
using TimeTrace.Model.Events;

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

		private bool isSelectMan;
		/// <summary>
		/// Enabled of man binding
		/// </summary>
		public bool IsSelectMan
		{
			get => isSelectMan;
			set
			{
				isSelectMan = value;
				OnPropertyChanged();
			}
		}

		private bool isSelectPlace;
		/// <summary>
		/// Enabled of location binding
		/// </summary>
		public bool IsSelectPlace
		{
			get => isSelectPlace;
			set
			{
				isSelectPlace = value;
				OnPropertyChanged();
			}
		}

		private int bindingObjectIndex;
		/// <summary>
		/// Binding object of event
		/// </summary>
		public int BindingObjectIndex
		{
			get => bindingObjectIndex;
			set
			{
				bindingObjectIndex = value;
				switch (value)
				{
					case 0:
						{
							IsSelectPlace = true;
							IsSelectMan = false;

							break;
						}
					case 1:
						{
							IsSelectMan = true;
							IsSelectPlace = false;

							break;
						}
					case 2:
						{
							IsSelectPlace = true;
							IsSelectMan = true;

							break;
						}
					default:
						throw new Exception("Не определенный индекс привязки события!");
				}
				OnPropertyChanged();
			}
		}

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
					EndDate = null;
					EndTime = TimeSpan.Parse("00:00");
				}

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
			IsNotAllDay = false;

			BindingObjectIndex = 0;
		}

		/// <summary>
		/// Creating a new event
		/// </summary>
		/// <returns>Result of event creation</returns>
		public async Task EventCreate()
		{
			if (string.IsNullOrEmpty(CurrentMapEvent.Name) || StartDate == null || (EndDate == null && IsNotAllDay))
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

			CurrentMapEvent.UpdateAt = DateTime.Now;
			CurrentMapEvent.IsDelete = false;

			if (CurrentMapEvent.Location == null)
			{
				CurrentMapEvent.Location = string.Empty;
			}

			if (CurrentMapEvent.UserBind == null)
			{
				CurrentMapEvent.UserBind = string.Empty;
			}

			if (string.IsNullOrEmpty(CurrentMapEvent.Description))
			{
				CurrentMapEvent.Description = null;
			}

			if (string.IsNullOrEmpty(CurrentMapEvent.UserBind))
			{
				CurrentMapEvent.UserBind = null;
			}

			if (string.IsNullOrEmpty(CurrentMapEvent.Location))
			{
				CurrentMapEvent.Location = null;
			}

			BindingEventToWindowsCalendar();

			using (MapEventContext db = new MapEventContext())
			{
				db.MapEvents.Add(CurrentMapEvent);
				db.SaveChanges();
			}

			NewMapEventNotification(CurrentMapEvent.Name, CurrentMapEvent.Start);
		}

		private async void BindingEventToWindowsCalendar()
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