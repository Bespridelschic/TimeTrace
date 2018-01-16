using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrace.Model
{
	/// <summary>
	/// Static class to work with the server
	/// </summary>
	public static class UserRequests
	{
		/// <summary>
		/// Base method for sending POST requests
		/// </summary>
		/// <param name="url">URL</param>
		/// <param name="data">Data in json format</param>
		/// <returns></returns>
		private static async Task<string> BasePostRequestAsync(string url, string data)
		{
			if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(data))
			{
				return null;
			}
			
			try
			{
				WebRequest request = WebRequest.Create(url);
				request.Method = "POST";

				byte[] byteArray = Encoding.UTF8.GetBytes(data);

				request.ContentType = "application/json";

				// Set header
				request.ContentLength = byteArray.Length;

				// Write data to stream
				using (Stream dataStream = request.GetRequestStream())
				{
					dataStream.Write(byteArray, 0, byteArray.Length);
				}

				string result = string.Empty;

				WebResponse response = await request.GetResponseAsync();
				using (Stream stream = response.GetResponseStream())
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						result += reader.ReadToEnd();
					}
				}
				response.Close();
				
				return result;
			}
			catch (Exception)
			{
				throw;
			}
		}

		/// <summary>
		/// Type of sending request
		/// </summary>
		public enum PostRequestDestination
		{
			SignIn,
			SignUp,
			AccountActivation,
			PasswordReset
		}

		/// <summary>
		/// Sending data to server
		/// </summary>
		/// <param name="user">Object of <see cref="User"/></param>
		/// <returns>Return code from server: 0 - success, 1 - fail</returns>
		public static async Task<int> PostRequestAsync(PostRequestDestination destination, User user)
		{
			string link = string.Empty;

			switch (destination)
			{
				case PostRequestDestination.SignIn:
					link = "https://mindstructuring.ru/customer/login";
					break;
				case PostRequestDestination.SignUp:
					link = "https://mindstructuring.ru/customer/signup";
					break;
				case PostRequestDestination.AccountActivation:
					link = "https://mindstructuring.ru/customer/sendactivationkey";
					break;
				case PostRequestDestination.PasswordReset:
					link = "https://mindstructuring.ru/customer/sendresetkey";
					break;
				default:
					throw new ArgumentException("Not fount type of sending request PostRequestDestination");
			}

			try
			{
				string result = await BasePostRequestAsync(link, JsonSerialize(user));

				if (string.IsNullOrEmpty(result))
				{
					throw new NullReferenceException("Сервер вернул null");
				}

				// Парсер JSON
				JObject JsonString = JObject.Parse(result);

				int answerCode = (int)JsonString["answer"];
				if (destination == PostRequestDestination.SignIn && answerCode == 0)
				{
					string token = (string)JsonString["_csrf"];
					await UserFileWorker.SaveUserTokenToFileAsync(token);
				}

				return answerCode;
			}
			catch (Exception)
			{
				throw;
			}
		}

		/// <summary>
		/// Sending data for enter with token and email
		/// </summary>
		/// <param name="email">Email</param>
		/// <param name="token">Token</param>
		/// <returns>Return code from server: 0 - success, 1 - fail</returns>
		public static async Task<int> SignInWithTokenPostRequestAsync(string email, string token)
		{
			try
			{
				string result = await BasePostRequestAsync("https://mindstructuring.ru/customer/login", SignInWithTokenJsonSerialize(email, token));

				if (string.IsNullOrEmpty(result))
				{
					throw new NullReferenceException("Server return null");
				}

				// Парсер JSON
				JObject JsonString = JObject.Parse(result);

				int answerCode = (int)JsonString["answer"];
				if (answerCode == 0)
				{
					string newToken = (string)JsonString["_csrf"];
					await UserFileWorker.SaveUserTokenToFileAsync(newToken);
				}

				return answerCode;
			}
			catch (Exception)
			{
				throw;
			}
		}

		#region JSON

		/// <summary>
		/// Serialize user to json format
		/// </summary>
		/// <param name="user">Object of <see cref="User"/></param>
		/// <returns>Json string</returns>
		private static string JsonSerialize(User user)
		{
			if (user != null)
			{
				return JsonConvert.SerializeObject(user);
			}
			return null;
		}

		/// <summary>
		/// Deserialize user from json
		/// </summary>
		/// <param name="user">Json string</param>
		/// <returns>New object of <see cref="User"/></returns>
		private static User JsonDeserialize(string jsonString)
		{
			if (!string.IsNullOrEmpty(jsonString))
			{
				User user = JsonConvert.DeserializeObject<User>(jsonString);
				return user;
			}
			return null;
		}

		/// <summary>
		/// Serialize token and email
		/// </summary>
		/// <param name="user">Object of <see cref="User"/></param>
		/// <param name="token">Token</param>
		/// <returns>Строка в формате Json</returns>
		private static string SignInWithTokenJsonSerialize(string email, string token)
		{
			if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(token))
			{
				var res = new { email = email, _csrf = token };
				return JsonConvert.SerializeObject(res);
			}
			return null;
		}

		#endregion
	}
}