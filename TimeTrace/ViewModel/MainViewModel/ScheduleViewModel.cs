using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTrace.Model;
using TimeTrace.Model.DBContext;
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

		private List<DateTimeOffset> filterDates;
		/// <summary>
		/// Current filtered date
		/// </summary>
		public List<DateTimeOffset> FilterDates
		{
			get => filterDates;
			set
			{
				filterDates = value;
				OnPropertyChanged();
			}
		}

		private int selectedMapEvent;

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
		public async void DateSelection(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
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
				//MapEvents = new ObservableCollection<MapEvent>(db.MapEvents.Where(i => i.Start.Date == DateTime.Today));
				MapEvents = new ObservableCollection<MapEvent>(db.MapEvents.ToList());
			}
		}

		/// <summary>
		/// Remove of selected map event
		/// </summary>
		public async void MapEventRemove()
		{
			ContentDialog contentDialog = new ContentDialog()
			{
				Title = "Подтверждение действия",
				Content = $"Вы уверены что хотите удалить событие?",
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
					MapEvents.RemoveAt(SelectedMapEvent);

					db.SaveChanges();
				}

				await (new MessageDialog("Элемент успешно удалён", "Успех")).ShowAsync();
			}
		}
	}
}
