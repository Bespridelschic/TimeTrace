using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace TimeTrace.ViewModel.MainViewModel.ContactsViewModel
{
	public class ChatViewModel : BaseViewModel
	{
		#region Properties

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
			
		}

		/// <summary>
		/// Sending message
		/// </summary>
		public async void SendMessage()
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
