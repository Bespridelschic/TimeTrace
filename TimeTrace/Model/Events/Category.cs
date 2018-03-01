using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrace.Model.Events
{
	/// <summary>
	/// Abstract base class for Area, Project and EventMap classes
	/// </summary>
	public abstract class Category : INotifyPropertyChanged
	{
		#region Properties

		private string id;
		/// <summary>
		/// GUID idenity
		/// </summary>
		[JsonProperty(PropertyName = "id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
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
		/// Name of calendar
		/// </summary>
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
		/// Description of calendar
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

		private string color;
		/// <summary>
		/// Color of calendar
		/// </summary>
		[JsonProperty(PropertyName = "color")]
		public string Color
		{
			get { return color; }
			set
			{
				color = value;
				OnPropertyChanged();
			}
		}

		private DateTime updateAt;
		/// <summary>
		/// Calendar creation or last update date
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

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		public Category()
		{
			Id = Guid.NewGuid().ToString();
			UpdateAt = DateTime.Now;
		}

		#region MVVM

		/// <summary>
		/// Event
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		/// <summary>
		/// Event registration
		/// </summary>
		/// <param name="property">Caller properties name</param>
		public void OnPropertyChanged([CallerMemberName]string property = "")
		{
			PropertyChanged(this, new PropertyChangedEventArgs(property));
		}

		#endregion
	}
}
