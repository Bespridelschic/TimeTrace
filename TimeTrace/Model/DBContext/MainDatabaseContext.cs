using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTrace.Model.Events;

namespace TimeTrace.Model.DBContext
{
	/// <summary>
	/// Main database context
	/// </summary>
	class MainDatabaseContext : DbContext
	{
		public DbSet<MapEvent> MapEvents { get; set; }
		public DbSet<Area> Areas { get; set; }
		public DbSet<Project> Projects { get; set; }
		public DbSet<Contact> Contacts { get; set; }

		public MainDatabaseContext()
		{
			Database.EnsureCreated();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite("Filename=local.db");
		}
	}
}
