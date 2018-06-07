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
using TimeTrace.View.MainView;

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

		private DateTimeOffset? maxDate;
		/// <summary>
		/// Max date for custom repeat setting
		/// </summary>
		public DateTimeOffset? MaxDate
		{
			get => maxDate;
			set
			{
				maxDate = value;
				OnPropertyChanged();
			}
		}

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
					BeforeDateRecurrence = value;
				}

				if (!isNotAllDay)
				{
					EndDate = value;
				}

				if (value.HasValue)
				{
					// Set max date as start + 1 year
					MaxDate = value.Value.AddYears(1);
				}

				if (SelectedRepeatMode == RepeatMode.YearOnce)
				{
					MaxDate = DateTime.Now.AddYears(10);
				}
				else
				{
					if (StartDate.HasValue)
					{
						MaxDate = StartDate.Value.AddYears(1);
					}
					else
					{
						MaxDate = DateTime.Now.AddYears(1);
					}
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

		private bool customRepeatEnable;
		/// <summary>
		/// Is custom setting of event repeat enabled
		/// </summary>
		public bool CustomRepeatEnable
		{
			get => customRepeatEnable;
			set
			{
				customRepeatEnable = value;
				OnPropertyChanged();
			}
		}

		private bool isCustomDateOfWeekSelectingEnable;
		/// <summary>
		/// Is custom setting of event dates enabled for days of week
		/// </summary>
		public bool IsCustomDateOfWeekSelectingEnable
		{
			get => isCustomDateOfWeekSelectingEnable;
			set
			{
				isCustomDateOfWeekSelectingEnable = value;
				OnPropertyChanged();
			}
		}

		private RepeatMode selectedRepeatMode;
		/// <summary>
		/// The selected repeat mode
		/// </summary>
		public RepeatMode SelectedRepeatMode
		{
			get => selectedRepeatMode;
			set
			{
				selectedRepeatMode = value;
				if (value != RepeatMode.NotRepeat)
				{
					CustomRepeatEnable = true;
					IsEndingForDateSelected = true;
				}
				else
				{
					CustomRepeatEnable = false;
				}

				if (value == RepeatMode.Custom)
				{
					IsCustomDateOfWeekSelectingEnable = true;
				}
				else
				{
					IsCustomDateOfWeekSelectingEnable = false;
				}

				if (value == RepeatMode.YearOnce)
				{
					MaxDate = DateTime.Now.AddYears(10);
				}
				else
				{
					if (StartDate.HasValue)
					{
						MaxDate = StartDate.Value.AddYears(1);
					}
					else
					{
						MaxDate = DateTime.Now.AddYears(1);
					}
				}

				OnPropertyChanged();
			}
		}

		private bool isEndingForDateSelected;
		/// <summary>
		/// Repeat event before date
		/// </summary>
		public bool IsEndingForDateSelected
		{
			get => isEndingForDateSelected;
			set
			{
				isEndingForDateSelected = value;
				if (IsEndingForRepeatSelected != !value)
				{
					IsEndingForRepeatSelected = !value;
				}
				BeforeNumberRecurrence = 0;

				if (value)
				{
					if (endRepeatDate.HasValue)
					{
						BeforeDateRecurrence = endRepeatDate;
					}
					else
					{
						BeforeDateRecurrence = StartDate.Value.ToLocalTime();
					}
				}
				else
				{
					BeforeDateRecurrence = null;
				}

				OnPropertyChanged();
			}
		}

		private DateTimeOffset? beforeDateRecurrence;
		/// <summary>
		/// Before the date of recurrence
		/// </summary>
		public DateTimeOffset? BeforeDateRecurrence
		{
			get => beforeDateRecurrence;
			set
			{
				beforeDateRecurrence = value;
				OnPropertyChanged();
			}
		}

		private bool isEndingForRepeatSelected;
		/// <summary>
		/// Repeat event by number
		/// </summary>
		public bool IsEndingForRepeatSelected
		{
			get => isEndingForRepeatSelected;
			set
			{
				isEndingForRepeatSelected = value;
				if (IsEndingForDateSelected != !value)
				{
					IsEndingForDateSelected = !value;
				}

				if (value)
				{
					BeforeDateRecurrence = null;
				}
				OnPropertyChanged();
			}
		}

		private int beforeNumberRecurrence;
		/// <summary>
		/// Before the number of recurrence
		/// </summary>
		public int BeforeNumberRecurrence
		{
			get { return beforeNumberRecurrence; }
			set
			{
				beforeNumberRecurrence = value;
				OnPropertyChanged();
			}
		}

		private ObservableCollection<bool> selectedRepeatedDates;
		/// <summary>
		/// Binded days
		/// </summary>
		public ObservableCollection<bool> SelectedRepeatedDates
		{
			get => selectedRepeatedDates;
			set
			{
				selectedRepeatedDates = value;
				OnPropertyChanged();
			}
		}

		#region Repetition edit data

		/// <summary>
		/// If edited event is repeatable
		/// </summary>
		private readonly bool isEditedEventRepeated;

		/// <summary>
		/// Start mode of edited event
		/// </summary>
		private readonly RepeatMode startRepeatMode;

		/// <summary>
		/// End date of repeated events
		/// </summary>
		private readonly DateTimeOffset? endRepeatDate;

		/// <summary>
		/// Count of repeated events
		/// </summary>
		private readonly int startRepeatNumbers;

		/// <summary>
		/// Start of original event date
		/// </summary>
		private readonly DateTime startEditedDate;

		#endregion

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		public PersonalEventCreateViewModel()
		{
			StartPageViewModel.Instance.SetHeader(Headers.MapEvents);
			StartTabIndex = 0;

			CurrentMapEvent = new MapEvent();
			StartDate = DateTime.Now;

			MinDate = DateTime.Today;
			MaxDate = StartDate.Value.AddYears(1);

			IsNotAllDay = IsBindingForWindowsCalendar = false;
			MapEventsPersonsSuggestList = new ObservableCollection<string>();
			SelectedRepeatedDates = new ObservableCollection<bool>()
			{
				DateTime.Now.DayOfWeek == DayOfWeek.Sunday,
				DateTime.Now.DayOfWeek == DayOfWeek.Monday,
				DateTime.Now.DayOfWeek == DayOfWeek.Tuesday,
				DateTime.Now.DayOfWeek == DayOfWeek.Wednesday,
				DateTime.Now.DayOfWeek == DayOfWeek.Thursday,
				DateTime.Now.DayOfWeek == DayOfWeek.Friday,
				DateTime.Now.DayOfWeek == DayOfWeek.Saturday
			};
		}

		/// <summary>
		/// Standart constructor for map event editing
		/// </summary>
		/// <param name="mapEvent"></param>
		public PersonalEventCreateViewModel(MapEvent mapEvent) : this()
		{
			StartTabIndex = 1;

			if (!string.IsNullOrEmpty(mapEvent.EventInterval))
			{
				using (var db = new MainDatabaseContext())
				{
					var originalMapEvent = db.MapEvents.Where(i => i.Id == mapEvent.EventInterval).FirstOrDefault();

					if (originalMapEvent != null)
					{
						mapEvent = originalMapEvent;
					}
				}
			}

			CurrentMapEvent = mapEvent.Clone() as MapEvent;
			if (CurrentMapEvent != null)
			{
				var localStartDate = CurrentMapEvent.Start.ToLocalTime().Date;
				var localStartTimeHour = CurrentMapEvent.Start.ToLocalTime().Hour;
				var localStartTimeMin = CurrentMapEvent.Start.ToLocalTime().Minute;

				var localEndDate = CurrentMapEvent.End.ToLocalTime().Date;
				var localEndTimeHour = CurrentMapEvent.End.ToLocalTime().Hour;
				var localEndTimeMin = CurrentMapEvent.End.ToLocalTime().Minute;

				StartDate = localStartDate;
				EndDate = localEndDate;

				StartTime = new TimeSpan(localStartTimeHour, localStartTimeMin, 0);
				EndTime = new TimeSpan(localEndTimeHour, localEndTimeMin, 0);

				if (StartDate.Value.Date == EndDate.Value.Date
					&& StartTime.Hours == 0
					&& StartTime.Minutes == 0
					&& EndTime.Hours == 23
					&& EndTime.Minutes == 59)
				{
					IsNotAllDay = false;
				}
				else
				{
					IsNotAllDay = true;
				}

				using (var db = new MainDatabaseContext())
				{
					// Set count of repeated events
					startRepeatNumbers = db.MapEvents.Count(i => i.EventInterval == CurrentMapEvent.Id);
					if (startRepeatNumbers > 0)
					{
						isEditedEventRepeated = true;
						startEditedDate = CurrentMapEvent.Start;

						if (startRepeatNumbers > 3)
						{
							startRepeatNumbers = 3;
						}

						var repeatedList = db.MapEvents.Where(i => i.EventInterval == CurrentMapEvent.Id).OrderBy(i => i.Start).ToList();

						// Set date of ending of repetition
						BeforeDateRecurrence = db.MapEvents
								.Where(i => i.EventInterval == CurrentMapEvent.Id)
								.OrderByDescending(i => i.Start)
								.First().Start.ToLocalTime();

						// Set start date of ending repetition date
						endRepeatDate = BeforeDateRecurrence;

						if (CurrentMapEvent.Start.AddDays(1).ToUniversalTime() == repeatedList[0].Start)
						{
							SelectedRepeatMode = RepeatMode.EveryDay;
							startRepeatMode = RepeatMode.EveryDay;
						}
						else if (CurrentMapEvent.Start.AddDays(2).ToUniversalTime() == repeatedList[0].Start)
						{
							SelectedRepeatMode = RepeatMode.AfterOneDay;
							startRepeatMode = RepeatMode.AfterOneDay;
						}

						else if (CurrentMapEvent.Start.AddDays(7).ToUniversalTime() == repeatedList[0].Start)
						{
							SelectedRepeatMode = RepeatMode.WeekOnce;
							startRepeatMode = RepeatMode.WeekOnce;
						}

						else if (CurrentMapEvent.Start.AddMonths(1).ToUniversalTime() == repeatedList[0].Start)
						{
							SelectedRepeatMode = RepeatMode.MonthOnce;
							startRepeatMode = RepeatMode.MonthOnce;
						}

						else if (CurrentMapEvent.Start.AddYears(1).ToUniversalTime() == repeatedList[0].Start)
						{
							SelectedRepeatMode = RepeatMode.YearOnce;
							startRepeatMode = RepeatMode.YearOnce;
						}
						else
						{
							var startWeek = CurrentMapEvent.Start.AddDays(7 - (int)CurrentMapEvent.Start.DayOfWeek);
							var copies = db.MapEvents.Where(i => i.Start >= startWeek && i.Start <= startWeek.AddDays(7)).OrderBy(i => i.Start).ToList();

							foreach (var item in copies)
							{
								switch (item.Start.ToLocalTime().DayOfWeek)
								{
									case DayOfWeek.Friday:
										SelectedRepeatedDates[5] = true;
										break;
									case DayOfWeek.Monday:
										SelectedRepeatedDates[1] = true;
										break;
									case DayOfWeek.Saturday:
										SelectedRepeatedDates[6] = true;
										break;
									case DayOfWeek.Sunday:
										SelectedRepeatedDates[0] = true;
										break;
									case DayOfWeek.Thursday:
										SelectedRepeatedDates[4] = true;
										break;
									case DayOfWeek.Tuesday:
										SelectedRepeatedDates[2] = true;
										break;
									case DayOfWeek.Wednesday:
										SelectedRepeatedDates[3] = true;
										break;
									default:
										break;
								}
							}

							SelectedRepeatMode = RepeatMode.Custom;
							startRepeatMode = RepeatMode.Custom;
						}
					}
				}
			}
		}

		/// <summary>
		/// Creating a new event or changing selected
		/// </summary>
		/// <returns>Result of event creation or changing</returns>
		public async Task EventCreateAsync()
		{
			if (!await CanCreateEventAsync())
			{
				return;
			}

			MapEvent savedMapEvent = new MapEvent(
				CurrentMapEvent.Name,
				CurrentMapEvent.Description,
				CurrentMapEvent.Start,
				CurrentMapEvent.End,
				CurrentMapEvent.Location,
				CurrentMapEvent.UserBind,
				"",
				CurrentMapEvent.IsPublic,
				CurrentMapEvent.Color,
				CurrentMapEvent.ProjectId)
			{
				ProjectOwnerEmail = CurrentMapEvent.ProjectOwnerEmail,
				EmailOfOwner = CurrentMapEvent.EmailOfOwner
			};

			if (IsBindingForWindowsCalendar)
			{
				BindingEventToWindowsCalendarAsync(savedMapEvent);
			}

			// Count of copies
			int count = 0;
			if (IsEndingForDateSelected)
			{
				if (BeforeDateRecurrence.HasValue)
				{
					switch (SelectedRepeatMode)
					{
						case RepeatMode.NotRepeat:
							count = 0;
							break;
						case RepeatMode.EveryDay:
							count = BeforeDateRecurrence.Value.Date.Subtract(savedMapEvent.Start.ToLocalTime().Date).Days;
							break;
						case RepeatMode.AfterOneDay:
							count = BeforeDateRecurrence.Value.Date.Subtract(savedMapEvent.Start.ToLocalTime().Date).Days / 2;
							break;
						case RepeatMode.WeekOnce:
							count = BeforeDateRecurrence.Value.Date.Subtract(savedMapEvent.Start.ToLocalTime().Date).Days / 7;
							break;
						case RepeatMode.MonthOnce:
							count = BeforeDateRecurrence.Value.Date.Subtract(savedMapEvent.Start.ToLocalTime().Date).Days / 31;
							break;
						case RepeatMode.YearOnce:
							count = BeforeDateRecurrence.Value.Date.Subtract(savedMapEvent.Start.ToLocalTime().Date).Days / 361;
							break;
						case RepeatMode.Custom:
							count = BeforeDateRecurrence.Value.Date.Subtract(savedMapEvent.Start.ToLocalTime().Date).Days / 7;
							break;
						default:
							break;
					}
				}
			}
			else
			{
				count = BeforeNumberRecurrence;
			}

			using (MainDatabaseContext db = new MainDatabaseContext())
			{
				if (StartTabIndex == 0)
				{
					db.MapEvents.Add(savedMapEvent);

					var savedMultipliedEvent = MultiplyEvents(savedMapEvent, SelectedRepeatMode, count, SelectedRepeatedDates);
					if (savedMultipliedEvent.Count != 0)
					{
						if (SelectedRepeatMode == RepeatMode.Custom)
						{
							savedMultipliedEvent.RemoveAll(i => i.Start.ToLocalTime() <= StartDate.Value.ToLocalTime() && i.Start.ToLocalTime() > BeforeDateRecurrence.Value.ToLocalTime());
						}
						db.MapEvents.AddRange(savedMultipliedEvent);
					}

					db.SaveChanges();
				}
				else
				{
					savedMapEvent.Id = CurrentMapEvent.Id;
					savedMapEvent.UpdateAt = DateTime.UtcNow;
					savedMapEvent.CreateAt = CurrentMapEvent.CreateAt;

					List<MapEvent> savedMultipliedEvent;
					if (SelectedRepeatMode == RepeatMode.Custom)
					{
						savedMultipliedEvent = MultiplyEvents(savedMapEvent, SelectedRepeatMode, count, SelectedRepeatedDates);
					}
					else
					{
						savedMultipliedEvent = MultiplyEvents(savedMapEvent, SelectedRepeatMode, count);
					}

					if (savedMultipliedEvent.Count != 0)
					{
						if (SelectedRepeatMode == RepeatMode.Custom)
						{
							savedMultipliedEvent.RemoveAll(i => i.Start <= StartDate && i.Start > BeforeDateRecurrence);
						}
						db.MapEvents.AddRange(savedMultipliedEvent);
					}

					// Update root event
					db.MapEvents.Update(savedMapEvent);
				}

				db.SaveChanges();

				(Application.Current as App).AppFrame.Navigate(typeof(SchedulePage));
			}

			NewMapEventNotification(savedMapEvent.Name, savedMapEvent.Start.ToLocalTime());

			if (StartPageViewModel.Instance.InternetFeaturesEnable)
			{
				// Synchronization of changes with server
				await StartPageViewModel.Instance.CategoriesSynchronization();
			}
		}

		/// <summary>
		/// Remove all copies and return removed ID
		/// </summary>
		/// <param name="mapEvent">Original event</param>
		/// <returns><seealso cref="List{T}" of events ID/></returns>
		private Stack<string> RemoveAllCopies(MapEvent mapEvent)
		{
			using (var db = new MainDatabaseContext())
			{
				var IdAndCreationDate = new Stack<string>(db.MapEvents
					.Where(i => i.EventInterval == mapEvent.Id)
					.OrderBy(i => i.Start)
					.Select(i => i.Id)
					.ToList());

				db.MapEvents
					.RemoveRange(db.MapEvents
						.Where(i => i.EventInterval == mapEvent.Id));

				db.SaveChanges();

				return IdAndCreationDate;
			}
		}

		/// <summary>
		/// Multiplication of events
		/// </summary>
		/// <param name="mapEvent">Multiplicated event</param>
		/// <param name="repeatMode">Mode of repetition</param>
		/// <param name="counts">Amount of copies setted by user</param>
		/// <param name="customOrder">If custom mode of repeat selected, get custom order</param>
		/// <returns></returns>
		private List<MapEvent> MultiplyEvents(MapEvent mapEvent, RepeatMode repeatMode, int counts, ObservableCollection<bool> customOrder = null)
		{
			var temp = (MapEvent)mapEvent.Clone();
			temp.EventInterval = mapEvent.Id;

			List<MapEvent> repeatedEvents = new List<MapEvent>(counts + 10);
			Stack<string> oldCopies = RemoveAllCopies(temp);

			if (counts > 0)
			{
				var countingDate = 1;

				switch (repeatMode)
				{
					case RepeatMode.NotRepeat:
						RemoveAllCopies(temp);
						return repeatedEvents;

					case RepeatMode.EveryDay:
						countingDate = 1;
						for (int i = 0; i < counts; i++)
						{
							var added = (MapEvent)temp.Clone();
							if (oldCopies.Count > 0)
							{
								added.Id = oldCopies.Pop();
							}
							else
							{
								added.Id = Guid.NewGuid().ToString();
							}
							added.Start = added.Start.AddDays(countingDate);
							added.End = added.End.AddDays(countingDate);

							countingDate++;

							repeatedEvents.Add(added);
						}
						return repeatedEvents;

					case RepeatMode.AfterOneDay:
						countingDate = 2;
						for (int i = 0; i < counts; i++)
						{
							var added = (MapEvent)temp.Clone();
							if (oldCopies.Count > 0)
							{
								added.Id = oldCopies.Pop();
							}
							else
							{
								added.Id = Guid.NewGuid().ToString();
							}
							added.Start = temp.Start.AddDays(countingDate);
							added.End = temp.End.AddDays(countingDate);

							countingDate += 2;

							repeatedEvents.Add(added);
						}
						return repeatedEvents;

					case RepeatMode.WeekOnce:
						countingDate = 7;
						for (int i = 0; i < counts; i++)
						{
							var added = (MapEvent)temp.Clone();
							if (oldCopies.Count > 0)
							{
								added.Id = oldCopies.Pop();
							}
							else
							{
								added.Id = Guid.NewGuid().ToString();
							}
							added.Start = temp.Start.AddDays(countingDate);
							added.End = temp.End.AddDays(countingDate);

							countingDate += 7;

							repeatedEvents.Add(added);
						}
						return repeatedEvents;

					case RepeatMode.MonthOnce:
						countingDate = 1;
						for (int i = 0; i < counts; i++)
						{
							var added = (MapEvent)temp.Clone();
							if (oldCopies.Count > 0)
							{
								added.Id = oldCopies.Pop();
							}
							else
							{
								added.Id = Guid.NewGuid().ToString();
							}
							added.Start = temp.Start.AddMonths(countingDate);
							added.End = temp.End.AddMonths(countingDate);

							countingDate++;

							repeatedEvents.Add(added);
						}
						return repeatedEvents;

					case RepeatMode.YearOnce:
						countingDate = 1;
						var localRecurrence = counts > 100 ? 100 : counts;
						for (int i = 0; i < localRecurrence; i++)
						{
							var added = (MapEvent)temp.Clone();
							if (oldCopies.Count > 0)
							{
								added.Id = oldCopies.Pop();
							}
							else
							{
								added.Id = Guid.NewGuid().ToString();
							}
							added.Start = temp.Start.AddYears(countingDate);
							added.End = temp.End.AddYears(countingDate);

							countingDate++;

							repeatedEvents.Add(added);
						}
						return repeatedEvents;

					case RepeatMode.Custom:
						temp.Start = temp.Start.AddDays((int)temp.Start.DayOfWeek * -1);
						temp.End = temp.End.AddDays((int)temp.End.DayOfWeek * -1);

						if (SelectedRepeatedDates[0] == true)
						{
							countingDate = 7;
							for (int i = 0; i < counts; i++)
							{
								var added = (MapEvent)temp.Clone();
								if (oldCopies.Count > 0)
								{
									added.Id = oldCopies.Pop();
								}
								else
								{
									added.Id = Guid.NewGuid().ToString();
								}
								added.Start = temp.Start.AddDays(countingDate);
								added.End = temp.End.AddDays(countingDate);

								countingDate += 7;

								repeatedEvents.Add(added);
							}
						}

						if (SelectedRepeatedDates[1] == true)
						{
							countingDate = 7;
							for (int i = 0; i < counts; i++)
							{
								var added = (MapEvent)temp.Clone();
								if (oldCopies.Count > 0)
								{
									added.Id = oldCopies.Pop();
								}
								else
								{
									added.Id = Guid.NewGuid().ToString();
								}
								added.Start = temp.Start.AddDays(countingDate + 1);
								added.End = temp.End.AddDays(countingDate + 1);

								countingDate += 7;

								repeatedEvents.Add(added);
							}
						}

						if (SelectedRepeatedDates[2] == true)
						{
							countingDate = 7;
							for (int i = 0; i < counts; i++)
							{
								var added = (MapEvent)temp.Clone();
								if (oldCopies.Count > 0)
								{
									added.Id = oldCopies.Pop();
								}
								else
								{
									added.Id = Guid.NewGuid().ToString();
								}
								added.Start = temp.Start.AddDays(countingDate + 2);
								added.End = temp.End.AddDays(countingDate + 2);

								countingDate += 7;

								repeatedEvents.Add(added);
							}
						}

						if (SelectedRepeatedDates[3] == true)
						{
							countingDate = 7;
							for (int i = 0; i < counts; i++)
							{
								var added = (MapEvent)temp.Clone();
								if (oldCopies.Count > 0)
								{
									added.Id = oldCopies.Pop();
								}
								else
								{
									added.Id = Guid.NewGuid().ToString();
								}
								added.Start = temp.Start.AddDays(countingDate + 3);
								added.End = temp.End.AddDays(countingDate + 3);

								countingDate += 7;

								repeatedEvents.Add(added);
							}
						}

						if (SelectedRepeatedDates[4] == true)
						{
							countingDate = 7;
							for (int i = 0; i < counts; i++)
							{
								var added = (MapEvent)temp.Clone();
								if (oldCopies.Count > 0)
								{
									added.Id = oldCopies.Pop();
								}
								else
								{
									added.Id = Guid.NewGuid().ToString();
								}
								added.Start = temp.Start.AddDays(countingDate + 4);
								added.End = temp.End.AddDays(countingDate + 4);

								countingDate += 7;

								repeatedEvents.Add(added);
							}
						}

						if (SelectedRepeatedDates[5] == true)
						{
							countingDate = 7;
							for (int i = 0; i < counts; i++)
							{
								var added = (MapEvent)temp.Clone();
								if (oldCopies.Count > 0)
								{
									added.Id = oldCopies.Pop();
								}
								else
								{
									added.Id = Guid.NewGuid().ToString();
								}
								added.Start = temp.Start.AddDays(countingDate + 5);
								added.End = temp.End.AddDays(countingDate + 5);

								countingDate += 7;

								repeatedEvents.Add(added);
							}
						}

						if (SelectedRepeatedDates[6] == true)
						{
							countingDate = 7;
							for (int i = 0; i < counts; i++)
							{
								var added = (MapEvent)temp.Clone();
								if (oldCopies.Count > 0)
								{
									added.Id = oldCopies.Pop();
								}
								else
								{
									added.Id = Guid.NewGuid().ToString();
								}
								added.Start = temp.Start.AddDays(countingDate + 6);
								added.End = temp.End.AddDays(countingDate + 6);

								countingDate += 7;

								repeatedEvents.Add(added);
							}
						}

						return repeatedEvents;

					default:
						return repeatedEvents;
				}
			}

			return repeatedEvents;
		}

		/// <summary>
		/// Checking the possibility of creating an event
		/// </summary>
		/// <returns>True if all requirement fulfilled</returns>
		private async Task<bool> CanCreateEventAsync()
		{
			if (string.IsNullOrEmpty(CurrentMapEvent.Name?.Trim()) || StartDate == null || (EndDate == null && IsNotAllDay))
			{
				await (new MessageDialog("Не заполнено одно из обязательных полей",
					(StartTabIndex == 0) ? "Ошибка создания нового события" : "Ошибка изменения события")).ShowAsync();

				return false;
			}

			if (CurrentMapEvent.Start > CurrentMapEvent.End)
			{
				await (new MessageDialog("Дата начала не может быть позже даты окончания события", "Ошибка создания нового события")).ShowAsync();

				return false;
			}

			if (IsEndingForDateSelected && !BeforeDateRecurrence.HasValue)
			{
				await new MessageDialog("Не указана дата окончания события", "Ошибка создания нового события").ShowAsync();

				return false;
			}

			if (IsEndingForRepeatSelected && BeforeNumberRecurrence < 1)
			{
				await new MessageDialog("Число повторений не может быть меньше 1", "Ошибка создания нового события").ShowAsync();

				return false;
			}

			return true;
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
		/// Controlling the input to the correctness of filling the numbers
		/// </summary>
		/// <param name="sender">TextBox sender</param>
		/// <param name="e">Sended parameter</param>
		public void ControlTheInputOfTheNumberOfRepetitions(object sender, TextChangedEventArgs e)
		{
			if (((TextBox)sender).Text.Length > 3)
			{
				((TextBox)sender).Text = ((TextBox)sender).Text.Substring(0, 3);
			}

			var isCorrect = int.TryParse(((TextBox)sender).Text, out int res);
			if (isCorrect && res >= 0)
			{
				BeforeNumberRecurrence = res;
			}
			else
			{
				try
				{
					((TextBox)sender).Text = ((TextBox)sender).Text.Remove(((TextBox)sender).Text.Length - 1);
				}
				catch (Exception)
				{
					((TextBox)sender).Text = string.Empty;
					BeforeNumberRecurrence = 0;
				}
			}

			((TextBox)sender).SelectionStart = ((TextBox)sender).Text.Length;
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