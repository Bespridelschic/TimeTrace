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

namespace TimeTrace.Model
{
	public class User
	{
		#region Свойства		
		private string email;
		public string Email
		{
			get { return email; }
			set
			{
				string pattern = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
				var res = Regex.Match(value, pattern);

				if (res.Success)
				{
					email = value;
				}
				else
				{
					email = "BadEmail";
				}
			}
		}

		private string password;
		public string Password
		{
			get { return password; }
			set
			{
				password = value;
			}
		}
		#endregion

		#region Конструкторы
		public User()
		{

		}

		public User(string email, string password)
		{
			Email = email;
			Password = password;
		}
		#endregion

		#region JSON
		/// <summary>
		/// Сериализация объекта User в формат Json
		/// </summary>
		/// <returns>Строка в формате Json</returns>
		public string JsonSerialize()
		{
			var res = new { email = email, password = password };
			return JsonConvert.SerializeObject(res);
		}

		/// <summary>
		/// Десериализация объекта User в формате Json
		/// </summary>
		/// <param name="jsonString">Строка в формате Json</param>
		/// <returns></returns>
		public User JsonDeserialize(string jsonString)
		{
			User user = JsonConvert.DeserializeObject<User>(jsonString);
			return user;
		}
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
		public async Task<string> SaveUserToFile()
		{
			StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
			StorageFile storageFile = await storageFolder.CreateFileAsync("PSF.bin", CreationCollisionOption.ReplaceExisting);

			string result = string.Empty;

			using (Aes crypto = Aes.Create())
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
			}

			await FileIO.WriteTextAsync(storageFile, result);
			return result;
		}
		string passwordCrypted;
		/// <summary>
		/// Получение пароля из локального хранилища
		/// </summary>
		/// <returns></returns>
		public async Task<string> LoadUserFromFile()
		{
			StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
			StorageFile storageFile = await storageFolder.GetFileAsync("PSF.bin");

			string result = string.Empty;

			using (Aes crypto = Aes.Create())
			{
				crypto.BlockSize = 128;
				crypto.KeySize = 256;
				crypto.GenerateIV();
				crypto.GenerateKey();
				crypto.Mode = CipherMode.CBC;
				crypto.Padding = PaddingMode.PKCS7;

				ICryptoTransform cryptoTransform = crypto.CreateDecryptor();

				byte[] encryptedBytes = Encoding.UTF8.GetBytes(passwordCrypted);
				byte[] passwordBytes = cryptoTransform.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

				return result = Encoding.UTF8.GetString(passwordBytes);
			}

			if (File.Exists(storageFile.Path))
			{
				return await FileIO.ReadTextAsync(storageFile);
			}

			return null;
		}
		#endregion

		#region Overrides
		public override string ToString()
		{
			return $"{Email}: {Password}";
		}
		#endregion
	}
}