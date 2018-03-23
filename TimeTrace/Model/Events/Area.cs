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
using TimeTrace.Model.Events;

namespace TimeTrace.Model.Events
{
	/// <summary>
	/// Area class. First level of calendar
	/// </summary>
	public class Area : Category
	{
		#region Properties

		private bool favorite;
		/// <summary>
		/// Is area favorite
		/// </summary>
		[JsonProperty(PropertyName = "favourite")]
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

		public Area()
		{
			Favorite = false;
		}
	}
}
