using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTrace.Model.Events;

namespace TimeTrace.Model.Events.DBContext
{
	/// <summary>
	/// Data base map event contet
	/// </summary>
	class MapEventContext : DbContext
	{
		public DbSet<MapEvent> MapEvents { get; set; }
		public DbSet<Area> Areas { get; set; }
		public DbSet<Project> Projects { get; set; }

		public MapEventContext()
		{
			Database.EnsureCreated();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite("Filename=MapEventsDB.db");
		}
	}
}
