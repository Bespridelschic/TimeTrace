using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using TimeTrace.Model.Events;
using TimeTrace.Model.DBContext;
using TimeTrace.View.MainView.PersonalMapsCreatePages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.ApplicationModel.Resources;
using TimeTrace.Model.Requests;

namespace TimeTrace.ViewModel.MainViewModel.MapEventsViewModel
{
	/// <summary>
	/// View model of project list
	/// </summary>
	public class ProjectViewModel : BaseViewModel, ISearchable<string>
	{
		#region Properties

		/// <summary>
		/// Reseived area
		/// </summary>
		public Area CurrentArea { get; set; }

		private ObservableCollection<Project> currentProjects;
		/// <summary>
		/// Binded collection of buttons
		/// </summary>
		public ObservableCollection<Project> CurrentProjects
		{
			get => currentProjects;
			set
			{
				currentProjects = value;
				OnPropertyChanged();
			}
		}

		private int? selectedProject;
		/// <summary>
		/// Index of selected project
		/// </summary>
		public int? SelectedProject
		{
			get => selectedProject;
			set
			{
				selectedProject = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Localization resource loader
		/// </summary>
		public ResourceLoader ResourceLoader { get; set; }

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		public ProjectViewModel(Area area)
		{
			StartPageViewModel.Instance.SetHeader(Headers.MapEvents);
			CurrentArea = area;

			ResourceLoader = ResourceLoader.GetForCurrentView("ProjectsVM");

			CurrentProjects = new ObservableCollection<Project>();
			SearchSuggestions = new ObservableCollection<string>();

			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

			using (MainDatabaseContext db = new MainDatabaseContext())
			{
				foreach (var i in db.Projects
					.Where(i => i.AreaId == CurrentArea.Id && !i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"])
					.Select(i => i))
				{
					CurrentProjects.Add(i);
				}
			}
		}

		/// <summary>
		/// Creation of new project
		/// </summary>
		public async void ProjectCreateAsync()
		{
			Project newProject = await ProjectCreationDialogAsync(null);

			if (newProject != null)
			{
				// Adding project into database
				using (var db = new MainDatabaseContext())
				{
					db.Projects.Add(newProject);
					db.SaveChanges();
				}

				CurrentProjects.Add(newProject);

				// Synchronization of changes with server
				await StartPageViewModel.Instance.CategoriesSynchronization();
			}
		}

		/// <summary>
		/// Navigate to map event create page
		/// </summary>
		/// <param name="sender">Object sender</param>
		/// <param name="e">Parameters</param>
		public void ProjectSelect()
		{
			if (SelectedProject.HasValue && SelectedProject.Value < CurrentProjects.Count && SelectedProject.Value >= 0)
			{
				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					(Application.Current as App)?.AppFrame.Navigate(typeof(PersonalEventCreatePage), CurrentProjects[SelectedProject.Value]);
				}
			}
		}

		public void ProjectsBulkRemoval()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Removing of selected project
		/// </summary>
		public async void ProjectRemoveAsync()
		{
			if (SelectedProject.HasValue)
			{
				ContentDialog confirmDialog = new ContentDialog()
				{
					Title = ResourceLoader.GetString("/ProjectsVM/ConfirmAction"),
					Content = $"{ResourceLoader.GetString("/ProjectsVM/ConfirmDeletion")} {CurrentProjects[SelectedProject.Value].Name}?\n" +
							  $"{ResourceLoader.GetString("/ProjectsVM/DeletionWarning")}",
					PrimaryButtonText = ResourceLoader.GetString("/ProjectsVM/Remove"),
					CloseButtonText = ResourceLoader.GetString("/ProjectsVM/Cancel"),
					DefaultButton = ContentDialogButton.Close,
				};

				var result = await confirmDialog.ShowAsync();

				if (result == ContentDialogResult.Primary)
				{
					using (MainDatabaseContext db = new MainDatabaseContext())
					{
						// Remove all events in this project
						foreach (var mapEvent in db.MapEvents.Where(i => i.ProjectId == CurrentProjects[SelectedProject.Value].Id))
						{
							db.MapEvents.FirstOrDefault(i => i.Id == mapEvent.Id).IsDelete = true;
						}

						// Remove Project from database
						db.Projects.FirstOrDefault(i => i.Id == CurrentProjects[SelectedProject.Value].Id).IsDelete = true;

						db.SaveChanges();

						await new MessageDialog($"{ResourceLoader.GetString("/ProjectsVM/Project")} {CurrentProjects[SelectedProject.Value].Name} {ResourceLoader.GetString("/ProjectsVM/RemoveInformation")}",
							ResourceLoader.GetString("/ProjectsVM/Success")).ShowAsync();

						// Remove Area from UI panel
						CurrentProjects.Remove(CurrentProjects[SelectedProject.Value]);

						// Synchronization of changes with server
						await StartPageViewModel.Instance.CategoriesSynchronization();
					}
				}
			}
		}

		/// <summary>
		/// Edit of selected project
		/// </summary>
		public async void ProjectEditAsync()
		{
			if (SelectedProject.HasValue)
			{
				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					// Get edited Project object
					var editedProject = await ProjectCreationDialogAsync(CurrentProjects[SelectedProject.Value]);

					if (editedProject == null)
					{
						return;
					}

					// If project was edit
					if (editedProject.Name != CurrentProjects[SelectedProject.Value].Name || editedProject.Description != CurrentProjects[SelectedProject.Value].Description)
					{
						CurrentProjects[SelectedProject.Value].Name = editedProject.Name.Trim();
						CurrentProjects[SelectedProject.Value].Description = editedProject.Description.Trim();

						CurrentProjects[SelectedProject.Value].UpdateAt = DateTime.UtcNow;

						try
						{
							// Update project in database
							db.Projects.Update(CurrentProjects[SelectedProject.Value]);
							db.SaveChanges();
						}
						catch (Exception)
						{
							await new MessageDialog(ResourceLoader.GetString("/ProjectsVM/UndefinedError"), ResourceLoader.GetString("/ProjectsVM/ErrorChangindData")).ShowAsync();
						}

						// Update current button
						int selectedIndex = SelectedProject.Value;
						var tempProject = CurrentProjects[selectedIndex];
						CurrentProjects.RemoveAt(selectedIndex);
						CurrentProjects.Insert(selectedIndex, tempProject);

						// Synchronization of changes with server
						await StartPageViewModel.Instance.CategoriesSynchronization();
					}
				}
			}
		}

		/// <summary>
		/// Marking events of selected project as public
		/// </summary>
		public async void MarkInnerEventsAsPublicAsync()
		{
			if (!SelectedProject.HasValue)
			{
				return;
			}

			using (var db = new MainDatabaseContext())
			{
				ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

				int eventsTotalCount = db.MapEvents
					.Where(i => i.ProjectId == CurrentProjects[SelectedProject.Value].Id
						&& i.End >= DateTime.UtcNow
						&& !i.IsDelete
						&& i.EmailOfOwner == (string)localSettings.Values["email"])
					.Count();

				if (eventsTotalCount < 1)
				{
					await new MessageDialog(ResourceLoader.GetString("NoActualEvents"), ResourceLoader.GetString("Error")).ShowAsync();
					return;
				}

				// List of selected ID's for mark as public
				List<string> publicSelectedEvents = new List<string>(eventsTotalCount);

				// List of selected ID's for mark as private
				List<string> privateSelectedEvents = new List<string>(eventsTotalCount);

				var mainPanel = new StackPanel();
				foreach (var item in db.MapEvents
					.Where(i => i.ProjectId == CurrentProjects[SelectedProject.Value].Id
						&& i.End >= DateTime.UtcNow
						&& !i.IsDelete
						&& i.EmailOfOwner == (string)localSettings.Values["email"]))
				{
					StackPanel project = new StackPanel();
					project.Children.Add(new TextBlock() { Text = item.Name, TextTrimming = TextTrimming.CharacterEllipsis });

					string description = string.IsNullOrEmpty(item.Description) ? ResourceLoader.GetString("NoDescription") : item.Description;
					ToolTip toolTip = new ToolTip()
					{
						Placement = Windows.UI.Xaml.Controls.Primitives.PlacementMode.Mouse,
						Content = new TextBlock()
						{
							Text = $"{ResourceLoader.GetString("Start")}: {item.Start.ToLocalTime()}\n" +
								$"{ResourceLoader.GetString("Name")}: {item.Name}\n" +
								$"{ResourceLoader.GetString("Description")}: {description}\n" +
								$"{ResourceLoader.GetString("Creator")}: {item.EmailOfOwner}",
							FontSize = 15,
						},
					};
					ToolTipService.SetToolTip(project, toolTip);

					if (item.IsPublic)
					{
						publicSelectedEvents.Add(item.Id);
					}
					else
					{
						privateSelectedEvents.Add(item.Id);
					}

					var checkBox = new CheckBox()
					{
						Content = project,
						Tag = item.Id,
						IsChecked = item.IsPublic,
					};
					checkBox.Checked += (i, e) =>
					{
						if (i is CheckBox chBox)
						{
							publicSelectedEvents.Add((string)chBox.Tag);
							privateSelectedEvents.Remove((string)chBox.Tag);
						}
					};
					checkBox.Unchecked += (i, e) =>
					{
						if (i is CheckBox chBox)
						{
							publicSelectedEvents.Remove((string)chBox.Tag);
							privateSelectedEvents.Add((string)chBox.Tag);
						}
					};

					mainPanel.Children.Add(checkBox);
				}

				ScrollViewer scroll = new ScrollViewer()
				{
					VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
					Content = mainPanel
				};

				ContentDialog getPublicDialog = new ContentDialog()
				{
					Title = ResourceLoader.GetString("PublicityChanging"),
					Content = scroll,
					PrimaryButtonText = ResourceLoader.GetString("Change"),
					CloseButtonText = ResourceLoader.GetString("Later"),
					DefaultButton = ContentDialogButton.Primary
				};

				var res = await getPublicDialog.ShowAsync();

				if (res == ContentDialogResult.Primary)
				{
					foreach (var item in publicSelectedEvents)
					{
						db.MapEvents.First(i => i.Id == item).IsPublic = true;
						db.MapEvents.First(i => i.Id == item).UpdateAt = DateTime.UtcNow;
						Debug.WriteLine($"Публично: {db.MapEvents.First(i => i.Id == item).Name}");
					}

					foreach (var item in privateSelectedEvents)
					{
						db.MapEvents.First(i => i.Id == item).IsPublic = false;
						db.MapEvents.First(i => i.Id == item).UpdateAt = DateTime.UtcNow;
						Debug.WriteLine($"Скрыто: {db.MapEvents.First(i => i.Id == item).Name}");
					}

					db.SaveChanges();

					// Synchronization last changes with server
					await StartPageViewModel.Instance.CategoriesSynchronization();
				}
			}
		}

		/// <summary>
		/// Send project to contacts
		/// </summary>
		public async void SendProjectToContacts()
		{
			if (!StartPageViewModel.Instance.InternetFeaturesEnable)
			{
				await new MessageDialog(ResourceLoader.GetString("NoInternet"), ResourceLoader.GetString("ProjectSentError")).ShowAsync();
				return;
			}

			if (SelectedProject.HasValue)
			{
				ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

				using (var db = new MainDatabaseContext())
				{
					// If no contacts
					if (db.Contacts.Count(i => !i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"]) < 1)
					{
						await new MessageDialog(ResourceLoader.GetString("NoContacts"), ResourceLoader.GetString("ProjectSentError")).ShowAsync();
						return;
					}

					// If no actual events in project
					if (db.MapEvents
							.Count(i => i.ProjectId == CurrentProjects[SelectedProject.Value].Id
								&& !i.IsDelete
								&& i.End >= DateTime.UtcNow)
							< 1)
					{
						await new MessageDialog(ResourceLoader.GetString("NoActualEvents"), ResourceLoader.GetString("ProjectSentError")).ShowAsync();
						return;
					}

					// List of selected ID's contacts
					List<string> selectedContacts = new List<string>(db.Contacts.Count(i => !i.IsDelete));

					var mainPanel = new StackPanel();
					foreach (var item in db.Contacts.Where(i => !i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"]))
					{
						StackPanel project = new StackPanel();
						project.Children.Add(new TextBlock() { Text = item.Email, TextTrimming = TextTrimming.CharacterEllipsis });

						string contactName = string.IsNullOrEmpty(item.Name) ? ResourceLoader.GetString("NoName") : item.Name;
						ToolTip toolTip = new ToolTip()
						{
							Placement = Windows.UI.Xaml.Controls.Primitives.PlacementMode.Mouse,
							Content = new TextBlock()
							{
								Text = $"{item.Email}\n" +
									$"{ResourceLoader.GetString("FirstName")}: {contactName}\n" +
									$"{ResourceLoader.GetString("Added")}: {item.CreateAt.Value.ToLocalTime()}",
								FontSize = 15,
							},
						};
						ToolTipService.SetToolTip(project, toolTip);

						var checkBox = new CheckBox()
						{
							Content = project,
							Tag = item.Id,
						};
						checkBox.Checked += (i, e) =>
						{
							if (i is CheckBox chBox)
							{
								selectedContacts.Add((string)chBox.Tag);
							}
						};
						checkBox.Unchecked += (i, e) =>
						{
							if (i is CheckBox chBox)
							{
								selectedContacts.Remove((string)chBox.Tag);
							}
						};

						mainPanel.Children.Add(checkBox);
					}

					ScrollViewer scroll = new ScrollViewer()
					{
						VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
						Content = mainPanel
					};

					ContentDialog getPublicDialog = new ContentDialog()
					{
						Title = ResourceLoader.GetString("SendingProjectToContacts"),
						Content = scroll,
						PrimaryButtonText = ResourceLoader.GetString("Send"),
						CloseButtonText = ResourceLoader.GetString("Later"),
						DefaultButton = ContentDialogButton.Primary
					};

					var res = await getPublicDialog.ShowAsync();

					if (res == ContentDialogResult.Primary)
					{
						if (selectedContacts.Count < 1)
						{
							await new MessageDialog(ResourceLoader.GetString("NoSelectedContacts"), ResourceLoader.GetString("ProjectSentError")).ShowAsync();
							return;
						}

						try
						{
							int result = await InternetRequests.SendInvitationToContact(selectedContacts, CurrentProjects[SelectedProject.Value]);

							if (result == 0)
							{
								await new MessageDialog(ResourceLoader.GetString("InvitationSentSuccessfully"), ResourceLoader.GetString("Success")).ShowAsync();
							}
							else
							{
								await new MessageDialog(ResourceLoader.GetString("InvitationSentError"), ResourceLoader.GetString("ProjectSentError")).ShowAsync();
							}
						}
						catch (Exception)
						{
							await new MessageDialog(ResourceLoader.GetString("InvitationSentError"), ResourceLoader.GetString("ProjectSentError")).ShowAsync();
						}
					}
				}
			}
		}

		/// <summary>
		/// Creation of edition project dialog
		/// </summary>
		/// <param name="project"><see cref="Project"/> object</param>
		/// <returns>Edited object of project or new object if created</returns>
		private async Task<Project> ProjectCreationDialogAsync(Project project)
		{
			#region Text boxes

			TextBox name = new TextBox()
			{
				Header = ResourceLoader.GetString("/ProjectsVM/Name"),
				PlaceholderText = (project == null)
									? ResourceLoader.GetString("/ProjectsVM/NewProjectName")
									: ResourceLoader.GetString("/ProjectsVM/CurrentProjectName"),
				Text = project?.Name ?? string.Empty,
				Margin = new Thickness(0, 0, 0, 10),
				MaxLength = 30,
				SelectionStart = project?.Name?.Length ?? 0,
			};

			TextBox description = new TextBox()
			{
				Header = ResourceLoader.GetString("/ProjectsVM/Description"),
				PlaceholderText = ResourceLoader.GetString("/ProjectsVM/ShortDescription"),
				Text = project?.Description ?? string.Empty,
				Margin = new Thickness(0, 0, 0, 10),
				MaxLength = 50,
			};

			#endregion

			StackPanel panel = new StackPanel();
			panel.Children.Add(name);
			panel.Children.Add(description);

			ContentDialog dialog = new ContentDialog()
			{
				Title = (project == null) ? ResourceLoader.GetString("/ProjectsVM/CreatingNewProject") : ResourceLoader.GetString("/ProjectsVM/ChangingProject"),
				Content = panel,
				PrimaryButtonText = (project == null) ? ResourceLoader.GetString("/ProjectsVM/Create") : ResourceLoader.GetString("/ProjectsVM/Change"),
				CloseButtonText = ResourceLoader.GetString("/ProjectsVM/Later"),
				DefaultButton = ContentDialogButton.Primary,
			};

			var result = await dialog.ShowAsync();

			if (result == ContentDialogResult.Primary)
			{
				if (project == null && string.IsNullOrEmpty(name.Text?.Trim()))
				{
					await new MessageDialog(ResourceLoader.GetString("/ProjectsVM/CreatingNewProjectErrorNameNotEntered"),
						ResourceLoader.GetString("/ProjectsVM/AddingNewProjectError")).ShowAsync();

					return null;
				}
				else
				{
					if (string.IsNullOrEmpty(name.Text?.Trim()))
					{
						await new MessageDialog(ResourceLoader.GetString("/ProjectsVM/ChangingProjectErrorNameNotEntered"),
							ResourceLoader.GetString("/ProjectsVM/ChangingProjectError")).ShowAsync();

						return null;
					}
				}

				// New area object for adding into Database
				return new Project()
				{
					Name = name.Text.Trim(),
					Description = description.Text.Trim(),
					AreaId = CurrentArea.Id,
					Color = CurrentArea.Color,
				};
			}
			else
			{
				return project;
			}
		}

		#region Searching projects

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
		public void DynamicSearch()
		{
			if (CurrentProjects == null) return;

			// Remove all projects for adding relevant filter
			CurrentProjects.Clear();
			SearchSuggestions.Clear();

			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

			// Select all projects
			if (string.IsNullOrEmpty(SearchTerm))
			{
				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					foreach (var i in db.Projects
						.Where(i => i.AreaId == CurrentArea.Id
							&& i.EmailOfOwner == (string)localSettings.Values["email"]
							&& !i.IsDelete)

						.Select(i => i))
					{
						CurrentProjects.Add(i);
					}
				}
			}

			else
			{
				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					foreach (var i in db.Projects
						.Where(i => i.Name.ToLowerInvariant().Contains(SearchTerm.ToLowerInvariant()) &&
									i.AreaId == CurrentArea.Id &&
									i.EmailOfOwner == (string)localSettings.Values["email"] &&
									!i.IsDelete)
						.Select(i => i))
					{
						if (!SearchSuggestions.Contains(i.Name))
						{
							SearchSuggestions.Add(i.Name);
						}

						CurrentProjects.Add(i);
					}
				}
			}
		}

		/// <summary>
		/// User request for searching
		/// </summary>
		public void SearchRequest()
		{
			SearchSuggestions.Clear();

			if (string.IsNullOrEmpty(SearchTerm))
			{
				return;
			}

			if (CurrentProjects != null)
			{
				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

					foreach (var i in db.Projects
						.Where(i => i.Name.ToLowerInvariant().Contains(SearchTerm.ToLowerInvariant()) &&
									i.AreaId == CurrentArea.Id &&
									!i.IsDelete
									&& i.EmailOfOwner == (string)localSettings.Values["email"])
						.Select(i => i))
					{
						CurrentProjects.Add(i);
					}
				}
			}
		}

		#endregion
	}
}