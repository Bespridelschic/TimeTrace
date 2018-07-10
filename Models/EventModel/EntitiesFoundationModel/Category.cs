using System.ComponentModel;
using System.Runtime.CompilerServices;
using Models.Annotations;

namespace Models.EventModel.EntitiesFoundationModel
{
	/// <summary>
	/// Abstract base class for Area, Project and EventMap classes
	/// </summary>
	public abstract class Category : BaseEntity.BaseEntity, INotifyPropertyChanged
	{
		#region Fields

		private string name;
		private string description;
		private int color;

		#endregion

		#region Properties

		/// <summary>
		/// Name of calendar
		/// </summary>
		public string Name
		{
			get => name;
			set
			{
				name = value;
				OnPropertyChanged();
			}
		}
		
		/// <summary>
		/// Description of calendar
		/// </summary>
		public string Description
		{
			get => description;
			set
			{
				description = value;
				OnPropertyChanged();
			}
		}
		
		/// <summary>
		/// Color of calendar
		/// </summary>
		public int Color
		{
			get => color;
			set
			{
				color = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Empty constructor for deserializing
		/// </summary>
		public Category()
		{
			
		}

		/// <summary>
		/// Standart constructor for initializing new category
		/// </summary>
		public Category(string owner) : base(owner)
		{
			Color = 1;
		}

		#endregion

		#region MVVM

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}