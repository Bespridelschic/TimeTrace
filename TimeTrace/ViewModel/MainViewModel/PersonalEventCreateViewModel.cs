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

namespace TimeTrace.ViewModel.MainViewModel
{
	public class PersonalEventCreateViewModel : BaseViewModel
	{
		#region Свойства

		private MapEvent currentMapEvent;
		/// <summary>
		/// Модель текущего устанавливаемого события
		/// </summary>
		public MapEvent CurrentMapEvent
		{
			get { return currentMapEvent; }
			set
			{
				currentMapEvent = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Минимальная дата - текущий день
		/// </summary>
		public DateTime MinDate { get; set; }

		private bool isSelectPlace;
		/// <summary>
		/// При привязке события к месту
		/// </summary>
		public bool IsSelectPlace
		{
			get { return isSelectPlace; }
			set
			{
				isSelectPlace = value;
				OnPropertyChanged();
			}
		}

		private bool isSelectMan;
		/// <summary>
		/// При привязке события к человеку
		/// </summary>
		public bool IsSelectMan
		{
			get { return isSelectMan; }
			set
			{
				isSelectMan = value;
				OnPropertyChanged();
			}
		}

		#endregion

		/// <summary>
		/// Стандартный конструктор
		/// </summary>
		public PersonalEventCreateViewModel()
		{
			CurrentMapEvent = new MapEvent();
			MinDate = DateTime.Today;

			IsSelectMan = false;
			IsSelectPlace = true;
		}

		public async Task EventCreate()
		{
			await (new MessageDialog($"{CurrentMapEvent.Name}", "Сработало")).ShowAsync();
		}

		/// <summary>
		/// Выбор категории события
		/// </summary>
		public void CategorySelect()
		{
			if (Window.Current.Content is Frame frame)
			{
				frame.Navigate(typeof(NewEventCreatePage));
			}
		}
	}
}