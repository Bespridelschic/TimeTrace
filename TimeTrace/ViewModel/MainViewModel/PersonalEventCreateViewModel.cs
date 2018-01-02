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

		private bool isSelectManAndPlace;
		/// <summary>
		/// При привязке события к человеку и месту
		/// </summary>
		public bool IsSelectManAndPlace
		{
			get { return isSelectManAndPlace; }
			set
			{
				isSelectManAndPlace = value;
				isSelectMan = true;
				IsSelectPlace = true;
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

			IsSelectPlace = true;
			IsSelectMan = false;
			IsSelectManAndPlace = false;
		}

		/// <summary>
		/// Создание нового события
		/// </summary>
		/// <returns>Результат создания события</returns>
		public async Task EventCreate()
		{
			XDocument doc = new XDocument(
				new XElement("Events",
					new XElement("Sport", new XAttribute("Name", $"{CurrentMapEvent.Name}"),
						new XElement("Description", $"{CurrentMapEvent.Description}"),
						new XElement("Place", $"{CurrentMapEvent.Place}"),
						new XElement("User", $"{CurrentMapEvent.UserBind.LastName}"),
						new XElement("Date", $"{CurrentMapEvent.EventDate}"),
						new XElement("Time", $"{CurrentMapEvent.EventTime}"),
						new XElement("Span", $"{CurrentMapEvent.EventTimeSpan}"),
						new XElement("Interval", $"{CurrentMapEvent.EventInterval}")
					)
				)
			);
			await (new MessageDialog($"{doc.ToString()}")).ShowAsync();
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