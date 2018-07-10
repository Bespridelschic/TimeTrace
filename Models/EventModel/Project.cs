using Models.EventModel.EntitiesFoundationModel;

namespace Models.EventModel
{
	/// <summary>
	/// Project class for implementation of time management model. This class is root for events
	/// </summary>
	public class Project : Category
	{
		#region Properties

		/// <summary>
		/// Identity of area
		/// </summary>
		public string CalendarId { get; set; }
		
		/// <summary>
		/// Email of original creator
		/// </summary>
		public string Creator { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Empty constructor for deserializing
		/// </summary>
		public Project()
		{
			
		}

		/// <summary>
		/// Standart constructor for loading projects of calendar
		/// </summary>
		/// <param name="calendar">Root calendar</param>
		public Project(Calendar calendar) : base(calendar.Owner)
		{
			CalendarId = calendar.Id;
			Creator = calendar.Owner;
		}

		#endregion
	}
}