using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models.EventModel.EntitiesFoundationModel.BaseEntity;

namespace Models.DatabaseModel
{
    public class DatabaseService
    {
		public async Task<IList<T>> GetDataAsync<T>(DbSet<T> dataSet, string owner) where T : class, IBaseEntity
		{
			if (string.IsNullOrEmpty(owner))
			{
				throw new ArgumentNullException("Owner cannot be null or empty");
			}

			using (var db = new MainDatabaseContext())
			{
				var result = await dataSet
					.Where(i => i.Owner == owner && !i.IsDelete)
					.Select(i => i)
					.ToListAsync();

				return result;
			}
		}
	}
}
