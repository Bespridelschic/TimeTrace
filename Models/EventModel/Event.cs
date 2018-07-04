using System;
using System.ComponentModel.DataAnnotations;
using Models.EventModel.EntitiesFoundationModel;

namespace Models.EventModel
{
	/// <summary>
	/// Event class for implementation of time management model
	/// </summary>
	public class Event : Category, ICloneable
	{
		#region Fields

		private string projectId;
		private string location;
		private string associatedPerson;
		private string eventInterval;
		private DateTime notificationTime;
		private bool isPublic;

		#endregion

		#region Properties

		/// <summary>
		/// Id of binded area
		/// </summary>
		[Required]
		public string ProjectId
		{
			get => projectId;
			set
			{
				projectId = value;
				OnPropertyChanged();
			}
		}
		
		/// <summary>
		/// Is public event
		/// </summary>
		public bool IsPublic
		{
			get => isPublic;
			set
			{
				isPublic = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Full start event time
		/// </summary>
		[Required]
		public DateTime Start { get; set; }

		/// <summary>
		/// Full end event time
		/// </summary>
		[Required]
		public DateTime End { get; set; }
		
		/// <summary>
		/// Event location
		/// </summary>
		public string Location
		{
			get => location;
			set
			{
				location = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// The person associated with the event
		/// </summary>
		public string AssociatedPerson
		{
			get => associatedPerson;
			set
			{
				associatedPerson = value;
				OnPropertyChanged();
			}
		}
		
		/// <summary>
		/// Repeat the event
		/// </summary>
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

		/// <summary>
		/// Pre-warning time
		/// </summary>
		public DateTime NotificationTime
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
		public Event()
		{
			IsPublic = false;
		}

		/// <summary>
		/// Standart constructor for create new event
		/// </summary>
		/// <param name="owner"></param>
		public Event(string owner) : base(owner)
		{
			IsPublic = false;
		}

		/// <summary>
		/// Create builder for creation new <seealso cref="Event"/> object in fluent notation
		/// </summary>
		/// <returns>New object of <seealso cref="EventBuilder"/></returns>
		public static EventBuilder CreateBuilder()
		{
			return new EventBuilder();
		}

		/// <summary>
		/// Create builder for creation new <seealso cref="Event"/> object in fluent notation
		/// </summary>
		/// <param name="owner">Owner of created event</param>
		/// <returns>New object of <seealso cref="EventBuilder"/></returns>
		public static EventBuilder CreateBuilder(string owner)
		{
			return new EventBuilder(owner);
		}

		#endregion

		/// <inheritdoc />
		/// <summary>
		/// Deep cloning of current object
		/// </summary>
		/// <returns>New object of event</returns>
		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}