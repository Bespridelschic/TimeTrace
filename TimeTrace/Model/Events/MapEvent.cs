using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace TimeTrace.Model
{
	/// <summary>
	/// Event map class
	/// </summary>
	public class MapEvent : INotifyPropertyChanged
	{
		#region Properties

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[JsonProperty(PropertyName = "id")]
		public string Id { get; private set; }

		private string areaId;
		/// <summary>
		/// Id of binded area
		/// </summary>
		[Required]
		[JsonProperty(PropertyName = "area_id")]
		public string AreaId
		{
			get { return areaId; }
			set
			{
				areaId = value;
				OnPropertyChanged();
			}
		}

		private DateTime updateAt;
		/// <summary>
		/// Last updateAt of update event
		/// </summary>
		[JsonProperty(PropertyName = "update_at")]
		public DateTime UpdateAt
		{
			get { return updateAt; }
			set
			{
				updateAt = value;
				OnPropertyChanged();
			}
		}

		private bool isDelete;
		/// <summary>
		/// If deleted - remove local event
		/// </summary>
		[JsonProperty(PropertyName = "is_delete")]
		public bool IsDelete
		{
			get { return isDelete; }
			set
			{
				isDelete = value;
				OnPropertyChanged();
			}
		}

		private string name;
		/// <summary>
		/// Event name
		/// </summary>
		[Required]
		[JsonProperty(PropertyName = "summary")]
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
		[JsonProperty(PropertyName = "description")]
		public string Description
		{
			get { return description; }
			set
			{
				description = value;
				OnPropertyChanged();
			}
		}

		private DateTimeOffset? startDate;
		/// <summary>
		/// Event start updateAt
		/// </summary>
		[JsonIgnore]
		public DateTimeOffset? StartDate
		{
			get { return startDate; }
			set
			{
				startDate = value;
				OnPropertyChanged();
			}
		}

		private TimeSpan startTime;
		/// <summary>
		/// Event start time
		/// </summary>
		[JsonIgnore]
		public TimeSpan StartTime
		{
			get { return startTime; }
			set
			{
				if (startTime != value)
				{
					startTime = value;
					OnPropertyChanged();
				}
			}
		}

		private DateTimeOffset? endDate;
		/// <summary>
		/// Event end updateAt
		/// </summary>
		[JsonIgnore]
		public DateTimeOffset? EndDate
		{
			get { return endDate; }
			set
			{
				endDate = value;
				OnPropertyChanged();
			}
		}

		private TimeSpan endTime;
		/// <summary>
		/// Event end time
		/// </summary>
		[JsonIgnore]
		public TimeSpan EndTime
		{
			get { return endTime; }
			set
			{
				if (endTime != value)
				{
					endTime = value;
					OnPropertyChanged();
				}
			}
		}

		private DateTime start;
		/// <summary>
		/// Full start event updateAt and time
		/// </summary>
		[Required]
		[JsonProperty(PropertyName = "start")]
		public DateTime Start
		{
			set { start = value; }
			get
			{
				return StartDate.Value.Date + StartTime;
			}
		}

		private DateTime end;
		/// <summary>
		/// Full end event updateAt and time
		/// </summary>
		[Required]
		[JsonProperty(PropertyName = "end")]
		public DateTime End
		{
			set { end = value; }
			get
			{
				return EndDate.Value.Date + EndTime;
			}
		}

		private string location;
		/// <summary>
		/// Event location
		/// </summary>
		[JsonProperty(PropertyName = "location")]
		public string Location
		{
			get { return location; }
			set
			{
				location = value;
				OnPropertyChanged();
			}
		}

		private string userBind;
		/// <summary>
		/// The person associated with the event
		/// </summary>
		[JsonProperty(PropertyName = "person")]
		public string UserBind
		{
			get { return userBind; }
			set
			{
				userBind = value;
				OnPropertyChanged();
			}
		}

		private string eventInterval;
		/// <summary>
		/// Repeat the event
		/// </summary>
		[JsonProperty(PropertyName = "recurrence")]
		public string EventInterval
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

		#endregion

		#region Constructors

		/// <summary>
		/// Standart constructor with Area ID parameter
		/// </summary>
		public MapEvent(string areaId) : this()
		{
			AreaId = areaId;
		}

		/// <summary>
		/// Standart constructor
		/// </summary>
		public MapEvent()
		{
			Id = Guid.NewGuid().ToString();
		}

		/// <summary>
		/// Event set
		/// </summary>
		/// <param name="eventName">Event name</param>
		/// <param name="startDate">Event start updateAt</param>
		/// <param name="startTime">Event start time</param>
		/// <param name="place">Event location</param>
		/// <param name="eventTimeSpan">Event duration</param>
		/// <param name="eventInterval">Repeat the event</param>
		/// <param name="eventType">Event type</param>
		public MapEvent(string eventName, string description, DateTimeOffset? startDate,
			TimeSpan startTime, DateTimeOffset? endDate, TimeSpan endTime, string place,
			string user, string eventInterval, string areaId)
		{
			Name = eventName;
			Description = description;
			StartDate = startDate;
			StartTime = startTime;
			EndDate = endDate;
			EndTime = endTime;
			Location = place;
			EventInterval = eventInterval;
			AreaId = areaId;
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