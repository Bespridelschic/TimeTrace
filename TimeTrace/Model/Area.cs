using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrace.Model
{
	public class Area : INotifyPropertyChanged
	{
		#region Properties

		private string id;
		/// <summary>
		/// Idenity of area
		/// </summary>
		public string Id
		{
			get { return id; }
			set
			{
				id = value;
				OnPropertyChanged();
			}
		}

		private string name;
		/// <summary>
		/// Area name
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
		/// Area description
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

		private string parentId;
		/// <summary>
		/// Identity of parent area
		/// </summary>
		public string ParentId
		{
			get { return parentId; }
			set
			{
				parentId = value;
				OnPropertyChanged();
			}
		}

		private string childId;
		/// <summary>
		/// Identity of child area
		/// </summary>
		public string ChildId
		{
			get { return childId; }
			set
			{
				childId = value;
				OnPropertyChanged();
			}
		}

		private DateTime date;
		/// <summary>
		/// Area creation date
		/// </summary>
		public DateTime Date
		{
			get { return date; }
			set
			{
				date = value;
				OnPropertyChanged();
			}
		}

		private bool favorite;
		/// <summary>
		/// Is area favorite
		/// </summary>
		public bool Favorite

		{
			get { return favorite; }
			set
			{
				favorite = value;
				OnPropertyChanged();
			}
		}

		#endregion

		public Area()
		{
			Id = Guid.NewGuid().ToString();
		}

		#region MVVM

		public event PropertyChangedEventHandler PropertyChanged = delegate { };
		public void OnPropertyChanged([CallerMemberName] string property = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}

		#endregion
	}
}
