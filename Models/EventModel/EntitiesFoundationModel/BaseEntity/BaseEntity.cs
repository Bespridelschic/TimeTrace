using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.EventModel.EntitiesFoundationModel.BaseEntity
{
	/// <summary>
	/// Abstract class for basic entities in database
	/// </summary>
	public abstract class BaseEntity : IBaseEntity
	{
		#region Fields

		private DateTime? createAt;
		private DateTime? updateAt;

		#endregion

		#region Properties

		/// <summary>
		/// GUID idenity
		/// </summary>
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public string Id { get; set; }

		/// <summary>
		/// Entity creation date
		/// </summary>
		public DateTime? CreateAt
		{
			get => createAt;
			set
			{
				if (value != null)
				{
					createAt = value;
				}
			}
		}

		/// <summary>
		/// Entity last update date
		/// </summary>
		public DateTime? UpdateAt
		{
			get => updateAt;
			set
			{
				if (value != null)
				{
					updateAt = value;
				}
			}
		}

		/// <summary>
		/// If deleted - remove local entity
		/// </summary>
		public bool IsDelete { get; set; }

		/// <summary>
		/// E-mail address of entity owner
		/// </summary>
		public string Owner { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Standart constructor
		/// </summary>
		public BaseEntity()
		{
			Id = Guid.NewGuid().ToString();
			UpdateAt = CreateAt = DateTime.Now.ToUniversalTime();
			IsDelete = false;
		}

		/// <summary>
		/// Base standart constructor for creation of new entity
		/// </summary>
		public BaseEntity(string owner) : this()
		{
			Owner = owner;
		}

		#endregion
	}
}