using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TimeTrace.Model.DBContext;
using TimeTrace.Model.Events;
using TimeTrace.Model.Requests;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace TimeTrace.ViewModel.MainViewModel
{
	public class InvitationsViewModel : BaseViewModel
	{
		#region Properties

		private ObservableCollection<Invitation> myInvitations;
		/// <summary>
		/// Collection of invitations sent for my contacts by me
		/// </summary>
		public ObservableCollection<Invitation> MyInvitations
		{
			get => myInvitations;
			set
			{
				myInvitations = value;
				OnPropertyChanged();
			}
		}

		private ObservableCollection<Invitation> invitationsForMe;
		/// <summary>
		/// Collection of invitations for me by contacts
		/// </summary>
		public ObservableCollection<Invitation> InvitationsForMe
		{
			get => invitationsForMe;
			set
			{
				invitationsForMe = value;
				OnPropertyChanged();
			}
		}

		private ObservableCollection<Project> projects;
		/// <summary>
		/// List of invited projects
		/// </summary>
		public ObservableCollection<Project> Projects
		{
			get => projects;
			set
			{
				projects = value;
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

		private List<Invitation> selectedInvitations;
		/// <summary>
		/// Collection of multiple invitations selection
		/// </summary>
		public List<Invitation> SelectedInvitations
		{
			get => selectedInvitations;
			set
			{
				selectedInvitations = value;
				OnPropertyChanged();
			}
		}

		private int? selectedIndexOfMyInvitations;
		/// <summary>
		/// Index of selected item in my invitations list
		/// </summary>
		public int? SelectedIndexOfMyInvitations
		{
			get => selectedIndexOfMyInvitations;
			set
			{
				selectedIndexOfMyInvitations = value;
				OnPropertyChanged();
			}
		}

		private int? selectedIndexOfInvitationsForMe;
		/// <summary>
		/// Index of selected item in list of invitations for me
		/// </summary>
		public int? SelectedIndexOfInvitationsForMe
		{
			get => selectedIndexOfInvitationsForMe;
			set
			{
				selectedIndexOfInvitationsForMe = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Localization resource loader
		/// </summary>
		public ResourceLoader ResourceLoader { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Get fresh info from server
		/// </summary>
		/// <returns></returns>
		private async Task InitializationDataAsync()
		{
			if (MyInvitations == null)
			{
				MyInvitations = new ObservableCollection<Invitation>(await InternetRequests.GetMyInvitations());
			}
			else
			{
				MyInvitations.Clear();
				foreach (var item in await InternetRequests.GetMyInvitations())
				{
					MyInvitations.Add(item);
				}
			}

			if (InvitationsForMe == null)
			{
				var invitations = await InternetRequests.GetInvitationsForMe();
				InvitationsForMe = new ObservableCollection<Invitation>(invitations.invitations);
				Projects = new ObservableCollection<Project>(invitations.projects);
			}
			else
			{
				var invitations = await InternetRequests.GetInvitationsForMe();

				InvitationsForMe.Clear();
				foreach (var item in invitations.invitations)
				{
					InvitationsForMe.Add(item);
				}

				Projects.Clear();
				foreach (var item in invitations.projects)
				{
					Projects.Add(item);
				}
			}
		}

		/// <summary>
		/// Standart constructor
		/// </summary>
		public InvitationsViewModel()
		{
			ResourceLoader = ResourceLoader.GetForCurrentView("InvitationsVM");
			StartPageViewModel.Instance.SetHeader(Headers.Invitations);

			MultipleSelection = ListViewSelectionMode.Single;
			SelectedInvitations = new List<Invitation>();

			InitializationDataAsync().GetAwaiter();
		}

		#endregion

		/// <summary>
		/// Reload all lists with new information
		/// </summary>
		public async void RefreshAsync()
		{
			await InitializationDataAsync();
		}

		/// <summary>
		/// Add project and its events to local
		/// </summary>
		public async void AddProjectAsync()
		{
			if (SelectedIndexOfInvitationsForMe.HasValue)
			{
				using (var db = new MainDatabaseContext())
				{
					ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

					if (db.Areas.Count(i => !i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"]) < 1)
					{
						await new MessageDialog(ResourceLoader.GetString("NoCalendars"), ResourceLoader.GetString("ErrorAddingProject")).ShowAsync();
						return;
					}

					ComboBox calendarsList = new ComboBox()
					{
						Header = ResourceLoader.GetString("Calendar"),
						Width = 250,
					};

					var calendars = db.Areas.Where(i => !i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"]).ToList();

					calendarsList.ItemsSource = calendars.Select(i => i.Name);
					calendarsList.SelectedIndex = 0;

					ContentDialog mainDialog = new ContentDialog()
					{
						Title = ResourceLoader.GetString("SelectCalendar"),
						Content = calendarsList,
						PrimaryButtonText = ResourceLoader.GetString("Add"),
						CloseButtonText = ResourceLoader.GetString("Later"),
						DefaultButton = ContentDialogButton.Primary
					};

					if (await mainDialog.ShowAsync() == ContentDialogResult.Primary)
					{
						var invites = await InternetRequests.AcceptInvite(Projects[SelectedIndexOfInvitationsForMe.Value].Id);

						invites.project.Color = calendars[calendarsList.SelectedIndex].Color;
						invites.project.AreaId = calendars[calendarsList.SelectedIndex].Id;
						invites.project.EmailOfOwner = calendars[calendarsList.SelectedIndex].EmailOfOwner;
						invites.project.CreateAt = invites.project.UpdateAt = DateTime.UtcNow;

						var events = new List<MapEvent>(invites.events.Count);

						foreach (var item in invites.events)
						{
							var temp = db.MapEvents.FirstOrDefault(i => i.Id == item.Id);

							// Add new event to list if absent in database
							if (temp == null)
							{
								events.Add(item);
							}
						}

						foreach (var item in events)
						{
							item.Color = calendars[calendarsList.SelectedIndex].Color;
						}

						if (events.Count > 0)
						{
							db.MapEvents.AddRange(events);
						}

						if (db.Projects.Count(i => i.Id == invites.project.Id) < 1)
						{
							invites.project.AreaId = calendars[calendarsList.SelectedIndex].Id;
							invites.project.Color = calendars[calendarsList.SelectedIndex].Color;
							db.Projects.Add(invites.project);
						}

						db.SaveChanges();

						Projects.RemoveAt(SelectedIndexOfInvitationsForMe.Value);
					}
				}
			}
		}

		/// <summary>
		/// Deny invitation for project
		/// </summary>
		public async void DenyProjectAsync()
		{
			if (SelectedIndexOfInvitationsForMe.HasValue)
			{
				ContentDialog mainDialog = new ContentDialog()
				{
					Title = ResourceLoader.GetString("RefusalOfTheProject"),
					Content = ResourceLoader.GetString("AreYouSureCancelTheInvitation"),
					PrimaryButtonText = ResourceLoader.GetString("Refuse"),
					CloseButtonText = ResourceLoader.GetString("Cancel"),
					DefaultButton = ContentDialogButton.Primary
				};

				if (await mainDialog.ShowAsync() == ContentDialogResult.Primary)
				{
					try
					{
						int result = await InternetRequests.DenyInvite(Projects[SelectedIndexOfInvitationsForMe.Value].Id);
						if (result == 0)
						{
							Projects.RemoveAt(SelectedIndexOfInvitationsForMe.Value);
						}
						else
						{
							await new MessageDialog(ResourceLoader.GetString("ErrorRefusingInvitations"), ResourceLoader.GetString("Error")).ShowAsync();
						}
					}
					catch (Exception)
					{
						await new MessageDialog(ResourceLoader.GetString("ErrorRefusingInvitations"), ResourceLoader.GetString("Error")).ShowAsync();
					}
				}
			}
		}

		#region Select on right click

		/// <summary>
		/// Selecting item of list with right click
		/// </summary>
		/// <param name="sender">Sender list</param>
		/// <param name="e">Parameter</param>
		public void SelectMyInvitationOnRightClick(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
		{
			ListView listView = (ListView)sender;
			if (((Windows.UI.Xaml.FrameworkElement)e.OriginalSource).DataContext is Invitation selectedItem)
			{
				SelectedIndexOfMyInvitations = MyInvitations.IndexOf(selectedItem);
			}
		}

		/// <summary>
		/// Selecting item of list with right click
		/// </summary>
		/// <param name="sender">Sender list</param>
		/// <param name="e">Parameter</param>
		public void SelectInvitationForMeOnRightClick(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
		{
			ListView listView = (ListView)sender;
			if (((Windows.UI.Xaml.FrameworkElement)e.OriginalSource).DataContext is Project selectedItem)
			{
				SelectedIndexOfInvitationsForMe = Projects.IndexOf(selectedItem);
			}
		}

		#endregion

		#region Cancel my invites

		/// <summary>
		/// Cancel one of my invitation
		/// </summary>
		public async void CancelInvitationAsync()
		{
			if (SelectedIndexOfMyInvitations.HasValue)
			{
				string projectName;
				using (var db = new MainDatabaseContext())
				{
					var project = db.Projects.FirstOrDefault(i => i.Id == MyInvitations[SelectedIndexOfMyInvitations.Value].ProjectId);
					if (project != null)
					{
						projectName = project.Name;
					}
					else
					{
						projectName = ResourceLoader.GetString("Undefined");
					}
				}

				ContentDialog dialog = new ContentDialog()
				{
					Title = ResourceLoader.GetString("ConfirmAction"),
					Content = $"{ResourceLoader.GetString("ConfirmationOfCancellationOfInvitation")} {projectName}?",
					PrimaryButtonText = ResourceLoader.GetString("Unsubscribe"),
					CloseButtonText = ResourceLoader.GetString("Cancel"),
					DefaultButton = ContentDialogButton.Close
				};

				if (await dialog.ShowAsync() == ContentDialogResult.Primary)
				{
					try
					{
						var result = await InternetRequests.UnsubscribeInvitations(MyInvitations[SelectedIndexOfMyInvitations.Value].Id);
						if (result == 0)
						{
							await new MessageDialog(ResourceLoader.GetString("InvitationCanceled"), ResourceLoader.GetString("Success")).ShowAsync();
							return;
						}
						else
						{
							await new MessageDialog(ResourceLoader.GetString("ErrorCancelingInvitations"), ResourceLoader.GetString("Error")).ShowAsync();
							return;
						}
					}
					catch (Exception)
					{
						await new MessageDialog(ResourceLoader.GetString("ErrorCancelingInvitations"), ResourceLoader.GetString("Error")).ShowAsync();
					}
				}
			}
		}

		/// <summary>
		/// Cancel of my several invitations
		/// </summary>
		public async void CancelInvitationsAsync()
		{
			if (MultipleSelection == ListViewSelectionMode.Single)
			{
				MultipleSelection = ListViewSelectionMode.Multiple;
			}
			else
			{
				try
				{
					if (SelectedInvitations == null || SelectedInvitations.Count <= 0)
					{
						MultipleSelection = ListViewSelectionMode.Single;
						return;
					}

					string removedInvitation = string.Empty;

					using (var db = new MainDatabaseContext())
					{
						foreach (var item in SelectedInvitations.Join(db.Projects, i => i.ProjectId, w => w.Id, (i, w) => w))
						{
							removedInvitation += $"{item.Name}\n";
						}
					}

					ScrollViewer scrollViewer = new ScrollViewer()
					{
						VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
						Content = new TextBlock() { Text = removedInvitation },
					};

					StackPanel mainPanel = new StackPanel()
					{
						Margin = new Windows.UI.Xaml.Thickness(0, 0, 0, 10),
					};
					mainPanel.Children.Add(new TextBlock() { Text = ResourceLoader.GetString("ConfirmationOfCancellationOfInvitations") });
					mainPanel.Children.Add(scrollViewer);

					ContentDialog contentDialog = new ContentDialog()
					{
						Title = ResourceLoader.GetString("ConfirmAction"),
						Content = mainPanel,
						PrimaryButtonText = ResourceLoader.GetString("Unsubscribe"),
						CloseButtonText = ResourceLoader.GetString("Cancel"),
						DefaultButton = ContentDialogButton.Close
					};

					var result = await contentDialog.ShowAsync();

					if (result == ContentDialogResult.Primary)
					{
						foreach (var item in SelectedInvitations)
						{
							await InternetRequests.UnsubscribeInvitations(item.Id);
							MyInvitations.Remove(item);
						}
					}
				}
				catch (Exception)
				{
					await new MessageDialog(ResourceLoader.GetString("ErrorCancelingInvitations"), ResourceLoader.GetString("Error")).ShowAsync();
				}
				finally
				{
					MultipleSelection = ListViewSelectionMode.Single;
				}
			}
		}

		#endregion

		/// <summary>
		/// Select several invitations
		/// </summary>
		/// <param name="sender">ListView</param>
		/// <param name="e">Parameters</param>
		public void MultipleInvitationsSelection(object sender, SelectionChangedEventArgs e)
		{
			ListView listView = sender as ListView;
			SelectedInvitations = new List<Invitation>();

			foreach (Invitation item in listView.SelectedItems)
			{
				SelectedInvitations.Add(item);
			};
		}
	}
}