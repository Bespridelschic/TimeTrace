using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using Windows.UI.Popups;

namespace TimeTrace.Model
{
	public class User : INotifyPropertyChanged
	{
		#region Свойства

		private string email;
		public string Email
		{
			get { return email; }
			set
			{
				email = value;
				OnPropertyChanged("Email");
			}
		}

		private string password;
		public string Password
		{
			get { return password; }
			set
			{
				password = value;
				OnPropertyChanged("Password");
			}
		}

		private string lastName;
		public string LastName
		{
			get { return lastName; }
			set
			{
				lastName = value;
				OnPropertyChanged("LastName");
			}
		}

		private string firstName;
		public string FirstName
		{
			get { return firstName; }
			set
			{
				firstName = value;
				OnPropertyChanged("FirstName");
			}
		}

		private string middleName;
		public string MiddleName
		{
			get { return middleName; }
			set
			{
				middleName = value;
				OnPropertyChanged("MiddleName");
			}
		}

		private string birthday;
		public string Birthday
		{
			get { return birthday; }
			set
			{
				birthday = value;
				OnPropertyChanged("Birthday");
			}
		}

		#endregion

		#region Проверка корректности данных

		/// <summary>
		/// Проверка адреса электронной почты на корректность записи
		/// </summary>
		/// <returns></returns>
		public bool EmailCorrectChech()
		{
			string pattern = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
			var res = Regex.Match(Email, pattern);

			if (res.Success)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Уровни сложности пароля
		/// </summary>
		public enum PasswordScore
		{
			Blank = 0,
			VeryWeak = 1,
			Weak = 2,
			Medium = 3,
			Strong = 4,
			VeryStrong = 5
		}

		/// <summary>
		/// Проверка уровня сложности пароля
		/// </summary>
		/// <returns>Уровень сложности пароля</returns>
		public PasswordScore PasswordSecurityCheck()
		{
			int score = 1;

			if (Password.Length < 1)
				return PasswordScore.Blank;
			if (Password.Length < 4)
				return PasswordScore.VeryWeak;

			if (Password.Length >= 8)
				score++;
			if (Password.Length >= 12)
				score++;
			if (Regex.Match(Password, @"/\d+/", RegexOptions.ECMAScript).Success)
				score++;
			if (Regex.Match(Password, @"/[a-z]/", RegexOptions.ECMAScript).Success &&
				Regex.Match(Password, @"/[A-Z]/", RegexOptions.ECMAScript).Success)
				score++;
			if (Regex.Match(Password, @"/.[!,@,#,$,%,^,&,*,?,_,~,-,£,(,)]/", RegexOptions.ECMAScript).Success)
				score++;

			return (PasswordScore)score;
		}

		#endregion

		#region Конструкторы

		public User()
		{
			Email = string.Empty;
			Password = string.Empty;

			LastName = string.Empty;
			FirstName = string.Empty;
			MiddleName = string.Empty;
			Birthday = string.Empty;
		}

		/// <summary>
		/// Инициализация объекта класса <seealso cref="User"/>
		/// </summary>
		/// <param name="email">Адрес электронной почты</param>
		/// <param name="password">Пароль</param>
		/// <param name="firstName">Имя</param>
		/// <param name="lastName">Фамилия</param>
		/// <param name="middleName">Отчество</param>
		/// <param name="birthday">Дата рождения</param>
		public User(string email, string password, string firstName = "", string lastName = "", string middleName = "", string birthday = "")
		{
			Email = email;
			Password = password;

			LastName = lastName;
			FirstName = firstName;
			MiddleName = middleName;
			Birthday = birthday;
		}

		#endregion

		#region JSON

		/// <summary>
		/// Сериализация объекта User в формат Json при входе в систему
		/// </summary>
		/// <returns>Строка в формате Json</returns>
		private string JsonSignInSerialize()
		{
			var res = new { email = Email, password = Password };
			return JsonConvert.SerializeObject(res);
		}

		/// <summary>
		/// Сериализация объекта User в формат Json при регистрации
		/// </summary>
		/// <returns>Строка в формате Json</returns>
		private string SignUpJsonSerialize()
		{
			var res = new { email = Email, password = Password, firstName = FirstName, lastName = LastName, middleName = MiddleName, birthday = Birthday };
			return JsonConvert.SerializeObject(res);
		}

		/// <summary>
		/// Десериализация объекта User в формате Json
		/// </summary>
		/// <param name="jsonString">Строка в формате Json</param>
		/// <returns>Десериализированный объект User</returns>
		private User JsonDeserialize(string jsonString)
		{
			User user = JsonConvert.DeserializeObject<User>(jsonString);
			return user;
		}

		/// <summary>
		/// Сериализация Email и активационного кода для активации аккаунта
		/// </summary>
		/// <param name="activationCode">Код активации</param>
		/// <returns>Строка в формате Json</returns>
		private string JsonAccountActivationSerialize(string activationCode)
		{
			var res = new { email = Email, SecretKey = activationCode };
			return JsonConvert.SerializeObject(res);
		}

		/// <summary>
		/// Сериализация Email и токена для входа в систему
		/// </summary>
		/// <param name="token">Значение токена</param>
		/// <returns></returns>
		private string JsonSignInWithTokenSerialize(string token)
		{
			var res = new { email = Email, token = token };
			return JsonConvert.SerializeObject(res);
		}

		// TODO: Десериализация токена

		#endregion

		/// <summary>
		/// Хэширование пароля алгоритмом SHA512 UTF8-UTF8
		/// </summary>
		/// <returns>Хэшированный пароль</returns>
		public string GetHashEncrypt()
		{
			SHA512 hash = SHA512.Create();
			var result = hash.ComputeHash(Encoding.UTF8.GetBytes(password));
			return Encoding.UTF8.GetString(result);
		}

		#region Работа с файлом

		/// <summary>
		/// Сохранение данных пользователя в файл
		/// </summary>
		public async Task SaveUserToFile()
		{
			StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
			StorageFile storageFile = await storageFolder.CreateFileAsync("PSF.bin", CreationCollisionOption.ReplaceExisting);

			// Расположение файла C:\Users\Bespridelschic\AppData\Local\Packages\c72abfd6-f805-4cdb-8b03-89abadbe4aec_4a9rgd3a66dme\LocalState

			string[] stringToFile = { Email, Password };

			await FileIO.WriteLinesAsync(storageFile, stringToFile);

			//string result = string.Empty;

			/*using (Aes crypto = Aes.Create())
			{
				crypto.BlockSize = 128;
				crypto.KeySize = 256;
				crypto.GenerateIV();
				crypto.GenerateKey();
				crypto.Mode = CipherMode.CBC;
				crypto.Padding = PaddingMode.PKCS7;

				ICryptoTransform cryptoTransform = crypto.CreateEncryptor();
				byte[] passwordBytes = cryptoTransform.TransformFinalBlock(Encoding.UTF8.GetBytes(Password), 0, Password.Length);

				passwordCrypted = result;
				return result = Encoding.UTF8.GetString(passwordBytes);
			}*/
		}

		/// <summary>
		/// Получение пароля из локального хранилища
		/// </summary>
		/// <returns></returns>
		public async Task LoadUserFromFile()
		{
			try
			{
				StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
				StorageFile storageFile = await storageFolder.GetFileAsync("PSF.bin");

				// Расположение файла C:\Users\Bespridelschic\AppData\Local\Packages\c72abfd6-f805-4cdb-8b03-89abadbe4aec_4a9rgd3a66dme\LocalState

				var fileLines = await (FileIO.ReadLinesAsync(storageFile));

				if (fileLines.Count <= 2)
				{
					Email = fileLines[0];
				}
				if (fileLines.Count == 2)
				{
					Password = fileLines[1];
				}
			}
			catch (Exception)
			{

			}

			/*string result = string.Empty;

			using (Aes crypto = Aes.Create())
			{
				crypto.BlockSize = 128;
				crypto.KeySize = 256;
				crypto.GenerateIV();
				crypto.GenerateKey();
				crypto.Mode = CipherMode.CBC;
				crypto.Padding = PaddingMode.PKCS7;

				ICryptoTransform cryptoTransform = crypto.CreateDecryptor();

				//byte[] encryptedBytes = Encoding.UTF8.GetBytes(passwordCrypted);
				//byte[] passwordBytes = cryptoTransform.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

			}*/
		}

		#endregion

		#region Работа с сервером

		/// <summary>
		/// Отправка данные на сервер для регистрации
		/// </summary>
		/// <returns>Код регистрации: 0 - Успех, 1 - Не удача, -1 - Не предвиденная ошибка</returns>
		public async Task<int> SignUpPostRequestAsync()
		{
			try
			{
				WebRequest request = WebRequest.Create("http://o129pak8.beget.tech/customer/signup");
				request.Method = "POST";

				string strDate = $"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}";
				User user = new User("mail2@mail.ru", "1234567890", "firstName", "lastName", "MiddleName", strDate);

				string data = user.SignUpJsonSerialize();
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

				Int32.TryParse(result, out int codeResult);
				return codeResult;
			}
			catch (Exception ex)
			{
				return -1;
			}
		}

		/// <summary>
		/// Отправка данных на сервер для входа
		/// </summary>
		/// <returns>Код регистрации: 0 - Успех, 1 - Не удача, -1 - Не предвиденная ошибка</returns>
		public async Task<int> SignInPostRequestAsync()
		{
			try
			{
				WebRequest request = WebRequest.Create("http://o129pak8.beget.tech/customer/login");
				request.Method = "POST";

				string data = this.JsonSignInSerialize();
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

				JsonTextReader jsonReader = new JsonTextReader(new StringReader(result));

				for (int i = 0; jsonReader.Read() && i < 2; i++)
				{
					if (i ==0 && jsonReader != null)
					{
						await (new MessageDialog($"{jsonReader.Value}")).ShowAsync();
						return Int32.Parse((string)jsonReader.Value);
					}
				}

				return -1;
			}
			catch (Exception)
			{
				return -1;
			}
		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			return $"{Email}: {Password}";
		}

		#endregion

		#region MVVM

		public event PropertyChangedEventHandler PropertyChanged = delegate { };
		private void OnPropertyChanged([CallerMemberName]string prop = "")
		{
			PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}

		#endregion
	}
}