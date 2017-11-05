using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

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
			set { password = value; }
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

		public string PasswordEncrypt()
		{
			SHA512 hash = SHA512.Create();


			TripleDES tripleDES = TripleDES.Create();
			tripleDES.KeySize = 128;
			tripleDES.BlockSize = 128;
			tripleDES.Padding = PaddingMode.PKCS7;
			tripleDES.Mode = CipherMode.CBC;
			tripleDES.Key = Encoding.UTF8.GetBytes("TimeTraceSecretCodeForPassword");

			ICryptoTransform cryptoTransform = tripleDES.CreateEncryptor();
			byte[] passwordIdByte = Encoding.UTF8.GetBytes(password);
			byte[] cryptoPassword = cryptoTransform.TransformFinalBlock(passwordIdByte, 0, passwordIdByte.Length);

			return Convert.ToBase64String(cryptoPassword);
		}

		#region Overrides
		public override string ToString()
		{
			return $"{Email}, {Password}";
		}
		#endregion
	}
}
