using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using TimeTrace.Model;
using TimeTrace.Model.Events;
using TimeTrace.Model.DBContext;
using TimeTrace.Model.Requests;
using TimeTrace.View.Converters;
using TimeTrace.View.MainView.ContactPages;
using TimeTrace.View.MainView.PersonalMapsCreatePages;

namespace TimeTrace.ViewModel.MainViewModel.ContactsViewModel
{
	/// <summary>
	/// Contacts view model
	/// </summary>
	public class ContactsViewModel : BaseViewModel
	{
		#region Properties

		private ObservableCollection<Contact> contacts;
		/// <summary>
		/// Collection of viewed contacts
		/// </summary>
		public ObservableCollection<Contact> Contacts
		{
			get => contacts;
			set
			{
				contacts = value;
				OnPropertyChanged();
			}
		}

		private List<Contact> selectedContacts;
		/// <summary>
		/// Collection of multiple contacts selection
		/// </summary>
		public List<Contact> SelectedContacts
		{
			get => selectedContacts;
			set
			{
				selectedContacts = value;
				OnPropertyChanged();
			}
		}

		private ObservableCollection<string> contactsSuggestList;
		/// <summary>
		/// Filter tips
		/// </summary>
		public ObservableCollection<string> ContactsSuggestList
		{
			get => contactsSuggestList;
			set
			{
				contactsSuggestList = value;
				OnPropertyChanged();
			}
		}

		private int? selectedContact;
		/// <summary>
		/// Index of selected contact
		/// </summary>
		public int? SelectedContact
		{
			get => selectedContact;
			set
			{
				selectedContact = value;
				OnPropertyChanged();
			}
		}

		private ListViewSelectionMode multipleSelection;
		/// <summary>
		/// Is multiple selection enable
		/// </summary>
		public ListViewSelectionMode MultipleSelection
		{
			get => multipleSelection;
			set
			{
				multipleSelection = value;
				OnPropertyChanged();
			}
		}

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		public ContactsViewModel()
		{
			MultipleSelection = ListViewSelectionMode.Single;
			RefreshContactsAsync().GetAwaiter();

			ContactsSuggestList = new ObservableCollection<string>();

			using (MainDatabaseContext db = new MainDatabaseContext())
			{
				ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

				Contacts = new ObservableCollection<Contact>(db.Contacts.Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete).ToList());
			}
		}

		/// <summary>
		/// Selecting item of list with right click
		/// </summary>
		/// <param name="sender">Sender list</param>
		/// <param name="e">Parameter</param>
		public void SelectItemOnRightClick(object sender, RightTappedRoutedEventArgs e)
		{
			ListView listView = (ListView)sender;
			if (((FrameworkElement)e.OriginalSource).DataContext is Contact selectedItem)
			{
				SelectedContact = Contacts.IndexOf(selectedItem);
			}
		}

		/// <summary>
		/// Select several contacts
		/// </summary>
		/// <param name="sender">ListView</param>
		/// <param name="e">Parameters</param>
		public void MultipleContactsSelection(object sender, SelectionChangedEventArgs e)
		{
			ListView listView = sender as ListView;
			SelectedContacts = new List<Contact>();

			foreach (Contact item in listView.SelectedItems)
			{
				SelectedContacts.Add(item);
			};
		}

		/// <summary>
		/// Navigate to chat page
		/// </summary>
		public void StartChat()
		{
			if (SelectedContact.HasValue)
			{
				(Application.Current as App).AppFrame.Navigate(typeof(ChatPage), Contacts[SelectedContact.Value]);
			}
		}

		#region Adding and editing contacts

		/// <summary>
		/// Add new contact
		/// </summary>
		public async void AddContactAsync()
		{
			await ContactDialog();
		}

		/// <summary>
		/// Edit selected contact
		/// </summary>
		public async void EditContactAsync()
		{
			if (SelectedContact.HasValue)
			{
				await ContactDialog(Contacts[SelectedContact.Value]);
			}
		}

		/// <summary>
		/// Dialog for adding new contact or edit selected
		/// </summary>
		/// <param name="contact">Sended contact. If null - create new</param>
		private async Task ContactDialog(Contact contact = null)
		{
			#region Text boxes

			TextBox email = new TextBox()
			{
				Header = "Электронная почта",
				PlaceholderText = "example@gmail.com",
				Margin = new Thickness(0, 0, 0, 10),
				MaxLength = 30,
				Text = (contact == null) ? string.Empty : contact.Email,
				IsReadOnly = (contact != null),
			};

			TextBox name = new TextBox()
			{
				Header = "Используемое имя",
				PlaceholderText = "Псевдоним контакта",
				Margin = new Thickness(0, 0, 0, 10),
				MaxLength = 50,
				Text = (contact == null) ? string.Empty : contact.Name,
			};

			#endregion

			StackPanel panel = new StackPanel();
			panel.Children.Add(email);
			panel.Children.Add(name);

			ContentDialog dialog = new ContentDialog()
			{
				Title = (contact == null) ? "Добавление нового контакта" : "Изменение контакта",
				Content = panel,
				PrimaryButtonText = (contact == null) ? "Добавить" : "Изменить",
				CloseButtonText = "Отложить",
				DefaultButton = ContentDialogButton.Primary,
			};

			var result = await dialog.ShowAsync();

			if (result == ContentDialogResult.Primary)
			{
				if (string.IsNullOrEmpty(email.Text) || string.IsNullOrEmpty(name.Text))
				{
					await new MessageDialog("Не заполнено одно из полей", (contact == null) ? "Ошибка добавления нового контакта" : "Ошибка изменения контакта").ShowAsync();

					return;
				}

				string pattern = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
				var res = Regex.Match(email.Text, pattern);

				if (res.Success)
				{
					// Adding new contact
					if (contact == null)
					{
						Contact newContact = new Contact()
						{
							Email = email.Text,
							Name = name.Text
						};

						using (MainDatabaseContext db = new MainDatabaseContext())
						{
							db.Contacts.Add(newContact);
							db.SaveChanges();
						}

						Contacts.Add(newContact);

						await RefreshContactsAsync();

						await new MessageDialog($"Ваше приглашение для {newContact.Email} успешно отправлено", "Успех").ShowAsync();

						return;
					}

					// Edit contact
					if (Contacts[SelectedContact.Value].Name != name.Text)
					{
						int tempSelectedContact = SelectedContact.Value;
						var temp = Contacts[tempSelectedContact];
						temp.Name = name.Text;
						Contacts[tempSelectedContact] = temp;

						Contacts[tempSelectedContact].Name = name.Text;

						using (MainDatabaseContext db = new MainDatabaseContext())
						{
							db.Contacts.Update(Contacts[tempSelectedContact]);
							db.SaveChanges();
						}

						await RefreshContactsAsync();

						await new MessageDialog($"Контакт {Contacts[tempSelectedContact].Email} успешно изменен", "Успех").ShowAsync();
					}

				}
				else
				{
					await new MessageDialog("Не корректно введенный адрес электронной почты", (contact == null) ? "Ошибка добавления нового контакта" : "Ошибка изменения контакта").ShowAsync();
				}
			}
		}

		#endregion

		/// <summary>
		/// Sending added contacts and get actuals from server
		/// </summary>
		/// <returns>Refresh result</returns>
		public async Task RefreshContactsAsync()
		{
			try
			{
				var contactsSynchResult = await InternetRequests.ContactsSynchronizationRequestAsync();
				if (contactsSynchResult == 1)
				{
					throw new HttpRequestException("Internet connection problem or bad token");
				}
			}
			catch (Exception)
			{
				new MessageDialog("Не удаётся синхронизировать контакты. Проверьте своё подключение к интернету, а так же попробуйте перезайти в аккаунт",
					"Ошибка синхронизации контактов");
			}

			//using (MainDatabaseContext db = new MainDatabaseContext())
			//{
			//	ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

			//	Contacts = new ObservableCollection<Contact>(db.Contacts.Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete).ToList());
			//}
		}

		#region Contacts removing

		/// <summary>
		/// Remove of selected contact
		/// </summary>
		public async void ContactRemoveAsync()
		{
			if (SelectedContact.HasValue)
			{
				ContentDialog contentDialog = new ContentDialog()
				{
					Title = "Подтверждение действия",
					Content = $"Вы уверены что хотите удалить контакт \"{Contacts[SelectedContact.Value].Name}\"?",
					PrimaryButtonText = "Удалить",
					CloseButtonText = "Отмена",
					DefaultButton = ContentDialogButton.Close
				};

				var result = await contentDialog.ShowAsync();

				if (result == ContentDialogResult.Primary)
				{
					using (MainDatabaseContext db = new MainDatabaseContext())
					{
						db.Contacts.FirstOrDefault(i => i.Id == Contacts[SelectedContact.Value].Id).IsDelete = true;
						Contacts.RemoveAt(SelectedContact.Value);

						db.SaveChanges();
					}
				}
			}
		}

		/// <summary>
		/// Remove of several contacts
		/// </summary>
		public async void ContactsRemoveAsync()
		{
			if (MultipleSelection == ListViewSelectionMode.Single)
			{
				MultipleSelection = ListViewSelectionMode.Multiple;
			}
			else
			{
				if (SelectedContacts == null || SelectedContacts.Count <= 0)
				{
					MultipleSelection = ListViewSelectionMode.Single;
					return;
				}

				string removedContacts = string.Empty;

				foreach (var contact in SelectedContacts)
				{
					removedContacts += $"{contact.Email}\n";
				}

				ScrollViewer scrollViewer = new ScrollViewer()
				{
					VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
					Content = new TextBlock() { Text = removedContacts },
				};

				StackPanel mainPanel = new StackPanel()
				{
					Margin = new Thickness(0, 0, 0, 10),
				};
				mainPanel.Children.Add(new TextBlock() { Text = "Вы уверены что хотите удалить контакты:" });
				mainPanel.Children.Add(scrollViewer);

				ContentDialog contentDialog = new ContentDialog()
				{
					Title = "Подтверждение действия",
					Content = mainPanel,
					PrimaryButtonText = "Удалить",
					CloseButtonText = "Отмена",
					DefaultButton = ContentDialogButton.Close
				};

				var result = await contentDialog.ShowAsync();

				if (result == ContentDialogResult.Primary)
				{
					using (MainDatabaseContext db = new MainDatabaseContext())
					{
						foreach (var contact in SelectedContacts)
						{
							contact.IsDelete = true;
						}

						db.Contacts.UpdateRange(SelectedContacts);
						db.SaveChanges();

						foreach (var contact in SelectedContacts)
						{
							Contacts.Remove(contact);
						}
					}
				}

				MultipleSelection = ListViewSelectionMode.Single;
			}
		}

		#endregion

		/// <summary>
		/// Show info about contact
		/// </summary>
		public async void MoreAboutContactAsync()
		{
			if (SelectedContact.HasValue)
			{
				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					TextBlock email = new TextBlock()
					{
						Text = $"Контактный адрес: {Contacts[selectedContact.Value].Email}",
					};

					TextBlock name = new TextBlock()
					{
						Text = $"Псевдоним: {Contacts[selectedContact.Value].Name}",
					};

					TextBlock addedAt = new TextBlock()
					{
						Text = $"Добавлен: {Contacts[selectedContact.Value].CreateAt.Value.ToLocalTime()}",
					};

					StackPanel mainPanel = new StackPanel();
					mainPanel.Children.Add(email);
					mainPanel.Children.Add(name);
					mainPanel.Children.Add(addedAt);

					ContentDialog contentDialog = new ContentDialog()
					{
						Title = "Подробности",
						Content = mainPanel,
						CloseButtonText = "Закрыть",
						DefaultButton = ContentDialogButton.Close
					};

					var result = await contentDialog.ShowAsync();
				}
			}
		}

		#region Searching contacts

		/// <summary>
		/// Filtration of input filters
		/// </summary>
		/// <param name="sender">Input filter</param>
		/// <param name="args">Event args</param>
		public void ContactsFilter(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (Contacts == null) return;

			if (args.CheckCurrent())
			{
				ContactsSuggestList.Clear();
			}

			// Remove all contacts for adding relevant filter
			Contacts.Clear();

			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

			// Select all contacts
			if (string.IsNullOrEmpty(sender.Text))
			{
				ContactsSuggestList.Clear();

				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					foreach (var i in db.Contacts.Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete).Select(i => i))
					{
						Contacts.Add(i);
					}
				}
			}

			else
			{
				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					foreach (var i in db.Contacts
						.Where(i => (i.Name.ToLowerInvariant().Contains(sender.Text.ToLowerInvariant()) ||
									i.Email.ToLowerInvariant().Contains(sender.Text.ToLowerInvariant())) &&
									i.EmailOfOwner == (string)localSettings.Values["email"] &&
									!i.IsDelete)
						.Select(i => i))
					{
						if (!ContactsSuggestList.Contains(i.Name))
						{
							ContactsSuggestList.Add(i.Name);
						}

						Contacts.Add(i);
					}
				}
			}
		}

		/// <summary>
		/// Click on Find contact
		/// </summary>
		/// <param name="sender">Object</param>
		/// <param name="args">Args</param>
		public void ContactsFilterQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			ContactsSuggestList.Clear();

			if (string.IsNullOrEmpty(args.QueryText))
			{
				return;
			}

			if (Contacts != null)
			{
				Contacts.Clear();

				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
					var term = args.QueryText.ToLower();
					foreach (var i in db.Contacts.Where(i => i.Name.ToLower().Contains(term) && !i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"]).Select(i => i))
					{
						Contacts.Add(i);
					}
				}
			}
		}

		#endregion
	}
}