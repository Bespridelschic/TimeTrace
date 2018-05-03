﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage;
using TimeTrace.Model.Events;
using TimeTrace.Model.DBContext;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;

namespace TimeTrace.ViewModel.MainViewModel
{
	/// <summary>
	/// Schedule view model
	/// </summary>
	public class ScheduleViewModel : BaseViewModel
	{
		#region Properties

		/// <summary>
		/// Collection of viewed map events
		/// </summary>
		public ObservableCollection<MapEvent> MapEvents { get; set; }

		private ObservableCollection<DateTimeOffset> filterDates;
		/// <summary>
		/// Current filtered date
		/// </summary>
		public ObservableCollection<DateTimeOffset> FilterDates
		{
			get => filterDates;
			set
			{
				filterDates = value;
				OnPropertyChanged();
			}
		}

		private int? selectedMapEvent;
		/// <summary>
		/// Index of selected event
		/// </summary>
		public int? SelectedMapEvent
		{
			get => selectedMapEvent;
			set
			{
				selectedMapEvent = value;
				OnPropertyChanged();
			}
		}

		private bool isFilterExpanded;
		/// <summary>
		/// Is filter panel open
		/// </summary>
		public bool IsFilterExpanded
		{
			get => isFilterExpanded;
			set
			{
				isFilterExpanded = value;
				OnPropertyChanged();
			}
		}

		private string requiredFilter;
		/// <summary>
		/// Term for finding map events
		/// </summary>
		public string RequiredFilter
		{
			get { return requiredFilter; }
			set
			{
				requiredFilter = value;
				OnPropertyChanged();
			}
		}

		private ObservableCollection<string> mapEventsSuggestList;
		/// <summary>
		/// Filter tips
		/// </summary>
		public ObservableCollection<string> MapEventsSuggestList
		{
			get => mapEventsSuggestList;
			set
			{
				mapEventsSuggestList = value;
				OnPropertyChanged();
			}
		}

		#region Filters

		private bool isPublicMapEventsOnly;
		/// <summary>
		/// Show only public map events
		/// </summary>
		public bool IsPublicMapEventsOnly
		{
			get => isPublicMapEventsOnly;
			set
			{
				isPublicMapEventsOnly = value;
				ApplyFilter();
				OnPropertyChanged();
			}
		}

		private bool isTimeFiltered;
		/// <summary>
		/// Enable time span filter
		/// </summary>
		public bool IsTimeFiltered
		{
			get => isTimeFiltered;
			set
			{
				isTimeFiltered = value;
				if (!value)
				{
					FilterStartTime = TimeSpan.Parse("00:00");
					FilterEndTime = TimeSpan.Parse("23:59");
					ApplyFilter();
				}

				OnPropertyChanged();
			}
		}

		private TimeSpan filterStartTime;
		/// <summary>
		/// Filtering by start time
		/// </summary>
		public TimeSpan FilterStartTime
		{
			get => filterStartTime;
			set
			{
				if (filterStartTime != value)
				{
					if (FilterEndTime < value)
					{
						FilterEndTime = value;
					}

					filterStartTime = value;
					ApplyFilter();
					OnPropertyChanged();
				}
			}
		}

		private TimeSpan filterEndTime;
		/// <summary>
		/// Filtering by start time
		/// </summary>
		public TimeSpan FilterEndTime
		{
			get => filterEndTime;
			set
			{
				if (filterEndTime != value)
				{
					if (value < FilterStartTime)
					{
						filterEndTime = FilterStartTime;
					}
					else
					{
						filterEndTime = value;
					}
					ApplyFilter();
					OnPropertyChanged();
				}
			}
		}

		private ObservableCollection<DateTimeOffset> selectedFilteredDates;
		/// <summary>
		/// Selected dates for filtering
		/// </summary>
		public ObservableCollection<DateTimeOffset> SelectedFilteredDates
		{
			get => selectedFilteredDates;
			set
			{
				selectedFilteredDates = value;
				OnPropertyChanged();
			}
		}

		private ObservableCollection<string> mapEventsPlacesSuggestList;
		/// <summary>
		/// Places filter tips
		/// </summary>
		public ObservableCollection<string> MapEventsPlacesSuggestList
		{
			get => mapEventsPlacesSuggestList;
			set
			{
				mapEventsPlacesSuggestList = value;
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

		private string requiredMapEventsLocation;
		/// <summary>
		/// Term for finding map events with locations
		/// </summary>
		public string RequiredMapEventsLocation
		{
			get => requiredMapEventsLocation;
			set
			{
				requiredMapEventsLocation = value;
				OnPropertyChanged();
			}
		}

		private string requiredMapEventsPerson;
		/// <summary>
		/// Term for finding map events with persons
		/// </summary>
		public string RequiredMapEventsPerson
		{
			get => requiredMapEventsPerson;
			set
			{
				requiredMapEventsPerson = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Selected dates in View
		/// </summary>
		private CalendarView selectedDates;

		#endregion

		#endregion

		#region Constructors

		/// <summary>
		/// Initialize local collections and properties for constructors
		/// </summary>
		private void InitializationData()
		{
			MapEventsSuggestList = new ObservableCollection<string>();
			SelectedFilteredDates = new ObservableCollection<DateTimeOffset>();

			MapEventsPlacesSuggestList = new ObservableCollection<string>();
			mapEventsPersonsSuggestList = new ObservableCollection<string>();
		}

		/// <summary>
		/// Standart constructor
		/// </summary>
		public ScheduleViewModel()
		{
			InitializationData();

			using (MainDatabaseContext db = new MainDatabaseContext())
			{
				ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

				MapEvents = new ObservableCollection<MapEvent>(db.MapEvents
						.Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete)
						.ToList());
			}
		}

		/// <summary>
		/// Constructor with sent project
		/// </summary>
		/// <param name="project">Current selected project</param>
		public ScheduleViewModel(Project project)
		{
			InitializationData();

			using (MainDatabaseContext db = new MainDatabaseContext())
			{
				ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

				if (!string.IsNullOrEmpty(project.Id))
				{
					MapEvents = new ObservableCollection<MapEvent>(db.MapEvents
						.Where(i => i.ProjectId == project.Id && i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete)
						.ToList());
				}
			}
		}

		/// <summary>
		/// Constructor with sent map events
		/// </summary>
		/// <param name="requestedMapEvents">List of requested events</param>
		public ScheduleViewModel(string requestedMapEvents)
		{
			InitializationData();

			using (MainDatabaseContext db = new MainDatabaseContext())
			{
				ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

				if (requestedMapEvents != null)
				{
					RequiredFilter = requestedMapEvents;

					MapEvents = new ObservableCollection<MapEvent>(db.MapEvents
						.Where(i => i.Name.ToLowerInvariant().Contains(requestedMapEvents) && i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete)
						.ToList());
				}
			}
		}

		#endregion

		/// <summary>
		/// Selecting item of list with right click
		/// </summary>
		/// <param name="sender">Sender list</param>
		/// <param name="e">Parameter</param>
		public void SelectItemOnRightClick(object sender, RightTappedRoutedEventArgs e)
		{
			ListView listView = (ListView)sender;
			if (((FrameworkElement)e.OriginalSource).DataContext is MapEvent selectedItem)
			{
				SelectedMapEvent = MapEvents.IndexOf(selectedItem);
			}
		}

		/// <summary>
		/// Remove of selected map event
		/// </summary>
		public async void MapEventRemove()
		{
			if (SelectedMapEvent.HasValue)
			{
				if (DateTime.UtcNow > MapEvents[SelectedMapEvent.Value].End)
				{
					await (new MessageDialog("Прошедшее событие не может быть удалено", "Ошибка")).ShowAsync();
					return;
				}

				ContentDialog contentDialog = new ContentDialog()
				{
					Title = "Подтверждение действия",
					Content = $"Вы уверены что хотите удалить событие \"{MapEvents[SelectedMapEvent.Value].Name}\"?",
					PrimaryButtonText = "Удалить",
					CloseButtonText = "Отмена",
					DefaultButton = ContentDialogButton.Close
				};

				var result = await contentDialog.ShowAsync();

				if (result == ContentDialogResult.Primary)
				{
					using (MainDatabaseContext db = new MainDatabaseContext())
					{
						db.MapEvents.FirstOrDefault(i => i.Id == MapEvents[SelectedMapEvent.Value].Id).IsDelete = true;
						MapEvents.RemoveAt(SelectedMapEvent.Value);

						db.SaveChanges();
					}

					await (new MessageDialog("Событие успешно удалёно", "Успех")).ShowAsync();
				}
			}
		}

		/// <summary>
		/// Show info about map event
		/// </summary>
		public async void MoreAboutMapEvent()
		{
			if (SelectedMapEvent.HasValue)
			{
				MapEvent tempEvent = MapEvents[SelectedMapEvent.Value];

				var person = string.IsNullOrEmpty(tempEvent.UserBind) ? "Отсутствует" : tempEvent.UserBind;
				var place = string.IsNullOrEmpty(tempEvent.Location) ? "Не задано" : tempEvent.Location;
				var isPublicMapEvent = tempEvent.IsPublic ? "Публичное событие" : "Персональное событие";

				TextBlock contentText = new TextBlock()
				{
					Text = $"Имя события: {tempEvent.Name}\n" +
							$"Описание: {tempEvent.Description ?? "Отсутствует"}\n" +
							$"Время начала: {tempEvent.Start.ToShortDateString()} {tempEvent.Start.ToLocalTime().ToShortTimeString()}\n" +
							$"Продолжительность: {(int)tempEvent.End.Subtract(tempEvent.Start).TotalHours} ч.\n" +
							$"Персона, связанная с событием: {person}\n" +
							$"Место события: {place}\n\n" +
							$"{isPublicMapEvent}",
				};

				ContentDialog contentDialog = new ContentDialog()
				{
					Title = "Подробности",
					Content = contentText,
					CloseButtonText = "Закрыть",
					DefaultButton = ContentDialogButton.Close
				};

				var result = await contentDialog.ShowAsync();
			}
		}

		/// <summary>
		/// Edit selected map event
		/// </summary>
		public async void MapEventEditAsync()
		{
			await new MessageDialog("Здесь должен быть переход на страницу редактирования. Но его нет... Пока что...").ShowAsync();
		}

		#region Filtering

		/// <summary>
		/// Selection date trigger
		/// </summary>
		/// <param name="sender">Calendar dates</param>
		/// <param name="args">Args</param>
		public void DateSelection(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
		{
			if (selectedDates == null)
			{
				selectedDates = sender;
			}

			SelectedFilteredDates.Clear();

			foreach (var item in sender.SelectedDates)
			{
				SelectedFilteredDates.Add(item);
			}

			ApplyFilter();
		}

		/// <summary>
		/// Reset selected dates
		/// </summary>
		public void ResetSelectedDates()
		{
			if (SelectedFilteredDates.Count <= 0) return;

			SelectedFilteredDates.Clear();
			selectedDates?.SelectedDates?.Clear();

			ApplyFilter();
		}

		/// <summary>
		/// Expand or close filter panel
		/// </summary>
		public void ExpandFilterPage()
		{
			IsFilterExpanded = !IsFilterExpanded;
		}

		/// <summary>
		/// Filtration of input places
		/// </summary>
		/// <param name="sender">Input filter</param>
		/// <param name="args">Event args</param>
		public void MapEventsPlacesFilter(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (args.CheckCurrent())
			{
				MapEventsPlacesSuggestList.Clear();
			}

			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

			if (!string.IsNullOrEmpty(sender.Text))
			{
				MapEventsPlacesSuggestList.Clear();

				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					foreach (var i in db.MapEvents
						.Where(i => (i.Location.ToLowerInvariant().Contains(sender.Text.ToLowerInvariant())) &&
									i.EmailOfOwner == (string)localSettings.Values["email"] &&
									!i.IsDelete)
						.Select(i => i))
					{
						if (!MapEventsPlacesSuggestList.Contains(i.Location))
						{
							MapEventsPlacesSuggestList.Add(i.Location);
						}
					}
				}
			}

			ApplyFilter();
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

			ApplyFilter();
		}

		/// <summary>
		/// Apply filter for selecting
		/// </summary>
		private void ApplyFilter()
		{
			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

			using (MainDatabaseContext db = new MainDatabaseContext())
			{
				MapEvents.Clear();

				foreach (var item in db.MapEvents
					.Join(
							SelectedFilteredDates,
							i => i.Start.Date,
							w => w.Date,
							(i, w) => i
						)
					.Where(i => !i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"])
					.ToList())
				{
					MapEvents.Add(item);
				}

				// Select all map events from database, if dates not selected
				if (SelectedFilteredDates.Count <= 0)
				{
					foreach (var item in db.MapEvents
						.Where(i => !i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"])
						.ToList())
					{
						MapEvents.Add(item);
					}
				}

				if (IsPublicMapEventsOnly)
				{
					foreach (var item in MapEvents.Where(i => !i.IsPublic).ToList())
					{
						MapEvents.Remove(item);
					}
				}

				if (IsTimeFiltered)
				{
					foreach (var item in MapEvents
						.Except(MapEvents
						.Where(i => FilterStartTime < TimeSpan.Parse(i.Start.ToString("HH:mm:ss")) && FilterEndTime > TimeSpan.Parse(i.End.ToString("HH:mm:ss"))))
						.ToList())
					{
						MapEvents.Remove(item);
					}
				}

				if (!string.IsNullOrEmpty(RequiredMapEventsLocation))
				{
					foreach (var item in MapEvents
						.Where(i => !i.Location.Contains(RequiredMapEventsLocation))
						.ToList())
					{
						MapEvents.Remove(item);
					}
				}

				if (!string.IsNullOrEmpty(RequiredMapEventsPerson))
				{
					foreach (var item in MapEvents
						.Where(i => !i.UserBind.Contains(RequiredMapEventsPerson))
						.ToList())
					{
						MapEvents.Remove(item);
					}
				}
			}
		}

		/// <summary>
		/// Reset all selected filters
		/// </summary>
		public void ResetAllFilters()
		{
			ResetSelectedDates();
			IsPublicMapEventsOnly = false;
			IsTimeFiltered = false;

			RequiredMapEventsLocation = string.Empty;
			RequiredMapEventsPerson = string.Empty;
		}

		#endregion

		#region Searching map events

		/// <summary>
		/// Filtration of input filters
		/// </summary>
		/// <param name="sender">Input filter</param>
		/// <param name="args">Event args</param>
		public void MapEventsFilter(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (args.CheckCurrent())
			{
				MapEventsSuggestList.Clear();
			}

			// Remove all map events for adding relevant filter
			MapEvents.Clear();

			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

			// Select all map events
			if (string.IsNullOrEmpty(sender.Text))
			{
				MapEventsSuggestList.Clear();

				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					foreach (var i in db.MapEvents.Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete).Select(i => i))
					{
						MapEvents.Add(i);
					}
				}
			}

			else
			{
				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					foreach (var i in db.MapEvents
						.Where(i => (i.Name.ToLowerInvariant().Contains(sender.Text.ToLowerInvariant())) &&
									i.EmailOfOwner == (string)localSettings.Values["email"] &&
									!i.IsDelete)
						.Select(i => i))
					{
						if (!MapEventsSuggestList.Contains(i.Name))
						{
							MapEventsSuggestList.Add(i.Name);
						}

						MapEvents.Add(i);
					}
				}
			}
		}

		/// <summary>
		/// Click on Find map events
		/// </summary>
		/// <param name="sender">Object</param>
		/// <param name="args">Args</param>
		public void MapEventsFilterQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			MapEventsSuggestList.Clear();

			if (string.IsNullOrEmpty(args.QueryText))
			{
				return;
			}

			if (MapEvents != null)
			{
				MapEvents.Clear();

				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
					var term = args.QueryText.ToLower();
					foreach (var i in db.MapEvents
						.Where(i => i.Name.ToLowerInvariant().Contains(term) && !i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"])
						.Select(i => i))
					{
						MapEvents.Add(i);
					}
				}
			}
		}

		#endregion
	}
}