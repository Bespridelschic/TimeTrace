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
	/// Event map class
	/// </summary>
	public class MapEvent : INotifyPropertyChanged
	{
		#region Properties

		private string name;
		/// <summary>
		/// Event name
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
		/// Event description
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
		/// Event start date
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
		/// Event start time
		/// </summary>
		public TimeSpan EventTime
		{
			get { return eventTime; }
			set
			{
				if (eventTime != value)
				{
					eventTime = value;
					OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// Full event date and time
		/// </summary>
		public DateTime FullEventDate
		{
			get
			{
				return EventDate.Value.Date + EventTime;
			}
		}

		private string place;
		/// <summary>
		/// Event place
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
		/// The person associated with the event
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

		private TimeSpan eventDuration;
		/// <summary>
		/// Event duration
		/// </summary>
		public TimeSpan EventDuration
		{
			get { return eventDuration; }
			set
			{
				if (eventDuration != value)
				{
					eventDuration = value;
					OnPropertyChanged();
				}
			}
		}

		private TimeSpan eventInterval;
		/// <summary>
		/// Repeat the event
		/// </summary>
		public TimeSpan EventInterval
		{
			get { return eventInterval; }
			set
			{
				if (eventInterval != value)
				{
					eventInterval = value;
					OnPropertyChanged();
				}
			}
		}

		private EventType typeOfEvent;
		/// <summary>
		/// Event type
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

		#region Enums

		/// <summary>
		/// Type of event
		/// </summary>
		public enum EventType
		{
			NotDefined,
			Health,
			Sport,
			Study
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Standart constructor
		/// </summary>
		public MapEvent()
		{
			TypeOfEvent = EventType.NotDefined;
			UserBind = new User();
		}

		/// <summary>
		/// Event set
		/// </summary>
		/// <param name="eventName">Event name</param>
		/// <param name="date">Event start date</param>
		/// <param name="time">Event start time</param>
		/// <param name="place">Event place</param>
		/// <param name="eventTimeSpan">Event duration</param>
		/// <param name="eventInterval">Repeat the event</param>
		/// <param name="eventType">Event type</param>
		public MapEvent(string eventName, string description, DateTimeOffset? date, TimeSpan time, string place, string user, TimeSpan eventTimeSpan, TimeSpan eventInterval, EventType eventType)
		{
			Name = eventName;
			Description = description;
			EventDate = date;
			EventTime = time;
			Place = place;
			UserBind = new User();
			userBind.LastName = user;
			EventDuration = eventTimeSpan;
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