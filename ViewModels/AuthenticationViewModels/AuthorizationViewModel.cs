using System;
using System.Linq;
using System.Threading.Tasks;
using Models.InternetRequests;
using Models.UserModel;
using ViewModels.Dialogs;

namespace ViewModels.AuthenticationViewModels
{
	/// <summary>
	/// Authorization of user in system
	/// </summary>
	public class AuthorizationViewModel : BaseViewModel, IMessageNotification
	{
		#region Properties

		public User User { get; private set; }

		public event Action<string, string> MessageNotification = delegate { };

		public bool IsUserDataSave { get; set; }

		#endregion

		public AuthorizationViewModel()
		{
			User = new User();
			IsUserDataSave = true;
		}

		/// <summary>
		/// Trying to authorization in system
		/// </summary>
		/// <returns></returns>
		public async Task<bool> SignInAsync()
		{
			if (!InternetRequests.CheckForInternetConnection())
			{
				await MessageInvokeAsync("No internet connection", "Authorization error");
				return false;
			}

			var validationErrorList = User.Validation();
			if (validationErrorList.Count > 0)
			{
				string errorsList = string.Empty;
				foreach (var errorText in validationErrorList)
				{
					errorsList += errorText + "\n";
				}

				await MessageInvokeAsync(errorsList, "Validation error");
				return false;
			}

			return true;
		}

		public async Task MessageInvokeAsync(string message, string title = "")
		{
			await Task.Run(() => MessageNotification(message, title));
		}
	}
}