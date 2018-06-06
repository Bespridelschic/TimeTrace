using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage;
using TimeTrace.Model.Events;
using TimeTrace.Model.DBContext;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;
using TimeTrace.View.MainView.PersonalMapsCreatePages;
using TimeTrace.Model.Requests;
using System.Diagnostics;
using Windows.ApplicationModel.Resources;
using System.Collections.Generic;

namespace TimeTrace.ViewModel.MainViewModel
{
	/// <summary>
	/// Schedule view model
	/// </summary>
	public class ScheduleViewModel : BaseViewModel, ISearchable<string>
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

		/// <summary>
		/// If offine sign in, block internet features
		/// </summary>
		public bool InternetFeaturesEnable { get; set; }

		private ListViewSelectionMode multipleSelection;
		/// <summary>
		/// Is multiple selection enable
		/// </summary>
		public ListViewSelectionMode MultipleSelection
		{
			get => multipleSelection;
			set
			{
				multipleSelection = value;
				OnPropertyChanged();
			}
		}

		private List<MapEvent> selectedEvents;
		/// <summary>
		/// Collection of multiple contacts selection
		/// </summary>
		public List<MapEvent> SelectedEvents
		{
			get => selectedEvents;
			set
			{
				selectedEvents = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Localization resource loader
		/// </summary>
		public ResourceLoader ResourceLoader { get; set; }

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

		private bool isHistoryEventsViewed;
		/// <summary>
		/// Is enabled history of events
		/// </summary>
		public bool IsHistoryEventsViewed
		{
			get => isHistoryEventsViewed;
			set
			{
				isHistoryEventsViewed = value;
				OnPropertyChanged();
				ApplyFilter();
			}
		}

		#endregion

		#endregion

		#region Constructors

		/// <summary>
		/// Initialize local collections and properties for constructors
		/// </summary>
		private void InitializationData()
		{
			StartPageViewModel.Instance.SetHeader(Headers.Shedule);
			ResourceLoader = ResourceLoader.GetForCurrentView("ScheduleVM");
			InternetFeaturesEnable = StartPageViewModel.Instance.InternetFeaturesEnable;

			SearchSuggestions = new ObservableCollection<string>();
			SelectedFilteredDates = new ObservableCollection<DateTimeOffset>();

			MapEventsPlacesSuggestList = new ObservableCollection<string>();
			mapEventsPersonsSuggestList = new ObservableCollection<string>();

			MultipleSelection = ListViewSelectionMode.Single;
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
						.Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete && i.End >= DateTime.UtcNow)
						.OrderBy(i => i.Start)
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
						.Where(i => i.ProjectId == project.Id && i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete && i.End >= DateTime.UtcNow)
						.OrderBy(i => i.Start)
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
					SearchTerm = requestedMapEvents;

					MapEvents = new ObservableCollection<MapEvent>(db.MapEvents
						.Where(i => i.Name.ToLowerInvariant().Contains(requestedMapEvents) &&
									i.EmailOfOwner == (string)localSettings.Values["email"] &&
									!i.IsDelete &&
									i.End >= DateTime.UtcNow)
						.OrderBy(i => i.Start)
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
					await (new MessageDialog(ResourceLoader.GetString("/ScheduleVM/PastEventCantBeRemoved"), ResourceLoader.GetString("/ScheduleVM/Error"))).ShowAsync();
					return;
				}

				ContentDialog contentDialog = new ContentDialog()
				{
					Title = ResourceLoader.GetString("/ScheduleVM/ConfirmAction"),
					Content = $"{ResourceLoader.GetString("/ScheduleVM/ConfirmationRemoving")} \"{MapEvents[SelectedMapEvent.Value].Name}\"?",
					PrimaryButtonText = ResourceLoader.GetString("/ScheduleVM/Remove"),
					CloseButtonText = ResourceLoader.GetString("/ScheduleVM/Cancel"),
					DefaultButton = ContentDialogButton.Close
				};

				var result = await contentDialog.ShowAsync();

				if (result == ContentDialogResult.Primary)
				{
					using (MainDatabaseContext db = new MainDatabaseContext())
					{
						// Remove copies
						if (string.IsNullOrEmpty(MapEvents[SelectedMapEvent.Value].EventInterval))
						{
							if (db.MapEvents.Count(i => i.EventInterval == MapEvents[SelectedMapEvent.Value].Id) > 0)
							{
								// Remove all repeated copies
								foreach (var item in db.MapEvents.Where(i => i.EventInterval == MapEvents[SelectedMapEvent.Value].Id && i.End > DateTime.UtcNow))
								{
									db.MapEvents.FirstOrDefault(i => i.Id == item.Id).IsDelete = true;
								}
							}

							// Remove original event
							if (MapEvents[SelectedMapEvent.Value].End >= DateTime.UtcNow)
							{
								db.MapEvents.FirstOrDefault(i => i.Id == MapEvents[SelectedMapEvent.Value].Id).IsDelete = true;
							}
						}
						// Remove original and copies
						else
						{
							var originalEvent = db.MapEvents.First(i => i.Id == MapEvents[SelectedMapEvent.Value].EventInterval);

							foreach (var item in db.MapEvents.Where(i => i.EventInterval == originalEvent.Id && i.End > DateTime.UtcNow))
							{
								db.MapEvents.FirstOrDefault(i => i.Id == item.Id).IsDelete = true;
							}

							if (originalEvent.End >= DateTime.UtcNow)
							{
								db.MapEvents.FirstOrDefault(i => i.Id == originalEvent.Id).IsDelete = true;
							}
						}

						db.SaveChanges();

						ResetAllFilters();
					}

					await (new MessageDialog(ResourceLoader.GetString("/ScheduleVM/EventSuccessfullyDeleted"), ResourceLoader.GetString("/ScheduleVM/Success"))).ShowAsync();

					// Synchronization of changes with server
					await StartPageViewModel.Instance.CategoriesSynchronization();
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

				var person = string.IsNullOrEmpty(tempEvent.UserBind) ? ResourceLoader.GetString("/ScheduleVM/Absent") : tempEvent.UserBind;
				var place = string.IsNullOrEmpty(tempEvent.Location) ? ResourceLoader.GetString("/ScheduleVM/NotSet") : tempEvent.Location;
				var description = string.IsNullOrEmpty(tempEvent.Description) ? ResourceLoader.GetString("/ScheduleVM/Absent") : tempEvent.Description;
				var isPublicMapEvent = tempEvent.IsPublic ? ResourceLoader.GetString("/ScheduleVM/PublicEvent") : ResourceLoader.GetString("/ScheduleVM/PrivateEvent");
				var duration = new TimeSpan(tempEvent.End.Subtract(tempEvent.Start).Ticks)
								.ToString(@"d\.hh\:mm")
								.Replace(".", $" {ResourceLoader.GetString("/ScheduleVM/Days")} ") + " " + ResourceLoader.GetString("/ScheduleVM/Hours");
				var willComeThrough = tempEvent.Start.Subtract(DateTime.UtcNow)
								.ToString(@"d\.hh\:mm")
								.Replace(".", $" {ResourceLoader.GetString("/ScheduleVM/Days")} ") + " " + ResourceLoader.GetString("/ScheduleVM/Hours");

				TextBlock contentText = new TextBlock()
				{
					Text = $"{ResourceLoader.GetString("/ScheduleVM/Name")}: {tempEvent.Name}\n" +
							$"{ResourceLoader.GetString("/ScheduleVM/Start")}: {tempEvent.Start.ToLocalTime().ToShortDateString()} {tempEvent.Start.ToLocalTime().ToShortTimeString()}\n" +
							$"{ResourceLoader.GetString("/ScheduleVM/Duration")}: {duration}.\n" +
							$"{ResourceLoader.GetString("/ScheduleVM/WillComeThrough")}: {willComeThrough}.\n" +
							$"{ResourceLoader.GetString("/ScheduleVM/PersonAssociatedWithEvent")}: {person}\n" +
							$"{ResourceLoader.GetString("/ScheduleVM/Place")}: {place}\n" +
							$"{isPublicMapEvent}\n\n" +
							$"{ResourceLoader.GetString("/ScheduleVM/Description")}: {description}",
					TextWrapping = TextWrapping.WrapWholeWords,
					TextTrimming = TextTrimming.WordEllipsis,
				};

				ContentDialog contentDialog = new ContentDialog()
				{
					Title = ResourceLoader.GetString("/ScheduleVM/Details"),
					Content = contentText,
					CloseButtonText = ResourceLoader.GetString("/ScheduleVM/Close"),
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
			if (SelectedMapEvent.HasValue && !IsHistoryEventsViewed)
			{
				(Application.Current as App)?.AppFrame.Navigate(typeof(PersonalEventCreatePage), MapEvents[selectedMapEvent.Value]);
			}

			if (IsHistoryEventsViewed)
			{
				await new MessageDialog(ResourceLoader.GetString("/ScheduleVM/EventChangingErrorMessage"), ResourceLoader.GetString("/ScheduleVM/EventChangingError")).ShowAsync();
			}
		}

		/// <summary>
		/// Select several events
		/// </summary>
		/// <param name="sender">ListView</param>
		/// <param name="e">Parameters</param>
		public void MultipleEventsSelection(object sender, SelectionChangedEventArgs e)
		{
			ListView listView = sender as ListView;
			SelectedEvents = new List<MapEvent>();

			foreach (MapEvent item in listView.SelectedItems)
			{
				SelectedEvents.Add(item);
			};
		}

		/// <summary>
		/// Remove of several events
		/// </summary>
		public async void EventsRemoveAsync()
		{
			if (MultipleSelection == ListViewSelectionMode.Single)
			{
				MultipleSelection = ListViewSelectionMode.Multiple;
			}
			else
			{
				if (SelectedEvents == null || SelectedEvents.Count <= 0)
				{
					MultipleSelection = ListViewSelectionMode.Single;
					return;
				}

				var originalList = SelectedEvents.Where(i => string.IsNullOrEmpty(i.EventInterval)).ToList();

				using (var db = new MainDatabaseContext())
				{
					var copies = db.MapEvents
						.Join(
							SelectedEvents.Where(i => !string.IsNullOrEmpty(i.EventInterval)).ToList(),
							i => i.EventInterval,
							w => w.EventInterval,
							(i, w) => i)
						.Distinct().ToList();

					originalList
						.AddRange(db.MapEvents
							.Join(
								copies,
								i => i.Id,
								w => w.EventInterval,
								(i, w) => i)
							.Distinct()
						);
				}

				string removedEvents = string.Empty;

				foreach (var localEvent in originalList)
				{
					if (!removedEvents.Contains(localEvent.Name))
					{
						removedEvents += $"{localEvent.Name}\n";
					}
				}

				ScrollViewer scrollViewer = new ScrollViewer()
				{
					VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
					Content = new TextBlock() { Text = removedEvents },
				};

				StackPanel mainPanel = new StackPanel()
				{
					Margin = new Thickness(0, 0, 0, 10),
				};
				mainPanel.Children.Add(new TextBlock() { Text = ResourceLoader.GetString("/ScheduleVM/ConfirmBulkRemoving") });
				mainPanel.Children.Add(scrollViewer);

				ContentDialog contentDialog = new ContentDialog()
				{
					Title = ResourceLoader.GetString("/ScheduleVM/ConfirmAction"),
					Content = mainPanel,
					PrimaryButtonText = ResourceLoader.GetString("/ScheduleVM/Remove"),
					CloseButtonText = ResourceLoader.GetString("/ScheduleVM/Cancel"),
					DefaultButton = ContentDialogButton.Close
				};

				var result = await contentDialog.ShowAsync();

				if (result == ContentDialogResult.Primary)
				{
					using (MainDatabaseContext db = new MainDatabaseContext())
					{
						foreach (var item in originalList)
						{
							if (item.End > DateTime.UtcNow)
							{
								db.MapEvents.Where(i => i.Id == item.Id).FirstOrDefault().IsDelete = true;
							}

							foreach (var copy in db.MapEvents.Where(i => i.EventInterval == item.Id && i.End > DateTime.UtcNow))
							{
								db.MapEvents.Where(i => i.Id == copy.Id).FirstOrDefault().IsDelete = true;
							}
						}

						db.SaveChanges();

						ResetAllFilters();
					}

					await StartPageViewModel.Instance.CategoriesSynchronization();
				}

				MultipleSelection = ListViewSelectionMode.Single;
			}
		}

		/// <summary>
		/// Getting public map events from contacts
		/// </summary>
		public async void GetPublicEventsAsync()
		{
			var result = await InternetRequests.GetPublicMapEventsAsync();
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

				if (!IsHistoryEventsViewed)
				{
					foreach (var item in db.MapEvents
						.Join(
								SelectedFilteredDates,
								i => i.Start.ToLocalTime().Date,
								w => w.Date,
								(i, w) => i
							)
						.Where(i => !i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"] && i.End >= DateTime.UtcNow)
						.OrderBy(i => i.Start)
						.ToList())
					{
						MapEvents.Add(item);
					}
				}
				else
				{
					foreach (var item in db.MapEvents
						.Join(
								SelectedFilteredDates,
								i => i.Start.ToLocalTime().Date,
								w => w.Date,
								(i, w) => i
							)
						.Where(i => !i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"] && i.End < DateTime.UtcNow)
						.OrderBy(i => i.Start)
						.ToList())
					{
						MapEvents.Add(item);
					}
				}

				// Select all map events from database, if dates not selected
				if (SelectedFilteredDates.Count <= 0)
				{
					if (!IsHistoryEventsViewed)
					{
						foreach (var item in db.MapEvents
							.Where(i => !i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"] && i.End >= DateTime.UtcNow)
							.OrderBy(i => i.Start)
							.ToList())
						{
							MapEvents.Add(item);
						}
					}
					else
					{
						foreach (var item in db.MapEvents
							.Where(i => !i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"] && i.End < DateTime.UtcNow)
							.OrderBy(i => i.Start)
							.ToList())
						{
							MapEvents.Add(item);
						}
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
						.Where(i => !(TimeSpan.Parse(i.Start.ToLocalTime().ToString("HH:mm:ss")) <= FilterStartTime
									&& TimeSpan.Parse(i.End.ToLocalTime().ToString("HH:mm:ss")) >= FilterEndTime))
						.OrderBy(i => i.Start)
						.ToList())
					{
						MapEvents.Remove(item);
					}
				}

				if (!string.IsNullOrEmpty(RequiredMapEventsLocation))
				{
					foreach (var item in MapEvents
						.Where(i => !i.Location.Contains(RequiredMapEventsLocation))
						.OrderBy(i => i.Start)
						.ToList())
					{
						MapEvents.Remove(item);
					}
				}

				if (!string.IsNullOrEmpty(RequiredMapEventsPerson))
				{
					foreach (var item in MapEvents
						.Where(i => !i.UserBind.Contains(RequiredMapEventsPerson))
						.OrderBy(i => i.Start)
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
			IsHistoryEventsViewed = false;

			RequiredMapEventsLocation = string.Empty;
			RequiredMapEventsPerson = string.Empty;
		}

		#endregion

		#region Searching map events

		private string searchTerm;
		/// <summary>
		/// Term for finding map events
		/// </summary>
		public string SearchTerm
		{
			get => searchTerm;
			set
			{
				searchTerm = value;
				OnPropertyChanged();
			}
		}

		private ObservableCollection<string> searchSuggestions;
		/// <summary>
		/// Suggestions for searching
		/// </summary>
		public ObservableCollection<string> SearchSuggestions
		{
			get => searchSuggestions;
			set
			{
				searchSuggestions = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Filtration of input terms
		/// </summary>
		public void DynamicSearch()
		{
			SearchSuggestions.Clear();

			// Remove all map events for adding relevant filter
			MapEvents.Clear();

			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

			// Select all map events
			if (string.IsNullOrEmpty(SearchTerm))
			{
				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					foreach (var i in db.MapEvents
						.Where(i => i.EmailOfOwner == (string)localSettings.Values["email"]
							&& !i.IsDelete
							&& i.End >= DateTime.UtcNow)
						.Select(i => i))
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
						.Where(i => i.Name.ToLowerInvariant().Contains(SearchTerm.ToLowerInvariant())
							&& i.EmailOfOwner == (string)localSettings.Values["email"]
							&& !i.IsDelete
							&& i.End >= DateTime.UtcNow)
						.Select(i => i))
					{
						if (!SearchSuggestions.Contains(i.Name))
						{
							SearchSuggestions.Add(i.Name);
						}

						MapEvents.Add(i);
					}
				}
			}
		}

		/// <summary>
		/// User request for searching
		/// </summary>
		public void SearchRequest()
		{
			SearchSuggestions.Clear();

			if (string.IsNullOrEmpty(SearchTerm))
			{
				return;
			}

			if (MapEvents != null)
			{
				MapEvents.Clear();

				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

					foreach (var i in db.MapEvents
						.Where(i => i.Name.ToLowerInvariant().Contains(SearchTerm.ToLowerInvariant())
							&& !i.IsDelete
							&& i.EmailOfOwner == (string)localSettings.Values["email"]
							&& i.End >= DateTime.UtcNow)
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