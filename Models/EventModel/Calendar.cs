using Models.EventModel.EntitiesFoundationModel;

namespace Models.EventModel
{
	/// <summary>
	/// Calendar class for implementation of time management model. This class is root for projects
	/// </summary>
	public class Calendar : Category
	{
		#region Fields

		private bool favorite;

		#endregion

		#region Properties

		/// <summary>
		/// Is calendar favorite
		/// </summary>
		public bool Favorite
		{
			get => favorite;
			set
			{
				favorite = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Constructors

		/// <inheritdoc />
		/// <summary>
		/// Standart constructor for creation new calendar
		/// </summary>
		/// <param name="owner">Owner of calendar</param>
		public Calendar(string owner) : base(owner)
		{
			Favorite = false;
		}

		#endregion
	}
}
