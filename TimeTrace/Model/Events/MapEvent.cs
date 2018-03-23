using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace TimeTrace.Model.Events
{
	/// <summary>
	/// Event map class
	/// </summary>
	public class MapEvent : Category
	{
		#region Properties

		private string projectId;
		/// <summary>
		/// Id of binded area
		/// </summary>
		[Required]
		[JsonProperty(PropertyName = "project_id")]
		public string ProjectId
		{
			get { return projectId; }
			set
			{
				projectId = value;
				OnPropertyChanged();
			}
		}

		private bool isDelete;
		/// <summary>
		/// If deleted - remove local event
		/// </summary>
		[JsonIgnore]
		public bool IsDelete
		{
			get { return isDelete; }
			set
			{
				isDelete = value;
				OnPropertyChanged();
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
				return start;
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
				return end;
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
		[JsonProperty(PropertyName = "people")]
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
		/// Standart constructor
		/// </summary>
		public MapEvent()
		{
			
		}

		/// <summary>
		/// Event set constructor
		/// </summary>
		/// <param name="name">Event name</param>
		/// <param name="description">Event description</param>
		/// <param name="start">Start event date time</param>
		/// <param name="end">End event date time</param>
		/// <param name="place">Event location</param>
		/// <param name="user">Binded user</param>
		/// <param name="eventInterval">Repeat of event</param>
		/// <param name="projectId">ID of map project</param>
		public MapEvent(string name, string description, DateTime start, DateTime end, string place, string user, string eventInterval, string projectId)
		{
			Name = name;
			Description = description;
			Start = start;
			End = end;
			Location = place;
			EventInterval = eventInterval;
			ProjectId = projectId;
			UserBind = user;
		}

		#endregion
	}
}