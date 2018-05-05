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

namespace TimeTrace.ViewModel.MainViewModel.MapEventsViewModel
{
	/// <summary>
	/// View model of project list
	/// </summary>
	public class ProjectViewModel : BaseViewModel
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

		private ObservableCollection<string> projectSuggestList;
		/// <summary>
		/// Filter tips
		/// </summary>
		public ObservableCollection<string> ProjectSuggestList
		{
			get => projectSuggestList;
			set
			{
				projectSuggestList = value;
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

		private string projectMapEventsToday;
		/// <summary>
		/// Count of today's map events
		/// </summary>
		public string ProjectMapEventsToday
		{
			get => projectMapEventsToday;
			set
			{
				projectMapEventsToday = value;
				OnPropertyChanged();
			}
		}

		private string projectMapEventsTotal;
		/// <summary>
		/// Count of total map events selected project
		/// </summary>
		public string ProjectMapEventsTotal
		{
			get => projectMapEventsTotal;
			set
			{
				projectMapEventsTotal = value;
				OnPropertyChanged();
			}
		}

		private DateTime nearMapEvent;
		/// <summary>
		/// How quickly came the nearest event
		/// </summary>
		public DateTime NearMapEvent
		{
			get => nearMapEvent;
			set
			{
				nearMapEvent = value;
				OnPropertyChanged();
			}
		}

		private string projectMapEventsCompleted;
		/// <summary>
		/// Total count of completed map events
		/// </summary>
		public string ProjectMapEventsCompleted
		{
			get => projectMapEventsCompleted;
			set
			{
				projectMapEventsCompleted = value;
				OnPropertyChanged();
			}
		}

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		public ProjectViewModel(Area area)
		{
			StartPageViewModel.Instance.SetHeader(StartPageViewModel.Headers.MapEvents);
			CurrentArea = area;

			CurrentProjects = new ObservableCollection<Project>();
			ProjectSuggestList = new ObservableCollection<string>();

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
		public async void ProjectCreate()
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
					Title = "Подтверждение удаления",
					Content = $"Вы уверены что хотите удалить проект {CurrentProjects[SelectedProject.Value].Name}?\n" +
							  $"Удаление приведен к потере всех событий внутри проекта!",
					PrimaryButtonText = "Удалить",
					CloseButtonText = "Отмена",
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

						await new MessageDialog($"Проект {CurrentProjects[SelectedProject.Value].Name} со всеми событиями внутри был удалён",
							"Операция завершена успешно").ShowAsync();

						// Remove Area from UI panel
						CurrentProjects.Remove(CurrentProjects[SelectedProject.Value]);
					}
				}
			}
		}

		/// <summary>
		/// Edit of selected project
		/// </summary>
		public async void ProjectEditAsync(object sender, RoutedEventArgs e)
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
							await new MessageDialog("Не предвиденная ошибка, попробуйте позже", "Ошибка при изменении данных").ShowAsync();
						}

						// Update current button
						int selectedIndex = SelectedProject.Value;
						var tempProject = CurrentProjects[selectedIndex];
						CurrentProjects.RemoveAt(selectedIndex);
						CurrentProjects.Insert(selectedIndex, tempProject);
					}
				}
			}
		}

		#region Searching projects

		/// <summary>
		/// Filtration of input filters
		/// </summary>
		/// <param name="sender">Input filter</param>
		/// <param name="args">Event args</param>
		public void ProjectFilter(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (CurrentProjects == null) return;

			if (args.CheckCurrent())
			{
				ProjectSuggestList.Clear();
			}

			// Remove all buttons for adding relevant filter
			CurrentProjects.Clear();

			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

			// Select all projects
			if (string.IsNullOrEmpty(sender.Text))
			{
				ProjectSuggestList.Clear();

				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					foreach (var i in db.Projects.Where(i => i.AreaId == CurrentArea.Id && i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete).Select(i => i))
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
						.Where(i => i.Name.ToLowerInvariant().Contains(sender.Text.ToLowerInvariant()) &&
									i.AreaId == CurrentArea.Id &&
									i.EmailOfOwner == (string)localSettings.Values["email"] &&
									!i.IsDelete)
						.Select(i => i))
					{
						if (!ProjectSuggestList.Contains(i.Name))
						{
							ProjectSuggestList.Add(i.Name);
						}

						CurrentProjects.Add(i);
					}
				}
			}
		}

		/// <summary>
		/// Click on Find button
		/// </summary>
		/// <param name="sender">Object</param>
		/// <param name="args">Args</param>
		public void ProjectFilterQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			ProjectSuggestList.Clear();

			if (string.IsNullOrEmpty(args.QueryText))
			{
				return;
			}

			if (CurrentProjects != null)
			{
				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
					var term = args.QueryText.ToLower();
					foreach (var i in db.Projects
						.Where(i => i.Name.ToLowerInvariant().Contains(term) &&
									i.AreaId == CurrentArea.Id &&
									!i.IsDelete
									&& i.EmailOfOwner == (string)localSettings.Values["email"]).Select(i => i))
					{
						CurrentProjects.Add(i);
					}
				}
			}
		}

		#endregion

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
				Header = "Название",
				PlaceholderText = "Название нового проекта",
				Text = project?.Name ?? string.Empty,
				Margin = new Thickness(0, 0, 0, 10),
				MaxLength = 30,
				SelectionStart = project?.Name?.Length ?? 0,
			};

			TextBox description = new TextBox()
			{
				Header = "Описание",
				PlaceholderText = "Краткое описание проекта",
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
				Title = (project == null) ? "Создание нового проекта" : "Редактирование проекта",
				Content = panel,
				PrimaryButtonText = (project == null) ? "Создать" : "Изменить",
				CloseButtonText = "Отложить",
				DefaultButton = ContentDialogButton.Primary,
			};

			var result = await dialog.ShowAsync();

			if (result == ContentDialogResult.Primary)
			{
				if (project == null && string.IsNullOrEmpty(name.Text?.Trim()))
				{
					await new MessageDialog("Не заполнено имя для нового проекта", "Ошибка добавления нового проекта").ShowAsync();

					return null;
				}
				else
				{
					if (string.IsNullOrEmpty(name.Text?.Trim()))
					{
						await new MessageDialog("Не заполнено имя изменяемого проекта", "Ошибка изменения проекта").ShowAsync();

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
	}
}