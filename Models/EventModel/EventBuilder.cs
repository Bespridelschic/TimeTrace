using System;
using System.Collections.Generic;
using System.Text;

namespace Models.EventModel
{
	/// <summary>
	/// <seealso cref="Event"/> object builder in fluent notation
	/// </summary>
	public class EventBuilder
	{
		private Event @event;

		public EventBuilder(Project project)
		{
			@event = new Event(project);
			@event.Start = @event.End = DateTime.UtcNow;
		}

		public EventBuilder SetName(string name)
		{
			@event.Name = !string.IsNullOrEmpty(name?.Trim()) ? name.Trim() : null;
			return this;
		}

		public EventBuilder SetDescription(string description)
		{
			@event.Description = !string.IsNullOrEmpty(description?.Trim()) ? description.Trim() : null;
			return this;
		}

		public EventBuilder SetLocation(string location)
		{
			@event.Location = !string.IsNullOrEmpty(location?.Trim()) ? location.Trim() : null;
			return this;
		}

		public EventBuilder SetAssociatedPerson(string associatedPerson)
		{
			@event.AssociatedPerson = !string.IsNullOrEmpty(associatedPerson?.Trim()) ? associatedPerson.Trim() : null;
			return this;
		}

		public EventBuilder SetStart(DateTime start)
		{
			@event.Start = start.ToUniversalTime();

			if (@event.Start > @event.End)
			{
				@event.End = @event.Start;
			}

			return this;
		}

		public EventBuilder SetEnd(DateTime end)
		{
			@event.End = end.ToUniversalTime();

			if (@event.Start > @event.End)
			{
				@event.End = @event.Start;
			}

			return this;
		}

		public EventBuilder SetEventInterval(string interval)
		{
			@event.EventInterval = !string.IsNullOrEmpty(interval) ? interval : null;
			return this;
		}

		public EventBuilder SetColor(int color)
		{
			if (color > 11 || color < 1)
			{
				throw new IndexOutOfRangeException("Color must be between 1 and 11 include");
			}

			@event.Color = color;
			return this;
		}

		public EventBuilder SetRootProject(Project project)
		{
			@event.ProjectId = project.Id;
			return this;
		}

		public EventBuilder SetPublicity(bool isPublic)
		{
			@event.IsPublic = isPublic;
			return this;
		}

		/// <summary>
		/// Implicit converting from <seealso cref="EventBuilder"/> to <seealso cref="Event"/>
		/// </summary>
		/// <param name="builder">Converted event builder</param>
		public static implicit operator Event(EventBuilder builder)
		{
			return builder.@event;
		}
	}
}