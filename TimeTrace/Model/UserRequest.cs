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
	/// Статический класс работы пользователя с сервером
	/// </summary>
	public static class UserRequest
	{
		/// <summary>
		/// Базовый метод отправки запроса серверу
		/// </summary>
		/// <param name="url">URL строка сервера</param>
		/// <param name="data">Данные отправляемые серверу в формате Json</param>
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

				// устанавливаем тип содержимого - параметр ContentType
				request.ContentType = "application/json";

				// Устанавливаем заголовок Content-Length запроса - свойство ContentLength
				request.ContentLength = byteArray.Length;

				//записываем данные в поток запроса
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
		/// Отправка данные на сервер для регистрации
		/// </summary>
		/// <param name="user">Объект класса <see cref="User"/></param>
		/// <returns>Код регистрации: 0 - Успех, 1 - Не удача</returns>
		public static async Task<int> SignUpPostRequestAsync(User user)
		{
			try
			{
				string result = await BasePostRequestAsync("https://mindstructuring.ru/customer/signup", SignUpJsonSerialize(user));

				if (string.IsNullOrEmpty(result))
				{
					throw new NullReferenceException("Сервер вернул null");
				}

				// Парсер JSON
				JObject JsonString = JObject.Parse(result);

				return (int)JsonString["answer"];
			}
			catch (Exception)
			{
				throw;
			}
		}

		/// <summary>
		/// Активация аккаунта
		/// </summary>
		/// <param name="user">Объект класса <see cref="User"/></param>
		/// <returns>Статус успеха активации: 0 - Успех, 1 - Не удача</returns>
		public static async Task<int> AccountActivationPostRequestAsync(User user)
		{
			try
			{
				string result = await BasePostRequestAsync("https://mindstructuring.ru/customer/sendactivationkey", AccountActivationJsonSerialize(user));

				if (string.IsNullOrEmpty(result))
				{
					throw new NullReferenceException("Сервер вернул null");
				}

				// Парсер JSON
				JObject JsonString = JObject.Parse(result);

				return (int)JsonString["answer"];
			}
			catch (Exception)
			{
				throw;
			}
		}

		/// <summary>
		/// Отправка данных на сервер для входа
		/// </summary>
		/// <param name="user">Объект класса <see cref="User"/></param>
		/// <returns>Код входа: 0 - Успех, 1 - Не удача</returns>
		public static async Task<int> SignInPostRequestAsync(User user)
		{
			try
			{
				string result = await BasePostRequestAsync("https://mindstructuring.ru/customer/login", SignInJsonSerialize(user));

				if (string.IsNullOrEmpty(result))
				{
					throw new NullReferenceException("Сервер вернул null");
				}

				// Парсер JSON
				JObject JsonString = JObject.Parse(result);

				int answerCode = (int)JsonString["answer"];
				if (answerCode == 0)
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
		/// Отправка данных на сервер для входа с помощью токена
		/// </summary>
		/// <param name="email">Строка с адресом электронной почты</param>
		/// <param name="token">Строка с токеном</param>
		/// <returns>Код входа: 0 - Успех, 1 - Не удача</returns>
		public static async Task<int> SignInWithTokenPostRequestAsync(string email, string token)
		{
			try
			{
				string result = await BasePostRequestAsync("https://mindstructuring.ru/customer/login", SignInWithTokenJsonSerialize(email, token));

				if (string.IsNullOrEmpty(result))
				{
					throw new NullReferenceException("Сервер вернул null");
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

		/// <summary>
		/// Отправка данные на сервер для восстановления пароля
		/// </summary>
		/// <param name="user">Объект класса <see cref="User"/></param>
		/// <returns>Код регистрации: 0 - Успех, 1 - Не удача</returns>
		public static async Task<int> PasswordResetPostRequestAsync(User user)
		{
			try
			{
				string result = await BasePostRequestAsync("https://mindstructuring.ru/customer/sendresetkey", SignInJsonSerialize(user));

				if (string.IsNullOrEmpty(result))
				{
					throw new NullReferenceException("Сервер вернул null");
				}

				// Парсер JSON
				JObject JsonString = JObject.Parse(result);

				return (int)JsonString["answer"];
			}
			catch (Exception)
			{
				throw;
			}
		}

		#region JSON

		/// <summary>
		/// Сериализация объекта User в формат Json при входе в систему
		/// </summary>
		/// <param name="user">Объекта класса <see cref="User"/></param>
		/// <returns>Строка в формате Json</returns>
		private static string SignInJsonSerialize(User user)
		{
			if (user != null)
			{
				var res = new { email = user.Email, password = user.Password };
				return JsonConvert.SerializeObject(res);
			}
			return null;
		}

		/// <summary>
		/// Сериализация объекта User в формат Json при регистрации
		/// </summary>
		/// <param name="user">Объекта класса <see cref="User"/></param>
		/// <returns>Строка в формате Json</returns>
		private static string SignUpJsonSerialize(User user)
		{
			if (user != null)
			{
				var res = new { email = user.Email, password = user.Password, firstName = user.FirstName, lastName = user.LastName, middleName = user.MiddleName, birthday = user.Birthday };
				return JsonConvert.SerializeObject(res);
			}
			return null;
		}

		/// <summary>
		/// Десериализация объекта User в формате Json
		/// </summary>
		/// <param name="user">Строка в формате Json</param>
		/// <returns>Объекта класса <see cref="User"/></returns>
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
		/// Сериализация Email для инициирования активации аккаунта
		/// </summary>
		/// <param name="user">Объекта класса <see cref="User"/></param>
		/// <returns>Строка в формате Json</returns>
		private static string AccountActivationJsonSerialize(User user)
		{
			if (user != null)
			{
				var res = new { email = user.Email };
				return JsonConvert.SerializeObject(res);
			}
			return null;
		}

		/// <summary>
		/// Сериализация Email и токена для входа в систему
		/// </summary>
		/// <param name="user">Объекта класса <see cref="User"/></param>
		/// <param name="token">Строка с токеном</param>
		/// <returns>Строка в формате Json</returns>
		private static string SignInWithTokenJsonSerialize(string email, string token)
		{
			if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(token))
			{
				var res = new { email = email, token = token };
				return JsonConvert.SerializeObject(res);
			}
			return null;
		}

		#endregion
	}
}