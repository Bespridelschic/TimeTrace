using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Models.UserModel;

namespace Models.InternetRequests
{
	/// <summary>
	/// Static class to work with internet server
	/// </summary>
	public static class InternetRequests
	{
		//	/// <summary>
		//	/// Base method for sending POST requests
		//	/// </summary>
		//	/// <param name="url">URL for request</param>
		//	/// <param name="data">Data in json format for sending</param>
		//	/// <returns>Answer of server in json format</returns>
		//	private static async Task<string> BasePostRequestAsync(string url, string data)
		//	{
		//		if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(data))
		//		{
		//			return string.Empty;
		//		}

		//		try
		//		{
		//			WebRequest request = WebRequest.Create(url);
		//			request.Method = "POST";

		//			byte[] byteArray = Encoding.UTF8.GetBytes(data);

		//			request.ContentType = "application/json";

		//			// Set header
		//			request.ContentLength = byteArray.Length;

		//			// Write data to stream
		//			using (Stream dataStream = request.GetRequestStream())
		//			{
		//				dataStream.Write(byteArray, 0, byteArray.Length);
		//			}

		//			string result = string.Empty;

		//			WebResponse response = await request.GetResponseAsync();
		//			using (Stream stream = response.GetResponseStream())
		//			{
		//				using (StreamReader reader = new StreamReader(stream ?? throw new InvalidOperationException()))
		//				{
		//					result += reader.ReadToEnd();
		//				}
		//			}
		//			response.Close();

		//			return result;
		//		}
		//		catch (Exception)
		//		{
		//			throw;
		//		}
		//	}

		//	/// <summary>
		//	/// Type of sending request
		//	/// </summary>
		//	public enum PostRequestDestination
		//	{
		//		SignIn,
		//		SignInWithToken,
		//		SignUp,
		//		AccountActivation,
		//		PasswordReset,
		//	}

		//	/// <summary>
		//	/// Sending data to server
		//	/// </summary>
		//	/// <param name="destination">Send request to</param>
		//	/// <param name="user">Object of <see cref="User"/></param>
		//	/// <returns>Return code from server: 0 - success, 1 - fail</returns>
		//	public static async Task<int> PostRequestAsync(PostRequestDestination destination, User user = null)
		//	{
		//		string link = string.Empty;
		//		string result = string.Empty;

		//		try
		//		{
		//			switch (destination)
		//			{
		//				case PostRequestDestination.SignIn:
		//					link = "https://planningway.ru/customer/login";
		//					result = await BasePostRequestAsync(link, JsonSerialize(user));
		//					break;

		//				case PostRequestDestination.SignInWithToken:
		//					link = "https://planningway.ru/customer/login";
		//					var res = await FileSystemRequests.LoadUserEmailAndTokenFromFileAsync();

		//					if (string.IsNullOrEmpty(res.email) || string.IsNullOrEmpty(res.token))
		//					{
		//						throw new Exception("File with email and token not fount");
		//					}

		//					result = await BasePostRequestAsync(link, TokenJsonSerialize(res.email, res.token));
		//					break;

		//				case PostRequestDestination.SignUp:
		//					link = "https://planningway.ru/customer/sign-up";
		//					result = await BasePostRequestAsync(link, JsonSerialize(user));
		//					break;

		//				case PostRequestDestination.AccountActivation:
		//					link = "https://planningway.ru/customer/send-activation-key";
		//					result = await BasePostRequestAsync(link, JsonSerialize(user));
		//					break;

		//				case PostRequestDestination.PasswordReset:
		//					link = "https://planningway.ru/customer/send-reset-key";
		//					result = await BasePostRequestAsync(link, JsonSerialize(user));
		//					break;

		//				default:
		//					throw new ArgumentException("Not fount type of sending request PostRequestDestination");
		//			}

		//			if (string.IsNullOrEmpty(result))
		//			{
		//				throw new NullReferenceException("Server return null");
		//			}

		//			// Json string parsing
		//			JObject JsonString = JObject.Parse(result);

		//			int answerCode = (int)JsonString["answer"];
		//			if ((destination == PostRequestDestination.SignIn || destination == PostRequestDestination.SignInWithToken) && answerCode == 0)
		//			{
		//				string token = (string)JsonString["_csrf"];
		//				await FileSystemRequests.SaveUserTokenToFileAsync(token);
		//			}

		//			return answerCode;
		//		}
		//		catch (Exception)
		//		{
		//			throw;
		//		}
		//	}

		//	/// <summary>
		//	/// Synchronizing local calendars to server
		//	/// </summary>
		//	/// <returns>Result of operation: 0 - synchronization is success, 1 - synchrozination problems</returns>
		//	public static async Task<int> SynchronizationRequestAsync()
		//	{
		//		int resultOfSynchronization = 1;

		//		string receivedlink = "https://planningway.ru/data/synchronization";
		//		string token = (await FileSystemRequests.LoadUserEmailAndTokenFromFileAsync()).token;

		//		ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
		//		string deviceId = (string)localSettings.Values["DeviceId"];

		//		using (MainDatabaseContext db = new MainDatabaseContext())
		//		{
		//			#region Create anonymous types for sending to server

		//			// is_delete - list of local deleted items
		//			// story_list - list of local elements of the current user and not deleted from the local database

		//			var areas = new
		//			{
		//				is_delete = db.Areas.Where(i => i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"]).Select(i => i.Id).ToList(),
		//				story_list = db.Areas.
		//					Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete).
		//					Select(i => new
		//					{
		//						create_at = i.CreateAt.Value.ToString("yyyy-MM-dd HH:mm:ss"),
		//						id = i.Id,
		//						update_at = i.UpdateAt.Value.ToString("yyyy-MM-dd HH:mm:ss"),
		//						personEmail = i.EmailOfOwner

		//					}).ToList()
		//			};

		//			var projects = new
		//			{
		//				is_delete = db.Projects.Where(i => i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"]).Select(i => i.Id).ToList(),
		//				story_list = db.Projects.
		//					Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete).
		//					Select(i => new
		//					{
		//						create_at = i.CreateAt.Value.ToString("yyyy-MM-dd HH:mm:ss"),
		//						id = i.Id,
		//						update_at = i.UpdateAt.Value.ToString("yyyy-MM-dd HH:mm:ss")

		//					}).ToList()
		//			};

		//			var events = new
		//			{
		//				is_delete = db.MapEvents.Where(i => i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"]).Select(i => i.Id).ToList(),
		//				story_list = db.MapEvents.
		//					Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete).
		//					Select(i => new
		//					{
		//						create_at = i.CreateAt.Value.ToString("yyyy-MM-dd HH:mm:ss"),
		//						id = i.Id,
		//						update_at = i.UpdateAt.Value.ToString("yyyy-MM-dd HH:mm:ss")

		//					}).ToList()
		//			};

		//			#endregion

		//			Debug.WriteLine($"Данные для сверки: {JsonSerialize(new { _csrf = token, idDevice = deviceId, areas, projects, events })}");
		//			var resultOfRequest = await BasePostRequestAsync(receivedlink, JsonSerialize(new { _csrf = token, idDevice = deviceId, areas, projects, events }));

		//			JObject jsonString = JObject.Parse(resultOfRequest);

		//			Debug.WriteLine($"Получаемые данные \n{jsonString}");

		//			#region Areas processing

		//			#region Adding new to database

		//			// Get areas items for adding to local database
		//			IList<Area> receivedAreas = new List<Area>();
		//			foreach (var result in jsonString["areas"]["item"].Children().ToList())
		//			{
		//				var searchResult = result.ToObject<Area>();
		//				receivedAreas.Add(searchResult);
		//			}

		//			// Add received areas to local database
		//			db.Areas.AddRange(receivedAreas);

		//			Debug.WriteLine($"Добавляем: {receivedAreas.Count} зон");

		//			#endregion

		//			#region Remove from database

		//			// Get removed areas, for removing from local database
		//			IList<string> removedAreas = new List<string>();
		//			foreach (var result in jsonString["areas"]["is_delete"].Children().ToList())
		//			{
		//				removedAreas.Add(result.ToObject<string>());
		//			}

		//			// Removing areas from local database
		//			foreach (var removedArea in removedAreas)
		//			{
		//				db.Areas.Remove(db.Areas.FirstOrDefault(i => i.Id == removedArea));
		//			}

		//			Debug.WriteLine($"Удаляем: {removedAreas.Count} зон");

		//			#endregion

		//			#region Seding to server

		//			// Create sending areas
		//			IList<string> sendedAreas = new List<string>();
		//			foreach (var result in jsonString["areas"]["requires"].Children().ToList())
		//			{
		//				sendedAreas.Add(result.ToObject<string>());
		//			}

		//			Debug.WriteLine($"Отправляемых: {sendedAreas.Count} зон");

		//			#endregion

		//			#endregion

		//			#region Projects processing

		//			#region Adding new to database

		//			// Get projects items for adding to local database
		//			IList<Project> receivedProjects = new List<Project>();
		//			foreach (var result in jsonString["projects"]["item"].Children().ToList())
		//			{
		//				var searchResult = result.ToObject<Project>();
		//				receivedProjects.Add(searchResult);
		//			}

		//			// Add received projects to local database
		//			db.Projects.AddRange(receivedProjects);

		//			Debug.WriteLine($"Добавляем: {receivedProjects.Count} проектов");

		//			#endregion

		//			#region Remove from database

		//			// Get removed projects, for removing from local database
		//			IList<string> removedProjects = new List<string>();
		//			foreach (var result in jsonString["projects"]["is_delete"].Children().ToList())
		//			{
		//				removedProjects.Add(result.ToObject<string>());
		//			}

		//			// Removing projects from local database
		//			foreach (var removedProject in removedProjects)
		//			{
		//				db.Projects.Remove(db.Projects.FirstOrDefault(i => i.Id == removedProject));
		//			}

		//			Debug.WriteLine($"Удаляем: {removedProjects.Count} проектов");

		//			#endregion

		//			#region Sending to server

		//			// Create sending projects
		//			IList<string> sendedProjects = new List<string>();
		//			foreach (var result in jsonString["projects"]["requires"].Children().ToList())
		//			{
		//				sendedProjects.Add(result.ToObject<string>());
		//			}

		//			Debug.WriteLine($"Отправляемых: {sendedProjects.Count} проектов");

		//			#endregion

		//			#endregion

		//			#region MapEvents processing

		//			#region Adding to database

		//			// Get map events items for adding to local database
		//			IList<Event> receivedMapEvents = new List<Event>();
		//			foreach (var result in jsonString["events"]["item"].Children().ToList())
		//			{
		//				var searchResult = result.ToObject<Event>();
		//				receivedMapEvents.Add(searchResult);
		//			}

		//			// Add received map events to local database
		//			db.MapEvents.AddRange(receivedMapEvents);

		//			Debug.WriteLine($"Добавляем: {receivedMapEvents.Count} событий");

		//			#endregion

		//			#region Remove from database

		//			// Get removed map events, for removing from local database
		//			IList<string> removedMapEvents = new List<string>();
		//			foreach (var result in jsonString["events"]["is_delete"].Children().ToList())
		//			{
		//				removedMapEvents.Add(result.ToObject<string>());
		//			}

		//			// Removing map events from local database
		//			foreach (var removedMapEvent in removedMapEvents)
		//			{
		//				db.MapEvents.Remove(db.MapEvents.FirstOrDefault(i => i.Id == removedMapEvent));
		//			}

		//			Debug.WriteLine($"Удаляем: {removedMapEvents.Count} событий");

		//			#endregion

		//			#region Sending to server

		//			// Create sending map events
		//			IList<string> sendedMapEvents = new List<string>();
		//			foreach (var result in jsonString["events"]["requires"].Children().ToList())
		//			{
		//				sendedMapEvents.Add(result.ToObject<string>());
		//			}

		//			Debug.WriteLine($"Отправляемых: {sendedMapEvents.Count} событий");

		//			#endregion

		//			#endregion

		//			var sendingData = new
		//			{
		//				areas = db.Areas.Join
		//				(
		//					sendedAreas,
		//					i => i.Id,
		//					w => w,
		//					(i, w) => new
		//					{
		//						id = i.Id,
		//						summary = i.Name,
		//						description = i.Description,
		//						color = i.Color,
		//						favourite = i.Favorite,
		//						update_at = i.UpdateAt.Value.ToString("yyyy-MM-dd HH:mm:ss"),
		//						create_at = i.CreateAt.Value.ToString("yyyy-MM-dd HH:mm:ss"),
		//						personEmail = i.EmailOfOwner
		//					}
		//				),
		//				projects = db.Projects.Join
		//				(
		//					sendedProjects,
		//					i => i.Id,
		//					w => w,
		//					(i, w) => new
		//					{
		//						id = i.Id,
		//						summary = i.Name,
		//						description = i.Description,
		//						color = i.Color,
		//						area_id = i.AreaId,
		//						update_at = i.UpdateAt.Value.ToString("yyyy-MM-dd HH:mm:ss"),
		//						create_at = i.CreateAt.Value.ToString("yyyy-MM-dd HH:mm:ss"),
		//						personEmail = i.EmailOfOwner,
		//						from = i.From
		//					}
		//				),
		//				events = db.MapEvents.Join
		//				(
		//					sendedMapEvents,
		//					i => i.Id,
		//					w => w,
		//					(i, w) => new
		//					{
		//						id = i.Id,
		//						start = i.Start.ToString("yyyy-MM-dd HH:mm:ss"),
		//						end = i.End.ToString("yyyy-MM-dd HH:mm:ss"),
		//						location = i.Location,
		//						summary = i.Name,
		//						description = i.Description,
		//						color = i.Color,
		//						recurrence = i.EventInterval,
		//						project_id = i.ProjectId,
		//						update_at = i.UpdateAt.Value.ToString("yyyy-MM-dd HH:mm:ss"),
		//						create_at = i.CreateAt.Value.ToString("yyyy-MM-dd HH:mm:ss"),
		//						projectPersonEmail = i.ProjectOwnerEmail,
		//						personEmail = i.EmailOfOwner,
		//						isPublic = i.IsPublic,
		//						timeNotification = i.NotificationTime.ToString("yyyy-MM-dd HH:mm:ss"),
		//						people = i.AssociatedPerson
		//					}
		//				)
		//			};

		//			string departureAddress = "https://planningway.ru/data/save";
		//			var completeSendingData = new
		//			{
		//				_csrf = token,
		//				idDevice = deviceId,
		//				areas = sendingData.areas,
		//				projects = sendingData.projects,
		//				events = sendingData.events,
		//			};

		//			Debug.WriteLine($"Отправляем данные:\n{JsonSerialize(completeSendingData)}");

		//			var finalResult = await BasePostRequestAsync(departureAddress, JsonSerialize(completeSendingData));

		//			jsonString = JObject.Parse(finalResult);
		//			resultOfSynchronization = (int)jsonString["answer"];


		//			if (resultOfSynchronization == 0)
		//			{
		//				db.Areas.RemoveRange(db.Areas.Where(i => i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"]));
		//				db.Projects.RemoveRange(db.Projects.Where(i => i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"]));
		//				db.MapEvents.RemoveRange(db.MapEvents.Where(i => i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"]));
		//			}

		//			db.SaveChanges();
		//		}

		//		return resultOfSynchronization;
		//	}

		//	/// <summary>
		//	/// Synchronizing local contacts to server
		//	/// </summary>
		//	/// <returns>Result of operation: 0 - synchronization is success, 1 - synchrozination problems</returns>
		//	public static async Task<int> ContactsSynchronizationRequestAsync()
		//	{
		//		int resultOfSynchronization = 1;

		//		string receivedlink = "https://planningway.ru/contact/synchronization";
		//		string token = (await FileSystemRequests.LoadUserEmailAndTokenFromFileAsync()).token;

		//		ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
		//		string deviceId = (string)localSettings.Values["DeviceId"];

		//		using (MainDatabaseContext db = new MainDatabaseContext())
		//		{
		//			#region Create anonymous types for sending to server

		//			// is_delete - list of local deleted items
		//			// story_list - list of local elements of the current user and not deleted from the local database

		//			var contacts = new
		//			{
		//				is_delete = db.Contacts.Where(i => i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"]).Select(i => i.Id).ToList(),
		//				story_list = db.Contacts.
		//					Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete).
		//					Select(i => new
		//					{
		//						create_at = i.CreateAt.Value.ToString("yyyy-MM-dd HH:mm:ss"),
		//						id = i.Id,
		//						update_at = i.UpdateAt.Value.ToString("yyyy-MM-dd HH:mm:ss"),

		//					}).ToList()
		//			};

		//			#endregion

		//			Debug.WriteLine($"Данные для сверки: {JsonSerialize(new { _csrf = token, idDevice = deviceId, contacts })}");
		//			var resultOfRequest = await BasePostRequestAsync(receivedlink, JsonSerialize(new { _csrf = token, idDevice = deviceId, contacts }));

		//			JObject jsonString = JObject.Parse(resultOfRequest);

		//			Debug.WriteLine($"Получаемые данные \n{jsonString}");

		//			#region Contacts processing

		//			#region Adding new to database

		//			// Get contacts items for adding to local database
		//			IList<Contact> receivedContacts = new List<Contact>();
		//			foreach (var result in jsonString["contacts"]["item"].Children().ToList())
		//			{
		//				var searchResult = result.ToObject<Contact>();
		//				receivedContacts.Add(searchResult);
		//			}

		//			// Add received contacts to local database
		//			db.Contacts.AddRange(receivedContacts);

		//			Debug.WriteLine($"Добавляем: {receivedContacts.Count} контактов");

		//			#endregion

		//			#region Remove from database

		//			// Get removed contacts, for removing from local database
		//			IList<string> removedContacts = new List<string>();
		//			foreach (var result in jsonString["contacts"]["is_delete"].Children().ToList())
		//			{
		//				removedContacts.Add(result.ToObject<string>());
		//			}

		//			// Removing contacts from local database
		//			foreach (var removedContact in removedContacts)
		//			{
		//				db.Contacts.Remove(db.Contacts.FirstOrDefault(i => i.Id == removedContact));
		//			}

		//			Debug.WriteLine($"Удаляем: {removedContacts.Count} контактов");

		//			#endregion

		//			#region Seding to server

		//			// Create sending contacts
		//			IList<string> sendedContacts = new List<string>();
		//			foreach (var result in jsonString["contacts"]["requires"].Children().ToList())
		//			{
		//				sendedContacts.Add(result.ToObject<string>());
		//			}

		//			Debug.WriteLine($"Отправляемых: {sendedContacts.Count} контактов");

		//			#endregion

		//			#endregion

		//			var sendingData = new
		//			{
		//				contacts = db.Contacts.Join
		//				(
		//					sendedContacts,
		//					i => i.Id,
		//					w => w,
		//					(i, w) => new
		//					{
		//						id = i.Id,
		//						email = i.Email,
		//						name = i.Name,
		//						update_at = i.UpdateAt.Value.ToString("yyyy-MM-dd HH:mm:ss"),
		//						create_at = i.CreateAt.Value.ToString("yyyy-MM-dd HH:mm:ss"),
		//						personEmail = i.EmailOfOwner
		//					}
		//				)
		//			};

		//			string departureAddress = "https://planningway.ru/contact/save";
		//			var completeSendingData = new
		//			{
		//				_csrf = token,
		//				idDevice = deviceId,
		//				contacts = sendingData.contacts
		//			};

		//			Debug.WriteLine($"Отправляем данные:\n{JsonSerialize(completeSendingData)}");

		//			var finalResult = await BasePostRequestAsync(departureAddress, JsonSerialize(completeSendingData));

		//			jsonString = JObject.Parse(finalResult);
		//			resultOfSynchronization = (int)jsonString["answer"];

		//			if (resultOfSynchronization == 0)
		//			{
		//				db.Contacts.RemoveRange(db.Contacts.Where(i => i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"]));
		//			}

		//			db.SaveChanges();
		//		}

		//		return resultOfSynchronization;
		//	}

		//	/// <summary>
		//	/// Get public map events from added contacts
		//	/// </summary>
		//	/// <returns>Result of getting</returns>
		//	public static async Task<(int operationResult, List<Project> publicProjects, List<Event> publicEvents)> GetPublicMapEventsAsync()
		//	{
		//		int resultOfSynchronization = 1;

		//		string receivedlink = "https://planningway.ru/data/get-public";
		//		string token = (await FileSystemRequests.LoadUserEmailAndTokenFromFileAsync()).token;

		//		ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
		//		string deviceId = (string)localSettings.Values["DeviceId"];

		//		Debug.WriteLine($"Данные для сверки: {JsonSerialize(new { _csrf = token, idDevice = deviceId })}");
		//		var resultOfRequest = await BasePostRequestAsync(receivedlink, JsonSerialize(new { _csrf = token, idDevice = deviceId }));

		//		JObject jsonString = JObject.Parse(resultOfRequest);
		//		Debug.WriteLine($"Получаемые данные \n{jsonString}");

		//		resultOfSynchronization = (int)jsonString["answer"];

		//		if (resultOfSynchronization != 0)
		//		{
		//			return (resultOfSynchronization, null, null);
		//		}

		//		List<Project> receivedProjects = new List<Project>();
		//		foreach (var result in jsonString["projects"].Children().ToList())
		//		{
		//			var searchResult = result.ToObject<Project>();
		//			searchResult.CreateAt = searchResult.UpdateAt = DateTime.UtcNow;

		//			receivedProjects.Add(searchResult);
		//		}

		//		List<Event> receivedMapEvents = new List<Event>();
		//		foreach (var result in jsonString["events"].Children().ToList())
		//		{
		//			var searchResult = result.ToObject<Event>();
		//			searchResult.ProjectOwnerEmail = (string)localSettings.Values["email"];
		//			searchResult.CreateAt = searchResult.UpdateAt = DateTime.UtcNow;

		//			receivedMapEvents.Add(searchResult);
		//		}

		//		return (resultOfSynchronization, receivedProjects, receivedMapEvents);
		//	}

		//	/// <summary>
		//	/// Sending invitation to contacts for project
		//	/// </summary>
		//	/// <param name="contacts">List of contacts for sending invitation</param>
		//	/// <param name="project">Shared project</param>
		//	/// <returns>0 - success, 1 - operation error</returns>
		//	public static async Task<int> SendInvitationToContact(IList<string> contacts, Project project)
		//	{
		//		int resultOfSynchronization = 1;
		//		if (contacts.Count < 1 || project == null)
		//		{
		//			return resultOfSynchronization;
		//		}

		//		string receivedlink = "https://planningway.ru/invitation/invite";
		//		string token = (await FileSystemRequests.LoadUserEmailAndTokenFromFileAsync()).token;

		//		ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
		//		string deviceId = (string)localSettings.Values["DeviceId"];

		//		List<Invitation> invitations = new List<Invitation>(contacts.Count);
		//		foreach (var item in contacts)
		//		{
		//			invitations.Add(new Invitation(item, project.Id));
		//		}

		//		Debug.WriteLine($"Данные для сверки: {JsonSerialize(new { _csrf = token, idDevice = deviceId, invitations })}");
		//		var resultOfRequest = await BasePostRequestAsync(receivedlink, JsonSerialize(new { _csrf = token, idDevice = deviceId, invitations }));

		//		JObject jsonString = JObject.Parse(resultOfRequest);
		//		Debug.WriteLine($"Получаемые данные \n{jsonString}");

		//		resultOfSynchronization = (int)jsonString["answer"];

		//		return resultOfSynchronization;
		//	}

		//	/// <summary>
		//	/// Getting invitations send by me to my contacts
		//	/// </summary>
		//	/// <returns></returns>
		//	public static async Task<List<Invitation>> GetMyInvitations()
		//	{
		//		List<Invitation> invites = new List<Invitation>();

		//		string receivedlink = "https://planningway.ru/invitation/get-my-invitation";
		//		string token = (await FileSystemRequests.LoadUserEmailAndTokenFromFileAsync()).token;

		//		ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
		//		string deviceId = (string)localSettings.Values["DeviceId"];

		//		Debug.WriteLine($"Данные для сверки: {JsonSerialize(new { _csrf = token, idDevice = deviceId})}");
		//		var resultOfRequest = await BasePostRequestAsync(receivedlink, JsonSerialize(new { _csrf = token, idDevice = deviceId }));

		//		JObject jsonString = JObject.Parse(resultOfRequest);
		//		Debug.WriteLine($"Получаемые данные \n{jsonString}");

		//		int resultOfSynchronization = (int)jsonString["answer"];

		//		if (resultOfSynchronization != 0)
		//		{
		//			return invites;
		//		}

		//		foreach (var result in jsonString["invitations"].Children().ToList())
		//		{
		//			invites.Add(result.ToObject<Invitation>());
		//		}

		//		return invites;
		//	}

		//	/// <summary>
		//	/// Getting invitations for me
		//	/// </summary>
		//	/// <returns></returns>
		//	public static async Task<(List<Invitation> invitations, List<Project> projects)> GetInvitationsForMe()
		//	{
		//		string receivedlink = "https://planningway.ru/invitation/get-invitation";
		//		string token = (await FileSystemRequests.LoadUserEmailAndTokenFromFileAsync()).token;

		//		ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
		//		string deviceId = (string)localSettings.Values["DeviceId"];

		//		Debug.WriteLine($"Данные для сверки: {JsonSerialize(new { _csrf = token, idDevice = deviceId })}");
		//		var resultOfRequest = await BasePostRequestAsync(receivedlink, JsonSerialize(new { _csrf = token, idDevice = deviceId }));

		//		JObject jsonString = JObject.Parse(resultOfRequest);
		//		Debug.WriteLine($"Получаемые данные \n{jsonString}");

		//		int resultOfSynchronization = (int)jsonString["answer"];

		//		List<Invitation> invitations = new List<Invitation>();
		//		List<Project> projects = new List<Project>();

		//		if (resultOfSynchronization != 0)
		//		{
		//			return (invitations, projects);
		//		}

		//		foreach (var result in jsonString["invitations"].Children().ToList())
		//		{
		//			invitations.Add(result.ToObject<Invitation>());
		//		}

		//		foreach (var result in jsonString["projects"].Children().ToList())
		//		{
		//			projects.Add(result.ToObject<Project>());
		//		}

		//		return (invitations, projects);
		//	}

		//	/// <summary>
		//	/// Accepting of contact project
		//	/// </summary>
		//	/// <param name="projectId">Id of accepted project</param>
		//	/// <returns></returns>
		//	public static async Task<(Project project, List<Event> events)> AcceptInvite(string projectId)
		//	{
		//		Project project = new Project();
		//		List<Event> events = new List<Event>(); 

		//		if (string.IsNullOrEmpty(projectId))
		//		{
		//			return (project, events);
		//		}

		//		string receivedlink = "https://planningway.ru/invitation/accept";
		//		string token = (await FileSystemRequests.LoadUserEmailAndTokenFromFileAsync()).token;

		//		ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
		//		string deviceId = (string)localSettings.Values["DeviceId"];

		//		Debug.WriteLine($"Данные для сверки: {JsonSerialize(new { _csrf = token, idDevice = deviceId, projectId })}");
		//		var resultOfRequest = await BasePostRequestAsync(receivedlink, JsonSerialize(new { _csrf = token, idDevice = deviceId, projectId }));

		//		JObject jsonString = JObject.Parse(resultOfRequest);
		//		Debug.WriteLine($"Получаемые данные \n{jsonString}");

		//		int resultOfSynchronization = (int)jsonString["answer"];

		//		if (resultOfSynchronization != 0)
		//		{
		//			return (project, events);
		//		}

		//		project = jsonString["project"].ToObject<Project>();

		//		foreach (var result in jsonString["events"].Children().ToList())
		//		{
		//			var temp = result.ToObject<Event>();
		//			temp.UpdateAt = temp.CreateAt = DateTime.UtcNow;
		//			temp.EmailOfOwner = (string)localSettings.Values["email"];

		//			events.Add(temp);
		//		}

		//		return (project, events);
		//	}

		//	/// <summary>
		//	/// Deny invitation for project
		//	/// </summary>
		//	/// <param name="projectId">Id of denied project</param>
		//	/// <returns>0 - success, 1 - error</returns>
		//	public static async Task<int> DenyInvite(string projectId)
		//	{
		//		int resultOfSynchronization = 1;

		//		if (string.IsNullOrEmpty(projectId))
		//		{
		//			return resultOfSynchronization;
		//		}

		//		string receivedlink = "https://planningway.ru/invitation/deny";
		//		string token = (await FileSystemRequests.LoadUserEmailAndTokenFromFileAsync()).token;

		//		ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
		//		string deviceId = (string)localSettings.Values["DeviceId"];

		//		Debug.WriteLine($"Данные для сверки: {JsonSerialize(new { _csrf = token, idDevice = deviceId, projectId })}");
		//		var resultOfRequest = await BasePostRequestAsync(receivedlink, JsonSerialize(new { _csrf = token, idDevice = deviceId, projectId }));

		//		JObject jsonString = JObject.Parse(resultOfRequest);
		//		Debug.WriteLine($"Получаемые данные \n{jsonString}");

		//		resultOfSynchronization = (int)jsonString["answer"];

		//		return resultOfSynchronization;
		//	}

		//	/// <summary>
		//	/// Cancel invitations for my contacts
		//	/// </summary>
		//	/// <param name="ids">Id's of unsubscribed invites</param>
		//	/// <returns>0 - success, 1 - error</returns>
		//	public static async Task<int> UnsubscribeInvitations(string id)
		//	{
		//		int resultOfSynchronization = 1;

		//		if (string.IsNullOrEmpty(id))
		//		{
		//			return resultOfSynchronization;
		//		}

		//		string receivedlink = "https://planningway.ru/invitation/delete-invitation";
		//		string token = (await FileSystemRequests.LoadUserEmailAndTokenFromFileAsync()).token;

		//		ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
		//		string deviceId = (string)localSettings.Values["DeviceId"];

		//		Debug.WriteLine($"Данные для сверки: {JsonSerialize(new { _csrf = token, idDevice = deviceId, isDelete = id })}");
		//		var resultOfRequest = await BasePostRequestAsync(receivedlink, JsonSerialize(new { _csrf = token, idDevice = deviceId, isDelete = id }));

		//		JObject jsonString = JObject.Parse(resultOfRequest);
		//		Debug.WriteLine($"Получаемые данные \n{jsonString}");

		//		resultOfSynchronization = (int)jsonString["answer"];

		//		return resultOfSynchronization;
		//	}

		//	/// <summary>
		//	/// Check Internet connection
		//	/// </summary>
		//	/// <returns>Return true, if connected</returns>
		//	public static bool CheckForInternetConnection()
		//	{
		//		try
		//		{
		//			using (var webClient = new WebClient())
		//			{
		//				webClient.Proxy = null;

		//				using (webClient.OpenRead("http://clients3.google.com/generate_204"))
		//				{
		//					return true;
		//				}
		//			}
		//		}
		//		catch (WebException)
		//		{
		//			return false;
		//		}

		//		catch (Exception)
		//		{
		//			return false;
		//		}
		//	}

		#region JSON Converting

		/// <summary>
		/// Serialize template object to json format
		/// </summary>
		/// <param name="obj">Serialized object</param>
		/// <returns>Json string</returns>
		public static async Task<string> JsonSerializeAsync<T>(T obj)
		{
			if (obj != null)
			{
				DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(T));

				using (MemoryStream ms = new MemoryStream())
				{
					jsonSerializer.WriteObject(ms, obj);

					ms.Position = 0;

					using (StreamReader sr = new StreamReader(ms))
					{
						return await sr.ReadLineAsync();
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Serialize token and email
		/// </summary>
		/// <param name="email">Serializable email</param>
		/// <param name="token">Token</param>
		/// <returns>Строка в формате Json</returns>
		public static string TokenJsonSerialize(string email, string token)
		{
			if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(token))
			{
				var res = new { email, _csrf = token };
				//return JsonConvert.SerializeObject(res);
			}

			return null;
		}

		#endregion
	}
}