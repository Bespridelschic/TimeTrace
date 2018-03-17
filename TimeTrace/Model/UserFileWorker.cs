using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;

namespace TimeTrace.Model
{
	/// <summary>
	/// Class for working with files and directories
	/// </summary>
	public static class UserFileWorker
	{
		/// <summary>
		/// Save user to file
		/// </summary>
		/// <param name="user">Saving object <see cref="User"/></param>
		/// <returns>Success of saving</returns>
		public static async Task SaveUserToFileAsync(this User user)
		{
			if (ArgumentIsNull(user))
			{
				throw new ArgumentNullException($"{nameof(User)} is null");
			}

			StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
			StorageFile storageFile = await storageFolder.CreateFileAsync("_psf.bin", CreationCollisionOption.ReplaceExisting);

			string[] stringToFile = { user.Email };

			await FileIO.WriteLinesAsync(storageFile, stringToFile);
		}

		/// <summary>
		/// Getting email and password from file
		/// </summary>
		/// <param name="user">Object of <see cref="User"/></param>
		/// <returns>Success of loading</returns>
		public static async Task LoadUserFromFileAsync(this User user)
		{
			if (ArgumentIsNull(user))
			{
				throw new ArgumentNullException($"{nameof(User)} is null");
			}

			try
			{
				StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

				var tryGetFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("_psf.bin");
				if (tryGetFile == null)
				{
					return;
				}

				StorageFile storageFile = await storageFolder.GetFileAsync("_psf.bin");

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
		}

		/// <summary>
		/// Save user token to file
		/// </summary>
		/// <param name="token">Token</param>
		/// <returns>Success of saving</returns>
		public static async Task SaveUserTokenToFileAsync(string token)
		{
			try
			{
				StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
				StorageFile storageFile = await storageFolder.CreateFileAsync("_tkf.bin", CreationCollisionOption.ReplaceExisting);

				string[] stringToFile = { token };

				await FileIO.WriteLinesAsync(storageFile, stringToFile);
			}
			catch (Exception)
			{
				throw;
			}
		}

		/// <summary>
		/// Remove all confidential files
		/// </summary>Результат удаления файлов</returns>
		public static async Task RemoveUserDataFromFilesAsync(this User user)
		{
			if (ArgumentIsNull(user))
			{
				throw new ArgumentNullException($"{nameof(User)} is null");
			}

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
		/// Getting email and token from file
		/// </summary>
		/// <returns>Кортеж email и token</returns>
		public static async Task<(string email, string token)> LoadUserEmailAndTokenFromFileAsync()
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

		/// <summary>
		/// Cheking input argument
		/// </summary>
		/// <typeparam name="T">Type of argument</typeparam>
		/// <param name="param">Sender parameter</param>
		/// <returns>Is param null</returns>
		private static bool ArgumentIsNull<T>(T param)
		{
			return param == null;
		}

		/// <summary>
		/// Private class for encrypt / decrypt AES
		/// </summary>
		private class AesEnDecryption
		{
			// Key with 256 and IV with 16 length
			private string AES_Key = "Y+3xQDLPWalRKK3U/JuabsJNnuEO91zRiOH5gjgOqck=";
			private string AES_IV = "15CV1/ZOnVI3rY4wk4INBg==";
			private IBuffer m_iv = null;
			private CryptographicKey m_key;

			public AesEnDecryption()
			{
				IBuffer key = Convert.FromBase64String(AES_Key).AsBuffer();
				m_iv = Convert.FromBase64String(AES_IV).AsBuffer();
				SymmetricKeyAlgorithmProvider provider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);
				m_key = provider.CreateSymmetricKey(key);
			}

			/// <summary>
			/// Метод шифрования байт
			/// </summary>
			/// <param name="input">Массив байт, подлежащих шифрованию</param>
			/// <returns>Массив зашифрованных байт</returns>
			public byte[] Encrypt(byte[] input)
			{

				IBuffer bufferMsg = CryptographicBuffer.ConvertStringToBinary(Encoding.ASCII.GetString(input), BinaryStringEncoding.Utf8);
				IBuffer bufferEncrypt = CryptographicEngine.Encrypt(m_key, bufferMsg, m_iv);
				return bufferEncrypt.ToArray();
			}

			/// <summary>
			/// Метод расшифровки байт
			/// </summary>
			/// <param name="input">Массив зашифрованных байт</param>
			/// <returns>Массив расшифрованных байт</returns>
			public byte[] Decrypt(byte[] input)
			{
				IBuffer bufferDecrypt = CryptographicEngine.Decrypt(m_key, input.AsBuffer(), m_iv);
				return bufferDecrypt.ToArray();
			}
		}
	}
}