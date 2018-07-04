using System;
using System.IO;
using System.Threading.Tasks;
using Models.UserModel;

namespace Models.FileSystemRequests
{
	/// <summary>
	/// Class for working with file system
	/// </summary>
	public class FileSystemRequests : IFileSystemService
	{
		#region Saving

		/// <summary>
		/// Save user email to local
		/// </summary>
		/// <param name="user">Saving object <see cref="User"/></param>
		/// <returns>Success of saving</returns>
		public async Task<bool> SaveUserEmailToFileAsync(User user)
		{
			// Check argument for null or empty user email
			if (ArgumentIsNull(user) || string.IsNullOrEmpty(user.Email))
			{
				throw new ArgumentNullException($"{nameof(User)} email is incorrect for saving");
			}

			const string fileName = "UserData.bin";

			return await WriteToFile(fileName, user.Email);
		}

		/// <summary>
		/// Save user email and password hash to file
		/// </summary>
		/// <returns>Result of saving</returns>
		public async Task<bool> SaveUserHashToFileAsync(User user)
		{
			// Check argument for null or empty user email
			if (ArgumentIsNull(user) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
			{
				throw new ArgumentNullException($"{nameof(User)} email is incorrect for saving");
			}

			const string fileName = "UserHashData.bin";

			return await WriteToFile(fileName, user.GetHashEncrypt());
		}

		/// <summary>
		/// Save user token to file
		/// </summary>
		/// <param name="user"><seealso cref="User"/> with token</param>
		/// <returns>Success of saving</returns>
		public async Task<bool> SaveUserTokenToFileAsync(User user)
		{
			// Check argument for null or empty user email
			if (ArgumentIsNull(user) || string.IsNullOrEmpty(user.Token?.Trim()))
			{
				throw new ArgumentNullException($"{nameof(User)} is incorrect for saving");
			}

			const string fileName = "UserTokenData.bin";

			return await WriteToFile(fileName, user.Token);
		}

		#endregion

		#region Loading

		/// <summary>
		/// Getting email from file
		/// </summary>
		/// <param name="user">Object of <see cref="User"/></param>
		/// <returns>Result of loading</returns>
		public async Task<bool> LoadUserEmailFromFileAsync(User user)
		{
			if (ArgumentIsNull(user))
			{
				throw new ArgumentNullException($"{nameof(User)} is incorrect");
			}

			const string fileName = "UserData.bin";

			if (!File.Exists(fileName))
			{
				return false;
			}

			user.Email = await ReadFromFile(fileName);

			return true;
		}

		/// <summary>
		/// Getting email and token from file
		/// </summary>
		/// /// <param name="user">Object of <see cref="User"/></param>
		/// <returns>Result of loading</returns>
		public async Task<bool> LoadUserEmailAndTokenFromFileAsync(User user)
		{
			if (ArgumentIsNull(user))
			{
				throw new ArgumentNullException($"{nameof(User)} is incorrect");
			}

			const string userDataFileName = "UserData.bin";
			const string userTokenFileName = "UserTokenData.bin";

			user.Email = await ReadFromFile(userDataFileName);
			user.Token = await ReadFromFile(userTokenFileName);

			return true;
		}

		#endregion

		#region Removing

		/// <summary>
		/// Remove all confidential files
		/// </summary>
		/// <returns>Result of removing of all files</returns>
		public async Task<bool> RemoveAllUserFilesAsync()
		{
			try
			{
				await Task.Run(() =>
				{
					const string userDataFileName = "UserData.bin";
					if (File.Exists(userDataFileName))
					{
						File.Delete(userDataFileName);
					}

					const string userTokenFileName = "UserTokenData.bin";
					if (File.Exists(userTokenFileName))
					{
						File.Delete(userTokenFileName);
					}

					const string userHashFileName = "UserHashData.bin";
					if (File.Exists(userHashFileName))
					{
						File.Delete(userHashFileName);
					}
				});

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Delete selected files
		/// </summary>
		/// <param name="userData">Is need remove file with user email</param>
		/// <param name="userToken">Is need remove file with user token</param>
		/// <param name="userHash">Is need remove file with user hash code</param>
		/// <returns></returns>
		public async Task<bool> RemoveUserFilesAsync(bool userData, bool userToken, bool userHash)
		{
			try
			{
				await Task.Run(() =>
				{
					if (userData)
					{
						const string userDataFileName = "UserData.bin";
						if (File.Exists(userDataFileName))
						{
							File.Delete(userDataFileName);
						}
					}

					if (userToken)
					{
						const string userTokenFileName = "UserTokenData.bin";
						if (File.Exists(userTokenFileName))
						{
							File.Delete(userTokenFileName);
						}
					}

					if (userHash)
					{
						const string userHashFileName = "UserHashData.bin";
						if (File.Exists(userHashFileName))
						{
							File.Delete(userHashFileName);
						}
					}
				});

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		#endregion

		/// <summary>
		/// Check available for authorization with local hash
		/// </summary>
		/// <param name="user">Current authorized <seealso cref="User"/></param>
		/// <returns>Is available offline authorization</returns>
		public async Task<bool> CanAuthorizationWithoutInternetAsync(User user)
		{
			if (ArgumentIsNull(user))
			{
				throw new ArgumentNullException($"{nameof(User)} is incorrect for saving");
			}

			try
			{
				const string fileName = "UserHashData.bin";

				if (File.Exists(fileName) && await ReadFromFile(fileName) == user.GetHashEncrypt())
				{
					return true;
				}

				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}

		#region Encapsulated methods

		/// <summary>
		/// Cheking input argument
		/// </summary>
		/// <typeparam name="T">Type of argument</typeparam>
		/// <param name="param">Sender parameter</param>
		/// <returns>Is param null</returns>
		private bool ArgumentIsNull<T>(T param)
		{
			return param == null;
		}

		/// <summary>
		/// Save data to file
		/// </summary>
		/// <param name="path">Path to file</param>
		/// <param name="data">Saved data</param>
		/// <returns>Result of saving</returns>
		private async Task<bool> WriteToFile(string path, string data)
		{
			try
			{
				await Task.Run(() =>
				{
					using (StreamWriter sw = File.CreateText(path))
					{
						sw.WriteLine(data);
					}
				});

				return true;
			}
			catch (Exception)
			{
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}

			return false;
		}

		/// <summary>
		/// Read data from file
		/// </summary>
		/// <param name="path">Path to file</param>
		/// <returns>Result of reading</returns>
		private async Task<string> ReadFromFile(string path)
		{
			if (!File.Exists(path))
			{
				return string.Empty;
			}

			try
			{
				// Read first line from file
				return await Task.Run(async () =>
				{
					using (StreamReader sr = File.OpenText(path))
					{
						string lineFromFile = await sr.ReadLineAsync();

						if (string.IsNullOrEmpty(lineFromFile) || string.IsNullOrEmpty(lineFromFile.Trim()))
						{
							return string.Empty;
						}

						return lineFromFile;
					}
				});
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		#endregion
	}
}