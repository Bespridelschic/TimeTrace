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

namespace TimeTrace.Model
{
	public class Area : INotifyPropertyChanged
	{
		#region Properties

		private string id;
		/// <summary>
		/// Idenity of area
		/// </summary>
		[JsonProperty(PropertyName = "area_id")]
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
		/// Area name
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
		/// Area description
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

		private string parentId;
		/// <summary>
		/// Identity of parent area
		/// </summary>
		[JsonProperty(PropertyName = "parent_id")]
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
		[JsonProperty(PropertyName = "child_id")]
		public string ChildId
		{
			get { return childId; }
			set
			{
				childId = value;
				OnPropertyChanged();
			}
		}

		private DateTime updateAt;
		/// <summary>
		/// Area creation or update date
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

		private bool favorite;
		/// <summary>
		/// Is area favorite
		/// </summary>
		[JsonProperty(PropertyName = "favourite")]
		public bool Favorite

		{
			get { return favorite; }
			set
			{
				favorite = value;
				OnPropertyChanged();
			}
		}

		private bool isDelete;
		/// <summary>
		/// If deleted - remove local area
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

		private string color;
		/// <summary>
		/// Color of area
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

		#endregion

		public Area(string parentId)
		{
			Id = Guid.NewGuid().ToString();
			IsDelete = false;
			UpdateAt = DateTime.Now;

			ParentId = parentId;
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
