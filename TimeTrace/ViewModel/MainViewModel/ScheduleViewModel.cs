using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			get { return selectedMapEvent; }
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
		public ScheduleViewModel()
		{
			using (MapEventContext db = new MapEventContext())
			{
				//MapEvents = new ObservableCollection<MapEvent>(db.MapEvents.Where(i => i.Start.Date == DateTime.Today)) ??
				var list = new ObservableCollection<MapEvent>(db.MapEvents.ToList());
							
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
					db.MapEvents.Remove(MapEvents[SelectedMapEvent]);
					MapEvents[SelectedMapEvent].IsDelete = true;
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
						$"Время начала: {tempEvent.Start.ToShortDateString()} {tempEvent.Start.ToShortTimeString()}\n" +
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
