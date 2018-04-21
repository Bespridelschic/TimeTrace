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
using TimeTrace.Annotations;

namespace TimeTrace.Model
{
	/// <summary>
	/// Contact class
	/// </summary>
	public class Contact : BaseEntity
	{
		#region Properties

		private string name;
		/// <summary>
		/// Name of contact
		/// </summary>
		[JsonProperty(PropertyName = "name")]
		public string Name
		{
			get => name;
			set
			{
				name = value;
				UpdateAt = DateTime.UtcNow;
				OnPropertyChanged();
			}
		}

		private string email;
		/// <summary>
		/// E-mail address of contact
		/// </summary>
		[JsonProperty(PropertyName = "email")]
		public string Email
		{
			get => email;
			set
			{
				email = value;
				OnPropertyChanged();
			}
		}

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		public Contact() : base()
		{

		}
	}
}