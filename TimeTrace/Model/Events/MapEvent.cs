using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace TimeTrace.Model.Events
{
	/// <summary>
	/// Event map class
	/// </summary>
	public class MapEvent : Category, ICloneable
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
			get => projectId;
			set
			{
				projectId = value;
				OnPropertyChanged();
			}
		}

		private bool isPublic;
		/// <summary>
		/// Is public event
		/// </summary>
		[JsonProperty(PropertyName = "isPublic")]
		public bool IsPublic
		{
			get => isPublic;
			set
			{
				isPublic = value;
				OnPropertyChanged();
			}
		}

		private DateTime start;
		/// <summary>
		/// Full start event time
		/// </summary>
		[Required]
		[JsonProperty(PropertyName = "start")]
		public DateTime Start
		{
			set => start = value;
			get => start;
		}

		private DateTime end;
		/// <summary>
		/// Full end event time
		/// </summary>
		[Required]
		[JsonProperty(PropertyName = "end")]
		public DateTime End
		{
			set => end = value;
			get => end;
		}

		private string location;
		/// <summary>
		/// Event location
		/// </summary>
		[JsonProperty(PropertyName = "location")]
		public string Location
		{
			get => location;
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
			get => userBind;
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
			get => eventInterval;
			set
			{
				if (eventInterval != value)
				{
					eventInterval = value;
					OnPropertyChanged();
				}
			}
		}

		private string projectOwnerEmail;
		/// <summary>
		/// Email of creators event
		/// </summary>
		[JsonProperty(PropertyName = "projectPersonEmail")]
		public string ProjectOwnerEmail
		{
			get { return projectOwnerEmail; }
			set
			{
				projectOwnerEmail = value;
				OnPropertyChanged();
			}
		}

		private int notificationTime;
		/// <summary>
		/// Pre-warning time
		/// </summary>
		public int NotificationTime
		{
			get => notificationTime;
			set
			{
				notificationTime = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Standart constructor
		/// </summary>
		public MapEvent() : base()
		{
			ProjectOwnerEmail = EmailOfOwner;
			IsPublic = false;
		}

		/// <summary>
		/// Event set constructor
		/// </summary>
		/// <param name="name">Event name</param>
		/// <param name="description">Event description</param>
		/// <param name="start">Start event (date + time)</param>
		/// <param name="end">End event (date + time)</param>
		/// <param name="place">Event location</param>
		/// <param name="user">Binded user</param>
		/// <param name="eventInterval">Repeat of event</param>
		/// <param name="isPublic">Is event available for contacts</param>
		/// <param name="color">Color of event. Color is between 1 and 11 inclusive</param>
		/// <param name="projectId">ID of map project</param>
		public MapEvent(string name, string description, DateTime start, DateTime end, string place,
			string user, string eventInterval, bool isPublic, int color, string projectId) : this()
		{
			Name = name.Trim();

			if (!string.IsNullOrEmpty(description?.Trim()))
			{
				Description = description.Trim();
			}
			else
			{
				Description = string.Empty;
			}

			Start = start.ToUniversalTime();
			End = end.ToUniversalTime();

			if (!string.IsNullOrEmpty(place?.Trim()))
			{
				Location = place.Trim();
			}
			else
			{
				Location = string.Empty;
			}

			if (!string.IsNullOrEmpty(user?.Trim()))
			{
				UserBind = user.Trim();
			}
			else
			{
				UserBind = string.Empty;
			}

			EventInterval = eventInterval;

			IsPublic = isPublic;
			Color = color;
			ProjectId = projectId;
		}

		#endregion

		/// <summary>
		/// Deep cloning of current object
		/// </summary>
		/// <returns></returns>
		public object Clone()
		{
			return new MapEvent(Name, Description, Start.ToLocalTime(), End.ToLocalTime(), Location, UserBind, EventInterval, IsPublic, Color, ProjectId)
			{
				ProjectOwnerEmail = this.ProjectOwnerEmail,
				Id = this.Id,
				CreateAt = this.CreateAt,
				UpdateAt = this.UpdateAt,
				IsDelete = this.IsDelete,
				EmailOfOwner = this.EmailOfOwner
			};
		}
	}
}