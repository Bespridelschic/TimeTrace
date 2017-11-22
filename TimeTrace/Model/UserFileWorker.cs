using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace TimeTrace.Model
{
	/// <summary>
	/// Статический класс работы с файлами и каталогами сохраняемых данных
	/// </summary>
	public static class UserFileWorker
	{
		/// <summary>
		/// Сохранение данных пользователя в файл
		/// </summary>
		/// <param name="user">Объект класса <see cref="User"/></param>
		/// <returns>Успех сохранения в файл</returns>
		public static async Task SaveUserToFileAsync(User user)
		{
			StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
			StorageFile storageFile = await storageFolder.CreateFileAsync("_psf.bin", CreationCollisionOption.ReplaceExisting);

			// Расположение файла C:\Users\Bespridelschic\AppData\Local\Packages\c72abfd6-f805-4cdb-8b03-89abadbe4aec_4a9rgd3a66dme\LocalState

			string[] stringToFile = { user.Email, user.Password };

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
		/// <param name="user">Объект класса <see cref="User"/></param>
		/// <returns>Успех загрузки из файла</returns>
		public static async Task LoadUserFromFileAsync(User user)
		{
			try
			{
				StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

				var tryGetFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("_psf.bin");
				if (tryGetFile == null)
				{
					return;
				}

				StorageFile storageFile = await storageFolder.GetFileAsync("_psf.bin");

				// Расположение файла C:\Users\Bespridelschic\AppData\Local\Packages\c72abfd6-f805-4cdb-8b03-89abadbe4aec_4a9rgd3a66dme\LocalState

				var fileLines = await (FileIO.ReadLinesAsync(storageFile));

				if (fileLines.Count <= 2)
				{
					user.Email = fileLines[0];
				}
				if (fileLines.Count == 2)
				{
					user.Password = fileLines[1];
				}
			}
			catch (Exception)
			{
				throw;
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

		/// <summary>
		/// Сохранение пользовательского токена в файл
		/// </summary>
		/// <param name="token">Токен</param>
		/// <returns>Результат сохранения токена в файл</returns>
		public static async Task SaveUserTokenToFileAsync(string token)
		{
			try
			{
				StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
				StorageFile storageFile = await storageFolder.CreateFileAsync("_tkf.bin", CreationCollisionOption.ReplaceExisting);

				// Расположение файла C:\Users\Bespridelschic\AppData\Local\Packages\c72abfd6-f805-4cdb-8b03-89abadbe4aec_4a9rgd3a66dme\LocalState

				string[] stringToFile = { token };

				await FileIO.WriteLinesAsync(storageFile, stringToFile);
			}
			catch (Exception)
			{
				throw;
			}
		}

		/// <summary>
		/// Удаление файлов, содержащих данные для входа
		/// </summary>Результат удаления файлов</returns>
		public static async Task RemoveUserDataFromFilesAsync()
		{
			try
			{
				var storageFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("_psf.bin");
				var storageFile2 = await ApplicationData.Current.LocalFolder.TryGetItemAsync("_tkf.bin");

				if (storageFile != null)
				{
					System.IO.File.Delete(storageFile.Path);
				}
				if (storageFile2 != null)
				{
					System.IO.File.Delete(storageFile2.Path);
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		/// <summary>
		/// Получение Email и Token из файлов
		/// </summary>
		/// <returns>Кортеж email и token</returns>
		public static async Task<(string email, string token)> LoadUserEmailAndTokenFromFile()
		{
			var storageFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("_psf.bin");
			var storageFile2 = await ApplicationData.Current.LocalFolder.TryGetItemAsync("_tkf.bin");

			if (storageFile != null && storageFile2 != null)
			{
				StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

				StorageFile storageFileEmail = await storageFolder.GetFileAsync("_psf.bin");
				StorageFile storageFileToken = await storageFolder.GetFileAsync("_tkf.bin");

				var fileLines = await (FileIO.ReadLinesAsync(storageFileEmail));

				string email = string.Empty;
				if (fileLines.Count > 0)
				{
					email = fileLines[0];
				}

				var fileLines2 = await (FileIO.ReadLinesAsync(storageFileToken));
				string token = string.Empty;

				if (fileLines2.Count > 0)
				{
					token = fileLines2[0];
				}

				if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(token))
				{
					return (email, token);
				}
				else
				{
					return (null, null);
				}
			}
			else
			{
				return (null, null);
			}
		}
	}
}