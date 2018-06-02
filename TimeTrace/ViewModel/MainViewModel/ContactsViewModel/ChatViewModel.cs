using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using TimeTrace.Model;

namespace TimeTrace.ViewModel.MainViewModel.ContactsViewModel
{
	/// <summary>
	/// Chat view model
	/// </summary>
	public class ChatViewModel : BaseViewModel
	{
		#region Properties

		private Contact interlocutor;
		/// <summary>
		/// Interlocutor object
		/// </summary>
		public Contact Interlocutor
		{
			get => interlocutor;
			set
			{
				interlocutor = value;
				OnPropertyChanged();
			}
		}

		private string currentMessage { get; set; }
		/// <summary>
		/// Sended message
		/// </summary>
		public string CurrentMessage
		{
			get => currentMessage;
			set
			{
				currentMessage = value;
				OnPropertyChanged();
			}
		}

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		public ChatViewModel()
		{
			StartPageViewModel.Instance.SetHeader(StartPageViewModel.Headers.Contacts);
		}

		/// <summary>
		/// Sending message
		/// </summary>
		public async void SendMessageAsync()
		{
			if (string.IsNullOrEmpty(CurrentMessage))
			{
				await new MessageDialog("Введите сообщение для отправки", "Ошибка").ShowAsync();
				return;
			}

			await new MessageDialog(CurrentMessage, "Отправляемое сообщение").ShowAsync();
		}

	}
}
