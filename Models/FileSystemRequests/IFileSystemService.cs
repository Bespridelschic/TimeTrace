using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Models.UserModel;

namespace Models.FileSystemRequests
{
	/// <summary>
	/// Crossplatform interaction with file system
	/// </summary>
    public interface IFileSystemService
	{
		Task<bool> SaveUserEmailToFileAsync(User user);
		Task<bool> SaveUserHashToFileAsync(User user);
		Task<bool> SaveUserTokenToFileAsync(User user);

		Task<bool> LoadUserEmailFromFileAsync(User user);
		Task<bool> LoadUserEmailAndTokenFromFileAsync(User user);

		Task<bool> RemoveAllUserFilesAsync();
		Task<bool> RemoveUserFilesAsync(bool userData, bool userToken, bool userHash);

		Task<bool> CanAuthorizationWithoutInternetAsync(User user);
	}
}
