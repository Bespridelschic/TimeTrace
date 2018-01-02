using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrace.Model
{
	/// <summary>
	/// Класс события календаря
	/// </summary>
	public class MapEvent : INotifyPropertyChanged
	{
		#region Свойства

		private string name;
		/// <summary>
		/// Наименование события
		/// </summary>
		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				OnPropertyChanged();
			}
		}

		private string description;
		/// <summary>
		/// Описание события
		/// </summary>
		public string Description
		{
			get { return description; }
			set
			{
				description = value;
				OnPropertyChanged();
			}
		}

		private DateTimeOffset? eventDate;
		/// <summary>
		/// Дата события
		/// </summary>
		public DateTimeOffset? EventDate
		{
			get { return eventDate; }
			set
			{
				eventDate = value;
				OnPropertyChanged();
			}
		}

		private TimeSpan eventTime;
		/// <summary>
		/// Время события
		/// </summary>
		public TimeSpan EventTime
		{
			get { return eventTime; }
			set
			{
				eventTime = value;
				OnPropertyChanged();
			}
		}

		private string place;
		/// <summary>
		/// Место привязки
		/// </summary>
		public string Place
		{
			get { return place; }
			set
			{
				place = value;
				OnPropertyChanged();
			}
		}

		private User userBind;
		/// <summary>
		/// Привязка события на человека
		/// </summary>
		public User UserBind
		{
			get { return userBind; }
			set
			{
				userBind = value;
				OnPropertyChanged();
			}
		}

		private TimeSpan? eventTimeSpan;
		/// <summary>
		/// Продолжительность события
		/// </summary>
		public TimeSpan? EventTimeSpan
		{
			get { return eventTimeSpan; }
			set
			{
				eventTimeSpan = value;
				OnPropertyChanged();
			}
		}

		private TimeSpan? eventInterval;
		/// <summary>
		/// Повторяемость события
		/// </summary>
		public TimeSpan? EventInterval
		{
			get { return eventInterval; }
			set { eventInterval = value; }
		}

		private EventType typeOfEvent;
		/// <summary>
		/// Тип события календаря
		/// </summary>
		public EventType TypeOfEvent
		{
			get { return typeOfEvent; }
			set
			{
				typeOfEvent = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Перечисления

		/// <summary>
		/// Перечисляемый тип события
		/// </summary>
		public enum EventType
		{
			NotDefined,
			Health,
			Sport,
			Study
		}

		#endregion

		#region Конструкторы

		/// <summary>
		/// Стандартный конструктор
		/// </summary>
		public MapEvent()
		{
			TypeOfEvent = EventType.NotDefined;
			UserBind = new User();
		}

		/// <summary>
		/// Установка события
		/// </summary>
		/// <param name="eventName">Имя события</param>
		/// <param name="date">Дата события</param>
		/// <param name="time">Время события</param>
		/// <param name="place">Место события</param>
		/// <param name="eventTimeSpan">Продолжительность события</param>
		/// <param name="eventInterval">Интервал повторения события</param>
		/// <param name="eventType">Тип события</param>
		public MapEvent(string eventName, string description, DateTimeOffset? date, TimeSpan time, string place, string user, TimeSpan? eventTimeSpan, TimeSpan? eventInterval, EventType eventType)
		{
			Name = eventName;
			Description = description;
			EventDate = date;
			EventTime = time;
			Place = place;
			UserBind = new User();
			userBind.LastName = user;
			EventTimeSpan = eventTimeSpan;
			EventInterval = eventInterval;
			TypeOfEvent = eventType;
		}

		#endregion

		#region MVVM

		public event PropertyChangedEventHandler PropertyChanged = delegate { };
		private void OnPropertyChanged([CallerMemberName]string prop = "")
		{
			PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}

		#endregion
	}
}