using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace TimeTrace.Model.Events
{
	/// <summary>
	/// Abstract base class for Area, Project and EventMap classes
	/// </summary>
	public abstract class Category : BasicEntity
	{
		#region Properties

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

		private int color;
		/// <summary>
		/// Color of calendar
		/// </summary>
		[JsonProperty(PropertyName = "color")]
		public int Color
		{
			get { return color; }
			set
			{
				color = value;
				OnPropertyChanged();
			}
		}

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		public Category() : base()
		{
			
		}
	}
}
