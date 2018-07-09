using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models.EventModel;
using Models.EventModel.EntitiesFoundationModel.BaseEntity;

namespace Models.DatabaseModel
{
	/// <summary>
	/// Requests for database
	/// </summary>
	public class DatabaseService
	{
		/// <summary>
		/// Current owner of required data
		/// </summary>
		private readonly string owner;

		/// <summary>
		/// Standart constructor
		/// </summary>
		/// <param name="owner">Current owner of required data</param>
		public DatabaseService(string owner)
		{
			this.owner = owner;
		}

		/// <summary>
		/// Getting not deleted data set of current user
		/// </summary>
		/// <typeparam name="T">Types that implement the IBaseEntity interface</typeparam>
		/// <param name="dataSet">Required data set</param>
		/// <returns><seealso cref="IQueryable{T}"/> data set</returns>
		public IQueryable<T> DatabaseRequest<T>(DbSet<T> dataSet) where T : class, IBaseEntity
		{
			if (string.IsNullOrEmpty(owner))
			{
				throw new ArgumentNullException("Owner cannot be null or empty");
			}

			using (var db = new MainDatabaseContext())
			{
				var result = dataSet
					.Where(i => i.Owner == owner && !i.IsDelete);

				return result;
			}
		}

		/// <summary>
		/// Getting not finished events
		/// </summary>
		/// <param name="dataSet">Required data set</param>
		/// <returns><seealso cref="IQueryable{T}"/> data set</returns>
		public IQueryable<Event> ActualEventsRequest(DbSet<Event> dataSet)
		{
			return DatabaseRequest(dataSet).Where(i => i.End > DateTime.UtcNow);
		}

		/// <summary>
		/// Getting finished events
		/// </summary>
		/// <param name="dataSet">Required data set</param>
		/// <returns><seealso cref="IQueryable{T}"/> data set</returns>
		public IQueryable<Event> PastEventsRequest(DbSet<Event> dataSet)
		{
			return DatabaseRequest(dataSet).Where(i => i.End <= DateTime.UtcNow);
		}

		public IQueryable<Event> EventsFiltrationRequest(DbSet<Event> dataSet, bool isPastEvents = false, bool isPublics = true, string person = null, string location = null, int color = 1)
		{
			throw new NotImplementedException();

			if (isPastEvents)
			{
				return PastEventsRequest(dataSet).Where(i => i.IsPublic == isPublics);
			}

			return ActualEventsRequest(dataSet).Where(i => i.IsPublic == isPublics);
		}
	}
}