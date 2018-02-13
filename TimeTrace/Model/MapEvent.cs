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

		private DateTimeOffset? startEventDate;
		/// <summary>
		/// Event start date
		/// </summary>
		public DateTimeOffset? StartEventDate
		{
			get { return startEventDate; }
			set
			{
				startEventDate = value;
				OnPropertyChanged();
			}
		}

		private TimeSpan startEventTime;
		/// <summary>
		/// Event start time
		/// </summary>
		public TimeSpan StartEventTime
		{
			get { return startEventTime; }
			set
			{
				if (startEventTime != value)
				{
					startEventTime = value;
					OnPropertyChanged();
				}
			}
		}

		private DateTimeOffset? endEventDate;
		/// <summary>
		/// Event end date
		/// </summary>
		public DateTimeOffset? EndEventDate
		{
			get { return endEventDate; }
			set
			{
				endEventDate = value;
				OnPropertyChanged();
			}
		}

		private TimeSpan endEventTime;
		/// <summary>
		/// Event end time
		/// </summary>
		public TimeSpan EndEventTime
		{
			get { return endEventTime; }
			set
			{
				if (endEventTime != value)
				{
					endEventTime = value;
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
				return StartEventDate.Value.Date + StartEventTime;
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
			StartEventDate = date;
			StartEventTime = time;
			Place = place;
			UserBind = new User();
			userBind.LastName = user;
			EndEventTime = eventTimeSpan;
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