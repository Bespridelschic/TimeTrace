using Newtonsoft.Json;

namespace TimeTrace.Model.Requests
{
	public class Invitation
	{
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }


		[JsonProperty(PropertyName = "email")]
		public string Email { get; set; }

		[JsonProperty(PropertyName = "projectId")]
		public string ProjectId { get; set; }

		[JsonProperty(PropertyName = "status")]
		public int Status { get; set; }

		[JsonProperty(PropertyName = "from")]
		public string From { get; set; }

		/// <summary>
		/// Standart constructor
		/// </summary>
		public Invitation()
		{

		}

		/// <summary>
		/// Constructor for initializing invitation
		/// </summary>
		/// <param name="email">Email of contact</param>
		/// <param name="projectId">Id of sended project</param>
		public Invitation(string email, string projectId)
		{
			Email = email;
			ProjectId = projectId;

			Id = From = string.Empty;
			Status = 0;
		}

		/// <summary>
		/// Constructor for initializing getting invitation
		/// </summary>
		/// <param name="email">Email of contact</param>
		/// <param name="projectId">Id of sended project</param>
		/// <param name="id">Id of invitation</param>
		/// <param name="status">Status of invitation. 0 - not viewed, 1 - accepted, 2 - denied</param>
		/// <param name="personEmail">Email of owner</param>
		public Invitation(string email, string projectId, string id, int status, string from) : this(email, projectId)
		{
			Id = id;
			Status = status;
			From = from;
		}
	}
}
