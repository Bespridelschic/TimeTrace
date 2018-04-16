using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;

namespace TimeTrace.Model
{
	/// <summary>
	/// Abstract class for basic entities in database
	/// </summary>
	public abstract class BaseEntity : INotifyPropertyChanged
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
			get => id;
			set
			{
				id = value;
				OnPropertyChanged();
			}
		}

		private DateTime? createAt;
		/// <summary>
		/// Entity creation date
		/// </summary>
		[JsonProperty(PropertyName = "create_at")]
		public DateTime? CreateAt
		{
			get => createAt;
			set
			{
				if (value != null)
				{
					createAt = value;
				}
				OnPropertyChanged();
			}
		}

		private DateTime? updateAt;
		/// <summary>
		/// Entity last update date
		/// </summary>
		[JsonProperty(PropertyName = "update_at")]
		public DateTime? UpdateAt
		{
			get => updateAt;
			set
			{
				updateAt = value;
				OnPropertyChanged();
			}
		}

		private bool isDelete;
		/// <summary>
		/// If deleted - remove local entity
		/// </summary>
		[JsonIgnore]
		public bool IsDelete
		{
			get => isDelete;
			set
			{
				isDelete = value;
				OnPropertyChanged();
			}
		}

		private string emailOfOwner;
		/// <summary>
		/// E-mail address of entity owner
		/// </summary>
		[JsonProperty(PropertyName = "personEmail")]
		public string EmailOfOwner
		{
			get => emailOfOwner;
			set
			{
				emailOfOwner = value;
				OnPropertyChanged();
			}
		}

		#endregion

		/// <summary>
		/// Base standart constructor
		/// </summary>
		public BaseEntity()
		{
			Id = Guid.NewGuid().ToString();
			UpdateAt = CreateAt = DateTime.Now.ToUniversalTime();
			IsDelete = false;

			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
			EmailOfOwner = (string)localSettings.Values["email"];
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
