using System;
using System.Collections.Generic;
using System.Text;

namespace Models.EventModel.EntitiesFoundationModel.BaseEntity
{
	/// <summary>
	/// Base interface for all entities
	/// </summary>
    public interface IBaseEntity
    {
		string Id { get; }
		string Owner { get; }
		bool IsDelete { get; }
		DateTime? CreateAt { get; }
		DateTime? UpdateAt { get; }
    }
}
