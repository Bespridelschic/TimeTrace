using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrace.Model.DBContext
{
	/// <summary>
	/// Data base map event contet
	/// </summary>
	class MapEventContext : DbContext
	{
		public DbSet<MapEvent> MapEvents { get; set; }

		public MapEventContext()
		{
			//Database.EnsureCreated();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite("Filename=MapEventDB.db");
		}
	}
}
