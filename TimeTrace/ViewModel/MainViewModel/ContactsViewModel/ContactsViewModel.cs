using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using TimeTrace.Model;
using TimeTrace.Model.Events;
using TimeTrace.Model.Events.DBContext;
using TimeTrace.View.MainView.ContactPages;

namespace TimeTrace.ViewModel.MainViewModel.ContactsViewModel
{
	/// <summary>
	/// Contacts view model
	/// </summary>
	public class ContactsViewModel : BaseViewModel
	{
		#region Properties

		/// <summary>
		/// Collection of viewed contacts
		/// </summary>
		public ObservableCollection<Contact> Contacts { get; set; }

		private int selectedContact;
		/// <summary>
		/// Index of selected contact
		/// </summary>
		public int SelectedContact
		{
			get => selectedContact;
			set
			{
				selectedContact = value;
				OnPropertyChanged();
			}
		}

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		public ContactsViewModel()
		{
			Contacts = new ObservableCollection<Contact>()
			{
				new Contact()
				{
					Name = "Name1", Email = "Email1"
				},
				new Contact()
				{
					Name = "Name2", Email = "Email3"
				},
				new Contact()
				{
					Name = "Name2", Email = "Email3"
				},
			};

			/*using (MapEventContext db = new MapEventContext())
			{
				ObservableCollection<Contact> list;
				ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
			}*/
		}

		public async void ShowMessage(object sender, RightTappedRoutedEventArgs e)
		{
			var s = (FrameworkElement)sender;
			var d = s.DataContext;

			await new MessageDialog($"Вызвал ").ShowAsync();
		}

		public void StartChat()
		{
			(Application.Current as App).AppFrame.Navigate(typeof(ChatPage));
		}

		/// <summary>
		/// Remove of selected contact
		/// </summary>
		public async void ContactRemove()
		{
			ContentDialog contentDialog = new ContentDialog()
			{
				Title = "Подтверждение действия",
				//Content = $"Вы уверены что хотите удалить контакт \"{MapEvents[SelectedMapEvent].Name}\"?",
				PrimaryButtonText = "Удалить",
				CloseButtonText = "Отмена",
				DefaultButton = ContentDialogButton.Close
			};

			var result = await contentDialog.ShowAsync();

			if (result == ContentDialogResult.Primary)
			{
				using (MapEventContext db = new MapEventContext())
				{
					/*db.MapEvents.FirstOrDefault(i => i.Id == MapEvents[SelectedMapEvent].Id).IsDelete = true;
					MapEvents.RemoveAt(SelectedMapEvent);*/

					db.SaveChanges();
				}

				await (new MessageDialog("Контакт успешно удалён", "Успех")).ShowAsync();
			}
		}

		/// <summary>
		/// Show info about contact
		/// </summary>
		public async void MoreAboutContact()
		{
			//MapEvent tempEvent = MapEvents[SelectedMapEvent];

			TextBlock contentText = new TextBlock()
			{
				
			};

			ContentDialog contentDialog = new ContentDialog()
			{
				Title = "Подробности",
				Content = contentText,
				CloseButtonText = "Закрыть",
				DefaultButton = ContentDialogButton.Close
			};

			var result = await contentDialog.ShowAsync();
		}
	}
}
