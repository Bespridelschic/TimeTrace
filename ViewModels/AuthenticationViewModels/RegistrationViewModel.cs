using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Models.InternetRequests;
using Models.UserModel;
using ViewModels.Dialogs;

namespace ViewModels.AuthenticationViewModels
{
	/// <summary>
	/// Registration of new user in system
	/// </summary>
	public class RegistrationViewModel : BaseViewModel, IMessageNotification
	{
		#region Properties

		public RegisteredUser User { get; private set; }

		public event Action<string, string> MessageNotification = delegate { };

		#endregion

		/// <summary>
		/// Creation of new user for registration
		/// </summary>
		/// <param name="user">Prepared <seealso cref="User"/> object</param>
		public RegistrationViewModel(User user)
		{
			User = new RegisteredUser(user.Email, string.Empty);
		}

		/// <summary>
		/// Trying to registration in system
		/// </summary>
		/// <returns></returns>
		public async Task<bool> SignUnAsync()
		{
			if (!InternetRequests.CheckForInternetConnection())
			{
				await MessageInvokeAsync("No internet connection", "Registration error");
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