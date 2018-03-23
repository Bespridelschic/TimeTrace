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
using TimeTrace.Model.Events.DBContext;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace TimeTrace.ViewModel.MainViewModel
{
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

		private int selectedMapEvent;
		/// <summary>
		/// Index of selected event
		/// </summary>
		public int SelectedMapEvent
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
			/*FilterDates = sender.SelectedDates.ToList();
			if (MapEvents == null)
			{
				MapEvents = new ObservableCollection<MapEvent>();
			}*/

			/*using (MapEventContext db = new MapEventContext())
			{
				var tmp = db.MapEvents.Where(i => i.Start.Date == args.AddedDates.Last().Date).Last();
				if (tmp != null) MapEvents.Add(tmp);
			}*/
		}

		/// <summary>
		/// Standart constructor
		/// </summary>
		public ScheduleViewModel(string projectId = null)
		{
			using (MapEventContext db = new MapEventContext())
			{
				ObservableCollection<MapEvent> list;
				ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

				if (projectId == null)
				{
					list = new ObservableCollection<MapEvent>(db.MapEvents.
						Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete).
						ToList());
				}
				else
				{
					list = new ObservableCollection<MapEvent>(db.MapEvents.
						Where(i => i.ProjectId == projectId && i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete).
						ToList());
				}

				if (list.Count == 0)
				{
					list = new ObservableCollection<MapEvent>(
						new List<MapEvent> { new MapEvent("Пример события", "Пример события", DateTime.Today, DateTime.Today, "Дом", "Я", "str", "123415") });
				}

				MapEvents = list;
			}
		}

		/// <summary>
		/// Remove of selected map event
		/// </summary>
		public async void MapEventRemove()
		{
			if (DateTime.Now > MapEvents[SelectedMapEvent].End)
			{
				await (new MessageDialog("Прошедшее событие не может быть удалено", "Ошибка")).ShowAsync();
				return;
			}

			ContentDialog contentDialog = new ContentDialog()
			{
				Title = "Подтверждение действия",
				Content = $"Вы уверены что хотите удалить событие \"{MapEvents[SelectedMapEvent].Name}\"?",
				PrimaryButtonText = "Удалить",
				CloseButtonText = "Отмена",
				DefaultButton = ContentDialogButton.Close
			};

			var result = await contentDialog.ShowAsync();

			if (result == ContentDialogResult.Primary)
			{
				using (MapEventContext db = new MapEventContext())
				{
					db.MapEvents.FirstOrDefault(i => i.Id == MapEvents[SelectedMapEvent].Id).IsDelete = true;
					MapEvents.RemoveAt(SelectedMapEvent);

					db.SaveChanges();
				}

				await (new MessageDialog("Событие успешно удалёно", "Успех")).ShowAsync();
			}
		}

		public async void MoreAboutMapEvent()
		{
			MapEvent tempEvent = MapEvents[SelectedMapEvent];

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
