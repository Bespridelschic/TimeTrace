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
		/// Standart constructor for creation new project
		/// </summary>
		public Project(string owner) : base(owner)
		{
			Creator = owner;
		}

		/// <summary>
		/// Standart constructor for loading projects of calendar
		/// </summary>
		/// <param name="calendar"></param>
		public Project(Calendar calendar)
		{
			CalendarId = calendar.Id;
		}

		#endregion
	}
}