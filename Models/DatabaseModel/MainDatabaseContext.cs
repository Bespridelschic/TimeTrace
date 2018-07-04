using Microsoft.EntityFrameworkCore;
using Models.EventModel;

namespace Models.DatabaseModel
{
	/// <summary>
	/// Main database context
	/// </summary>
	public sealed class MainDatabaseContext : DbContext
	{
		public DbSet<Event> Events { get; private set; }
		public DbSet<Calendar> Calendars { get; private set; }
		public DbSet<Project> Projects { get; private set; }
		//public DbSet<Contact> Contacts { get; set; }        TODO Make contacts in next update

		public MainDatabaseContext()
		{
			Database.EnsureCreated();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite("Filename=LocalBase.db");
		}
	}
}