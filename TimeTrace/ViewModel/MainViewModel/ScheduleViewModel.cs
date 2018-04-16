using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using TimeTrace.Model;
using TimeTrace.Model.Events;
using TimeTrace.Model.DBContext;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace TimeTrace.ViewModel.MainViewModel
{
	/// <summary>
	/// Schedule view model
	/// </summary>
	public class ScheduleViewModel : BaseViewModel
	{
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

		/// <summary>
		/// Selection date trigger
		/// </summary>
		/// <param name="sender">Calendar dates</param>
		/// <param name="args">Args</param>
		public void DateSelection(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
		{
			
		}

		/// <summary>
		/// Standart constructor
		/// </summary>
		public ScheduleViewModel(string projectId = null)
		{
			using (MainDatabaseContext db = new MainDatabaseContext())
			{
				ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

				if (projectId == null)
				{
					MapEvents = new ObservableCollection<MapEvent>(db.MapEvents.
						Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete).
						ToList());
				}
				else
				{
					MapEvents = new ObservableCollection<MapEvent>(db.MapEvents.
						Where(i => i.ProjectId == projectId && i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete).
						ToList());
				}
			}
		}

		/// <summary>
		/// Remove of selected map event
		/// </summary>
		public async void MapEventRemove()
		{
			if (SelectedMapEvent.HasValue)
			{
				if (DateTime.Now > MapEvents[SelectedMapEvent.Value].End)
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

				TextBlock contentText = new TextBlock()
				{
					Text = $"Имя события: {tempEvent.Name}\n" +
							$"Описание: {tempEvent.Description ?? "Отсутствует"}\n" +
							$"Время начала: {tempEvent.Start.ToShortDateString()} {tempEvent.Start.ToLocalTime().ToShortTimeString()}\n" +
							$"Продолжительность: {(int)tempEvent.End.Subtract(tempEvent.Start).TotalHours} ч.\n" +
							$"Персона, связанная с событием: {tempEvent.UserBind ?? "Отсутствует"}\n" +
							$"Место, связанное с событием: {tempEvent.Location ?? "Не задано"}\n",
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
	}
}
