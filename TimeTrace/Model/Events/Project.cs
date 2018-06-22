using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrace.Model.Events
{
	/// <summary>
	/// Project class. Second level of calendar
	/// </summary>
	public class Project : Category
	{
		#region Properties

		private string areaId;
		/// <summary>
		/// Identity of area
		/// </summary>
		[JsonProperty(PropertyName = "area_id")]
		public string AreaId
		{
			get => areaId;
			set
			{
				areaId = value;
				OnPropertyChanged();
			}
		}

		private string from;
		/// <summary>
		/// Email of original creator
		/// </summary>
		[JsonProperty(PropertyName = "from")]
		public string From
		{
			get => from;
			set
			{
				from = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Standart constructor
		/// </summary>
		public Project()
		{
			
		}

		/// <summary>
		/// Standart constructor with Area ID parameter
		/// </summary>
		/// <param name="areaId"></param>
		public Project(string areaId) : this()
		{
			AreaId = areaId;
		}

		#endregion
	}
}
