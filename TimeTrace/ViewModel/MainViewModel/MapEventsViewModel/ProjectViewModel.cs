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

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		public ProjectViewModel(Area area)
		{
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
			if (SelectedProject.HasValue)
			{
				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					(Application.Current as App)?.AppFrame.Navigate(typeof(PersonalEventCreatePage), CurrentProjects[SelectedProject.Value]);
				}
			}
		}

		public void ProjectsBulkRemoval()
		{
			
		}

		/// <summary>
		/// Removing of selected project
		/// </summary>
		public async void ProjectRemoveAsync(object sender, RoutedEventArgs e)
		{
			using (MainDatabaseContext db = new MainDatabaseContext())
			{
				// Get selected Area
				var selectedProject = db.Projects.First(i => i.Id == (string)(sender as MenuFlyoutItem).Tag);

				ContentDialog confirmDialog = new ContentDialog()
				{
					Title = "Подтверждение удаления",
					Content = $"Вы уверены что хотите удалить проект {selectedProject.Name}?\n" +
							  $"Удаление приведен к потере всех событий внутри проекта!",
					PrimaryButtonText = "Удалить",
					CloseButtonText = "Отмена",
					DefaultButton = ContentDialogButton.Close,
				};

				var result = await confirmDialog.ShowAsync();

				if (result == ContentDialogResult.Primary)
				{
					// Remove all events in this project
					foreach (var mapEvent in db.MapEvents.Where(i => i.Id == selectedProject.Id))
					{
						db.MapEvents.FirstOrDefault(i => i.Id == mapEvent.Id).IsDelete = true;
					}

					// Remove Project from database
					db.Projects.FirstOrDefault(i => i.Id == selectedProject.Id).IsDelete = true;

					db.SaveChanges();

					await new MessageDialog($"Проект {selectedProject.Name} со всеми событиями внутри был удалён",
						"Операция завершена успешно").ShowAsync();
				}
			}
		}

		/// <summary>
		/// Edit of selected project
		/// </summary>
		public async void ProjectEditAsync(object sender, RoutedEventArgs e)
		{
			using (MainDatabaseContext db = new MainDatabaseContext())
			{
				// Get selected Project
				var selectedProject = db.Projects.First(i => i.Id == (string)(sender as MenuFlyoutItem).Tag);

				// Get edited Project object
				var editedProject = await ProjectCreationDialogAsync(selectedProject);

				if (editedProject == null)
				{
					return;
				}

				// If project was edit
				if (editedProject.Name != selectedProject.Name || editedProject.Description != selectedProject.Description || editedProject.Color != selectedProject.Color)
				{
					selectedProject.Name = editedProject.Name;
					selectedProject.Description = editedProject.Description;
					selectedProject.Color = editedProject.Color;

					selectedProject.UpdateAt = DateTime.UtcNow;

					// Update project in database
					db.Projects.Update(selectedProject);
					db.SaveChanges();

					// Update current button
					/*var index = CurrentProjects.IndexOf(CurrentProjects.First(i => (string)i.Tag == selectedProject.Id));
					CurrentProjects.RemoveAt(index);
					CurrentProjects.Insert(index, NewProjectButtonCreate(selectedProject));*/
				}
			}
		}

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

			// Select all calendars
			if (string.IsNullOrEmpty(sender.Text))
			{
				ProjectSuggestList.Clear();

				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					foreach (var i in db.Projects.Where(i => i.AreaId == CurrentArea.Id).Select(i => i))
					{
						//CurrentProjects.Add(NewProjectButtonCreate(i));
					}
				}
			}

			else
			{
				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					foreach (var i in db.Projects.
										Where(i => i.Name.ToLowerInvariant().Contains(sender.Text.ToLowerInvariant()) && i.AreaId == CurrentArea.Id).
										Select(i => i))
					{
						if (!ProjectSuggestList.Contains(i.Name))
						{
							ProjectSuggestList.Add(i.Name);
						}

						//CurrentProjects.Add(NewProjectButtonCreate(i));
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

			if (CurrentProjects != null)
			{
				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					var term = args.QueryText.ToLower();
					foreach (var i in db.Projects.Where(i => i.Name.ToLower().Contains(term) && i.AreaId == CurrentArea.Id).Select(i => i))
					{
						//CurrentProjects.Add(NewProjectButtonCreate(i));
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
				if (project == null && string.IsNullOrEmpty(name.Text))
				{
					await new MessageDialog("Не заполнено имя для нового проекта", "Ошибка добавления нового проекта").ShowAsync();

					return null;
				}
				else
				{
					if (string.IsNullOrEmpty(name.Text))
					{
						await new MessageDialog("Не заполнено имя изменяемого проекта", "Ошибка изменения проекта").ShowAsync();

						return null;
					}
				}

				// New area object for adding into Database
				return new Project()
				{
					Name = name.Text,
					Description = description.Text,
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