using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using TimeTrace.Model;
using TimeTrace.Model.DBContext;
using TimeTrace.Model.Requests;
using TimeTrace.View.MainView.ContactPages;
using Windows.ApplicationModel.Resources;

namespace TimeTrace.ViewModel.MainViewModel.ContactsViewModel
{
	/// <summary>
	/// Contacts view model
	/// </summary>
	public class ContactsViewModel : BaseViewModel, ISearchable<string>
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

		private int selectedTabIndex;
		/// <summary>
		/// Selected tab pivot
		/// </summary>
		public int SelectedTabIndex
		{
			get => selectedTabIndex;
			set
			{
				selectedTabIndex = value;

				if (Contacts.Count > 0)
				{
					Contacts.Clear();
				}

				if (selectedTabIndex == 1)
				{
					using (MainDatabaseContext db = new MainDatabaseContext())
					{
						ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

						var busyMapEvents = db.MapEvents.Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] &&
																	!i.IsDelete &&
																	!string.IsNullOrEmpty(i.UserBind) &&
																	i.End >= DateTime.UtcNow);

						if (busyMapEvents.Count() > 0)
						{
							foreach (var item in db.Contacts.Join
								(
									busyMapEvents,
									i => i.Name.ToLowerInvariant(),
									w => w.UserBind.ToLowerInvariant(),
									(i, w) => i
								))
							{
								Contacts.Add(item);
							}
						}
					}
				}
				else
				{
					using (MainDatabaseContext db = new MainDatabaseContext())
					{
						ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

						foreach (var item in db.Contacts.Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete).ToList())
						{
							Contacts.Add(item);
						}
					}
				}

				OnPropertyChanged();
			}
		}

		/// <summary>
		/// If offine sign in, block internet features
		/// </summary>
		public bool InternetFeaturesEnable { get; set; }

		/// <summary>
		/// Localization resource loader
		/// </summary>
		public ResourceLoader ResourceLoader { get; set; }

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		public ContactsViewModel()
		{
			ResourceLoader = ResourceLoader.GetForCurrentView("ContactsVM");
			StartPageViewModel.Instance.SetHeader(StartPageViewModel.Headers.Contacts);

			Contacts = new ObservableCollection<Contact>();
			SelectedTabIndex = 0;
			MultipleSelection = ListViewSelectionMode.Single;
			SearchSuggestions = new ObservableCollection<string>();

			InternetFeaturesEnable = StartPageViewModel.Instance.InternetFeaturesEnable;
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
				if (InternetFeaturesEnable)
				{
					(Application.Current as App).AppFrame.Navigate(typeof(ChatPage), Contacts[SelectedContact.Value]);
				}
			}
		}

		#region Adding and editing contacts

		/// <summary>
		/// Add new contact
		/// </summary>
		public async void AddContactAsync()
		{
			await ContactDialogAsync();
		}

		/// <summary>
		/// Edit selected contact
		/// </summary>
		public async void EditContactAsync()
		{
			if (SelectedContact.HasValue)
			{
				await ContactDialogAsync(Contacts[SelectedContact.Value]);
			}
		}

		/// <summary>
		/// Dialog for adding new contact or edit selected
		/// </summary>
		/// <param name="contact">Sended contact. If null - create new</param>
		private async Task ContactDialogAsync(Contact contact = null)
		{
			#region Text boxes

			TextBox email = new TextBox()
			{
				Header = ResourceLoader.GetString("/ContactsVM/EmailHeader"),
				PlaceholderText = "example@gmail.com",
				Margin = new Thickness(0, 0, 0, 10),
				MaxLength = 30,
				Text = (contact == null) ? string.Empty : contact.Email,
				IsReadOnly = (contact != null),
			};

			TextBox name = new TextBox()
			{
				Header = ResourceLoader.GetString("/ContactsVM/NameHeader"),
				PlaceholderText = ResourceLoader.GetString("/ContactsVM/NamePlaceholder"),
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
				Title = (contact == null) ? ResourceLoader.GetString("/ContactsVM/AddingNewContact") : ResourceLoader.GetString("/ContactsVM/EditingContact"),
				Content = panel,
				PrimaryButtonText = (contact == null) ? ResourceLoader.GetString("/ContactsVM/AddContact") : ResourceLoader.GetString("/ContactsVM/ChangeContact"),
				CloseButtonText = ResourceLoader.GetString("/ContactsVM/Later"),
				DefaultButton = ContentDialogButton.Primary,
			};

			var result = await dialog.ShowAsync();

			if (result == ContentDialogResult.Primary)
			{
				if (string.IsNullOrEmpty(email.Text) || string.IsNullOrEmpty(name.Text))
				{
					await new MessageDialog(ResourceLoader.GetString("/ContactsVM/FieldsEmptyError"),
											(contact == null) ? ResourceLoader.GetString("/ContactsVM/AddContactError")
											: ResourceLoader.GetString("/ContactsVM/EditingContactError"))
						.ShowAsync();

					return;
				}

				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					if (contact == null && (db.Contacts.Where(i => i.Email.ToLower() == email.Text.Trim().ToLower()).Count() > 0))
					{
						await new MessageDialog(ResourceLoader.GetString("/ContactsVM/ContactAlreadyAddedError"), ResourceLoader.GetString("/ContactsVM/AddContactError")).ShowAsync();

						return;
					}
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
							Email = email.Text.Trim(),
							Name = name.Text.Trim()
						};

						using (MainDatabaseContext db = new MainDatabaseContext())
						{
							db.Contacts.Add(newContact);
							db.SaveChanges();
						}

						Contacts.Add(newContact);

						await RefreshContactsAsync();

						await new MessageDialog($"{ResourceLoader.GetString("/ContactsVM/InviteFor")} {newContact.Email} {ResourceLoader.GetString("/ContactsVM/SuccessSend")}",
							ResourceLoader.GetString("/ContactsVM/Success")).ShowAsync();

						return;
					}

					// Edit contact
					if (Contacts[SelectedContact.Value].Name != name.Text)
					{
						int tempSelectedContact = SelectedContact.Value;
						var temp = Contacts[tempSelectedContact];
						temp.Name = name.Text.Trim();
						Contacts[tempSelectedContact] = temp;

						Contacts[tempSelectedContact].Name = name.Text.Trim();

						using (MainDatabaseContext db = new MainDatabaseContext())
						{
							db.Contacts.Update(Contacts[tempSelectedContact]);
							db.SaveChanges();
						}

						await RefreshContactsAsync();

						await new MessageDialog($"{ResourceLoader.GetString("/ContactsVM/Contact")} {Contacts[tempSelectedContact].Email} {ResourceLoader.GetString("/ContactsVM/SuccessChanging")}",
							ResourceLoader.GetString("/ContactsVM/Success")).ShowAsync();
					}

				}
				else
				{
					await new MessageDialog(ResourceLoader.GetString("/ContactsVM/IncorrectEmailError"),
							(contact == null)
								? ResourceLoader.GetString("/ContactsVM/AddContactError")
								: ResourceLoader.GetString("/ContactsVM/EditingContactError"))
						.ShowAsync();
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
				new MessageDialog(ResourceLoader.GetString("/ContactsVM/SynchronizationMessageError"),
					ResourceLoader.GetString("/ContactsVM/SynchronizationError"));

				return;
			}

			using (MainDatabaseContext db = new MainDatabaseContext())
			{
				ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

				Contacts.Clear();

				foreach (var item in db.Contacts.Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete).ToList())
				{
					Contacts.Add(item);
				}
			}
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
					Title = ResourceLoader.GetString("/ContactsVM/ConfirmAction"),
					Content = $"{ResourceLoader.GetString("/ContactsVM/ConfirmRemoving")} \"{Contacts[SelectedContact.Value].Name}\"?",
					PrimaryButtonText = ResourceLoader.GetString("/ContactsVM/Remove"),
					CloseButtonText = ResourceLoader.GetString("/ContactsVM/Cancel"),
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

					await RefreshContactsAsync();
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
				mainPanel.Children.Add(new TextBlock() { Text = ResourceLoader.GetString("/ContactsVM/ConfirmBulkRemoving") });
				mainPanel.Children.Add(scrollViewer);

				ContentDialog contentDialog = new ContentDialog()
				{
					Title = ResourceLoader.GetString("/ContactsVM/ConfirmAction"),
					Content = mainPanel,
					PrimaryButtonText = ResourceLoader.GetString("/ContactsVM/Remove"),
					CloseButtonText = ResourceLoader.GetString("/ContactsVM/Cancel"),
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

					await RefreshContactsAsync();
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
						Text = $"{ResourceLoader.GetString("/ContactsVM/Email")} {Contacts[selectedContact.Value].Email}",
					};

					TextBlock name = new TextBlock()
					{
						Text = $"{ResourceLoader.GetString("/ContactsVM/Name")} {Contacts[selectedContact.Value].Name}",
					};

					TextBlock addedAt = new TextBlock()
					{
						Text = $"{ResourceLoader.GetString("/ContactsVM/Uploaded")} {Contacts[selectedContact.Value].CreateAt.Value.ToLocalTime()}",
					};

					StackPanel mainPanel = new StackPanel();
					mainPanel.Children.Add(email);
					mainPanel.Children.Add(name);
					mainPanel.Children.Add(addedAt);

					ContentDialog contentDialog = new ContentDialog()
					{
						Title = ResourceLoader.GetString("/ContactsVM/Details"),
						Content = mainPanel,
						CloseButtonText = ResourceLoader.GetString("/ContactsVM/Close"),
						DefaultButton = ContentDialogButton.Close
					};

					var result = await contentDialog.ShowAsync();
				}
			}
		}

		#region Searching contacts

		private string searchTerm;
		/// <summary>
		/// Term for searching
		/// </summary>
		public string SearchTerm
		{
			get => searchTerm;
			set
			{
				searchTerm = value;
				OnPropertyChanged();
			}
		}

		private ObservableCollection<string> searchSuggestions;
		/// <summary>
		/// Suggestions for searching
		/// </summary>
		public ObservableCollection<string> SearchSuggestions
		{
			get => searchSuggestions;
			set
			{
				searchSuggestions = value;
				OnPropertyChanged();
			}

		}

		/// <summary>
		/// Filtration of input terms
		/// </summary>
		/// <param name="sender">Input term</param>
		/// <param name="args">Event args</param>
		public void DynamicSearch()
		{
			if (Contacts == null) return;

			SearchSuggestions.Clear();

			// Remove all contacts for adding relevant filter
			Contacts.Clear();

			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

			// Select all contacts
			if (string.IsNullOrEmpty(SearchTerm))
			{
				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					if (SelectedTabIndex == 0)
					{
						foreach (var i in db.Contacts.Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete).Select(i => i))
						{
							Contacts.Add(i);
						}
					}
					else
					{
						var busyMapEvents = db.MapEvents.Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] &&
																	!i.IsDelete &&
																	!string.IsNullOrEmpty(i.UserBind) &&
																	i.End >= DateTime.UtcNow);

						if (busyMapEvents.Count() > 0)
						{
							foreach (var item in db.Contacts.Join
								(
									busyMapEvents,
									i => i.Name.ToLowerInvariant(),
									w => w.UserBind.ToLowerInvariant(),
									(i, w) => i
								))
							{
								Contacts.Add(item);
							}
						}
					}
				}
			}

			// Find all equals contacts
			else
			{
				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					if (SelectedTabIndex == 0)
					{
						foreach (var i in db.Contacts
							.Where(i => (i.Name.ToLowerInvariant().Contains(SearchTerm.ToLowerInvariant()) ||
										i.Email.ToLowerInvariant().Contains(SearchTerm.ToLowerInvariant())) &&
										i.EmailOfOwner == (string)localSettings.Values["email"] &&
										!i.IsDelete)
							.Select(i => i))
						{
							if (!SearchSuggestions.Contains(i.Name))
							{
								SearchSuggestions.Add(i.Name);
							}

							Contacts.Add(i);
						}
					}

					else
					{
						var busyMapEvents = db.MapEvents.Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] &&
																	!i.IsDelete &&
																	!string.IsNullOrEmpty(i.UserBind) &&
																	i.UserBind.ToLowerInvariant().Contains(SearchTerm.ToLowerInvariant()) &&
																	i.End >= DateTime.UtcNow);

						if (busyMapEvents.Count() > 0)
						{
							foreach (var mapEvent in busyMapEvents)
							{
								foreach (var item in db.Contacts.Where(i => i.Name.ToLowerInvariant().Contains(mapEvent.UserBind.ToLowerInvariant()) &&
																			i.EmailOfOwner == (string)localSettings.Values["email"] &&
																			!i.IsDelete))
								{
									if (!SearchSuggestions.Contains(item.Name))
									{
										SearchSuggestions.Add(item.Name);
									}

									Contacts.Add(item);
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// User request for searching
		/// </summary>
		/// <param name="sender">Input term</param>
		/// <param name="args">Event args</param>
		public void SearchRequest()
		{
			SearchSuggestions.Clear();

			if (string.IsNullOrEmpty(SearchTerm))
			{
				return;
			}

			if (Contacts != null)
			{
				Contacts.Clear();

				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

					if (SelectedTabIndex == 0)
					{
						foreach (var i in db.Contacts.Where(i => i.Name.ToLower().Contains(SearchTerm.ToLowerInvariant()) && !i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"]).Select(i => i))
						{
							Contacts.Add(i);
						}
					}

					else
					{
						var busyMapEvents = db.MapEvents.Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] &&
																	!i.IsDelete &&
																	!string.IsNullOrEmpty(i.UserBind) &&
																	i.UserBind.ToLowerInvariant().Contains(SearchTerm.ToLowerInvariant()) &&
																	i.End >= DateTime.UtcNow);

						if (busyMapEvents.Count() > 0)
						{
							foreach (var mapEvent in busyMapEvents)
							{
								foreach (var item in db.Contacts.Where(i => i.Name.ToLowerInvariant().Contains(mapEvent.UserBind.ToLowerInvariant()) &&
																			i.EmailOfOwner == (string)localSettings.Values["email"] &&
																			!i.IsDelete))
								{
									Contacts.Add(item);
								}
							}
						}
					}
				}
			}
		}

		#endregion
	}
}