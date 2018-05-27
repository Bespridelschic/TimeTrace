using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using TimeTrace.Model.Events;
using TimeTrace.Model.DBContext;
using TimeTrace.View.MainView.PersonalMapsCreatePages;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Input;
using TimeTrace.Model.Requests;
using Windows.ApplicationModel.Resources;

namespace TimeTrace.ViewModel.MainViewModel.MapEventsViewModel
{
	/// <summary>
	/// View model of category (calendar) creation
	/// </summary>
	public class CategoryViewModel : BaseViewModel, ISearchable<string>
	{
		#region Properties

		/// <summary>
		/// Available colors for choosing
		/// </summary>
		private Dictionary<string, string> ColorsTable { get; set; }

		private ObservableCollection<Area> calendars;
		/// <summary>
		/// Collection of calendars
		/// </summary>
		public ObservableCollection<Area> Calendars
		{
			get => calendars;
			set
			{
				calendars = value;
				OnPropertyChanged();
			}
		}

		private List<Area> selectedAreas;
		/// <summary>
		/// Collection of multiple areas selection
		/// </summary>
		public List<Area> SelectedAreas
		{
			get => selectedAreas;
			set
			{
				selectedAreas = value;
				OnPropertyChanged();
			}
		}

		private int? selectedArea;
		/// <summary>
		/// Index of selected area
		/// </summary>
		public int? SelectedArea
		{
			get => selectedArea;
			set
			{
				selectedArea = value;
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

		/// <summary>
		/// Localization resource loader
		/// </summary>
		public ResourceLoader ResourceLoader { get; set; }

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		/// <param name="mainPanel">Panel of frame</param>
		public CategoryViewModel()
		{
			StartPageViewModel.Instance.SetHeader(StartPageViewModel.Headers.MapEvents);
			ResourceLoader = ResourceLoader.GetForCurrentView("CalendarsVM");

			MultipleSelection = ListViewSelectionMode.Single;

			ColorsTable = new Dictionary<string, string>()
			{
				{ ResourceLoader.GetString("/CalendarsVM/Red"), "#d50000" },
				{ ResourceLoader.GetString("/CalendarsVM/Pink"), "#E67C73" },
				{ ResourceLoader.GetString("/CalendarsVM/Orange"), "#F4511E"},
				{ ResourceLoader.GetString("/CalendarsVM/Yellow"), "#F6BF26" },
				{ ResourceLoader.GetString("/CalendarsVM/LightGreen"), "#33B679" },
				{ ResourceLoader.GetString("/CalendarsVM/Green"), "#0B8043" },
				{ ResourceLoader.GetString("/CalendarsVM/Blue"), "#039BE5" },
				{ ResourceLoader.GetString("/CalendarsVM/DarkBlue"), "#3F51B5" },
				{ ResourceLoader.GetString("/CalendarsVM/LightPurple"), "#7986CB" },
				{ ResourceLoader.GetString("/CalendarsVM/Purple"), "#8E24AA" },
				{ ResourceLoader.GetString("/CalendarsVM/Black"), "#616161" }
			};

			Calendars = new ObservableCollection<Area>();
			SearchSuggestions = new ObservableCollection<string>();

			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

			using (MainDatabaseContext db = new MainDatabaseContext())
			{
				foreach (var i in db.Areas.Where(i => !i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"]).Select(i => i))
				{
					Calendars.Add(i);
				}
			}
		}

		/// <summary>
		/// Creation of new category
		/// </summary>
		public async void CategoryCreateAsync()
		{
			Area newArea = await CategoryCreationDialogAsync(null);

			if (newArea != null)
			{
				// Adding area into DB
				using (var db = new MainDatabaseContext())
				{
					db.Areas.Add(newArea);
					db.SaveChanges();
				}

				Calendars.Add(newArea);

				// Synchronization of changes with server
				await StartPageViewModel.Instance.CategoriesSynchronization();
			}
		}

		/// <summary>
		/// Selecting an event category
		/// </summary>
		public void CategorySelect()
		{
			if (SelectedArea.HasValue)
			{
				(Application.Current as App).AppFrame.Navigate(typeof(ProjectListPage), Calendars[SelectedArea.Value]);
			}
		}

		/// <summary>
		/// Selecting item of list with right click
		/// </summary>
		/// <param name="sender">Sender list</param>
		/// <param name="e">Parameter</param>
		public void SelectItemOnRightClick(object sender, RightTappedRoutedEventArgs e)
		{
			GridView listView = (GridView)sender;
			if (((FrameworkElement)e.OriginalSource).DataContext is Area selectedItem)
			{
				SelectedArea = Calendars.IndexOf(selectedItem);
			}
		}

		/// <summary>
		/// Select several areas
		/// </summary>
		/// <param name="sender">ListView</param>
		/// <param name="e">Parameters</param>
		public void MultipleAreasSelection(object sender, SelectionChangedEventArgs e)
		{
			GridView listView = sender as GridView;
			SelectedAreas = new List<Area>();

			foreach (Area item in listView.SelectedItems)
			{
				SelectedAreas.Add(item);
			};
		}

		/// <summary>
		/// Bulk removal of calendars
		/// </summary>
		public async void CategoriesBulkRemovalAsync()
		{
			if (MultipleSelection == ListViewSelectionMode.Single)
			{
				MultipleSelection = ListViewSelectionMode.Multiple;
			}
			else
			{
				if (SelectedAreas == null || SelectedAreas.Count <= 0)
				{
					MultipleSelection = ListViewSelectionMode.Single;
					return;
				}

				string removedAreas = string.Empty;

				foreach (var area in SelectedAreas)
				{
					removedAreas += $"{area.Name}\n";
				}

				ScrollViewer scrollViewer = new ScrollViewer()
				{
					VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
					Content = new TextBlock() { Text = removedAreas },
				};

				StackPanel mainPanel = new StackPanel()
				{
					Margin = new Thickness(0, 0, 0, 10),
				};

				mainPanel.Children.Add(new TextBlock()
				{
					Text = ResourceLoader.GetString("/CalendarsVM/RemoveConfirm"),
					TextWrapping = TextWrapping.WrapWholeWords
				});

				mainPanel.Children.Add(scrollViewer);

				ContentDialog contentDialog = new ContentDialog()
				{
					Title = ResourceLoader.GetString("/CalendarsVM/ConfirmRemove"),
					Content = mainPanel,
					PrimaryButtonText = ResourceLoader.GetString("/CalendarsVM/Remove"),
					CloseButtonText = ResourceLoader.GetString("/CalendarsVM/Cancel"),
					DefaultButton = ContentDialogButton.Close
				};

				var result = await contentDialog.ShowAsync();

				if (result == ContentDialogResult.Primary)
				{
					using (MainDatabaseContext db = new MainDatabaseContext())
					{
						foreach (var area in SelectedAreas)
						{
							// Projects of the deleted calendar
							foreach (var innerProject in db.Projects.Where(i => i.AreaId == area.Id))
							{
								foreach (var mapEvent in db.MapEvents.Where(i => i.ProjectId == innerProject.Id))
								{
									db.MapEvents.FirstOrDefault(i => i.Id == mapEvent.Id).IsDelete = true;
								}

								db.Projects.FirstOrDefault(i => i.Id == innerProject.Id).IsDelete = true;
							}

							// Remove Area from database
							db.Areas.FirstOrDefault(i => i.Id == area.Id).IsDelete = true;

							db.SaveChanges();

							// Remove Area from UI panel
							Calendars.Remove(area);
						}
					}

					await new MessageDialog(ResourceLoader.GetString("/CalendarsVM/CalendarWasRemovedNotification"),
									ResourceLoader.GetString("/CalendarsVM/OperationSuccess")).ShowAsync();
				}

				MultipleSelection = ListViewSelectionMode.Single;
			}
		}

		/// <summary>
		/// Removing of selected category
		/// </summary>
		public async void CategoryRemoveAsync()
		{
			if (SelectedArea.HasValue)
			{
				ContentDialog confirmDialog = new ContentDialog()
				{
					Title = ResourceLoader.GetString("/CalendarsVM/ConfirmRemove"),
					Content = $"{ResourceLoader.GetString("/CalendarsVM/ConfirmationRemoving")} {Calendars[selectedArea.Value].Name}?\n" +
								$"{ResourceLoader.GetString("/CalendarsVM/CascadingRemovalWarning")}",
					PrimaryButtonText = ResourceLoader.GetString("/CalendarsVM/Remove"),
					CloseButtonText = ResourceLoader.GetString("/CalendarsVM/Cancel"),
					DefaultButton = ContentDialogButton.Close,
				};

				var result = await confirmDialog.ShowAsync();

				if (result == ContentDialogResult.Primary)
				{
					using (MainDatabaseContext db = new MainDatabaseContext())
					{
						// Projects of the deleted calendar
						foreach (var innerProject in db.Projects.Where(i => i.AreaId == Calendars[selectedArea.Value].Id))
						{
							foreach (var mapEvent in db.MapEvents.Where(i => i.ProjectId == innerProject.Id))
							{
								db.MapEvents.FirstOrDefault(i => i.Id == mapEvent.Id).IsDelete = true;
							}

							db.Projects.FirstOrDefault(i => i.Id == innerProject.Id).IsDelete = true;
						}

						// Remove Area from database
						db.Areas.FirstOrDefault(i => i.Id == Calendars[SelectedArea.Value].Id).IsDelete = true;

						db.SaveChanges();

						await new MessageDialog($"{ResourceLoader.GetString("/CalendarsVM/Calendar")} {Calendars[SelectedArea.Value].Name} {ResourceLoader.GetString("/CalendarsVM/CascadingRemovalNotification")}",
							ResourceLoader.GetString("/CalendarsVM/OperationSuccess")).ShowAsync();

						// Remove Area from UI panel
						Calendars.Remove(Calendars[SelectedArea.Value]);

						// Synchronization of changes with server
						await StartPageViewModel.Instance.CategoriesSynchronization();
					}
				}
			}
		}

		/// <summary>
		/// Edit of selected category
		/// </summary>
		public async void CategoryEditAsync()
		{
			if (SelectedArea.HasValue)
			{
				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					// Get edited Area object
					var editedArea = await CategoryCreationDialogAsync(Calendars[SelectedArea.Value]);

					if (editedArea == null)
					{
						return;
					}

					// If area was edit
					if (editedArea.Name != Calendars[SelectedArea.Value].Name ||
						editedArea.Description != Calendars[SelectedArea.Value].Description ||
						editedArea.Color != Calendars[SelectedArea.Value].Color)
					{
						Calendars[SelectedArea.Value].Name = editedArea.Name;
						Calendars[SelectedArea.Value].Description = editedArea.Description;

						if (editedArea.Color != Calendars[SelectedArea.Value].Color)
						{
							Calendars[SelectedArea.Value].Color = editedArea.Color;

							foreach (var innerProjects in db.Projects.Where(i => i.AreaId == Calendars[SelectedArea.Value].Id))
							{
								foreach (var innerMaps in db.MapEvents.Where(i => i.ProjectId == innerProjects.Id))
								{
									innerMaps.Color = Calendars[SelectedArea.Value].Color;
									innerMaps.UpdateAt = DateTime.UtcNow;
									db.MapEvents.Update(innerMaps);
								}

								innerProjects.Color = Calendars[SelectedArea.Value].Color;
								innerProjects.UpdateAt = DateTime.UtcNow;
								db.Projects.Update(innerProjects);
							}
						}

						Calendars[SelectedArea.Value].UpdateAt = DateTime.UtcNow;

						try
						{
							// Update area in database
							db.Areas.Update(Calendars[SelectedArea.Value]);
							db.SaveChanges();
						}
						catch (Exception)
						{
							await new MessageDialog(ResourceLoader.GetString("/CalendarsVM/UndefinedErrorMessage"), ResourceLoader.GetString("/CalendarsVM/CalendarChangeError")).ShowAsync();
						}

						// Update current button
						int selectedIndex = SelectedArea.Value;
						var tempArea = Calendars[selectedIndex];
						Calendars.RemoveAt(selectedIndex);
						Calendars.Insert(selectedIndex, tempArea);

						// Synchronization of changes with server
						await StartPageViewModel.Instance.CategoriesSynchronization();
					}
				}
			}
		}

		/// <summary>
		/// Creation of edition area dialog
		/// </summary>
		/// <param name="area"><see cref="Area"/> object</param>
		/// <returns>Edited object of area or new object if created</returns>
		private async Task<Area> CategoryCreationDialogAsync(Area area)
		{
			#region Text boxes

			TextBox name = new TextBox()
			{
				Header = ResourceLoader.GetString("/CalendarsVM/CalendarNameTitle"),
				PlaceholderText = ResourceLoader.GetString("/CalendarsVM/CalendarName"),
				Text = area?.Name ?? string.Empty,
				Margin = new Thickness(0, 0, 0, 10),
				MaxLength = 30,
				SelectionStart = area?.Name?.Length ?? 0,
			};

			TextBox description = new TextBox()
			{
				Header = ResourceLoader.GetString("/CalendarsVM/CalendarDescriptionTitle"),
				PlaceholderText = ResourceLoader.GetString("/CalendarsVM/CalendarDescription"),
				Text = area?.Description ?? string.Empty,
				Margin = new Thickness(0, 0, 0, 10),
				MaxLength = 50,
			};

			#endregion

			#region Color box

			ComboBox colors = new ComboBox()
			{
				Header = ResourceLoader.GetString("/CalendarsVM/CalendarColor"),
				Width = 300,
			};

			// Building list of colors for choosing
			for (int i = 0; i < ColorsTable.Count; i++)
			{
				var brush = GetColorFromString(ColorsTable.ElementAt(i).Value);

				// Ring of color
				Ellipse colorRing = new Ellipse
				{
					Fill = brush,
					Width = 16,
					Height = 16,
				};

				// Center ring with white color
				Ellipse innerColorRing = new Ellipse
				{
					Fill = new SolidColorBrush(new Color() { A = 255, R = 255, G = 255, B = 255 }),
					Width = 12,
					Height = 12,
				};

				// Item panel with color & name of color
				StackPanel colorBoxItem = new StackPanel();
				colorBoxItem.Orientation = Orientation.Horizontal;

				Grid colorGrid = new Grid();
				colorGrid.Children.Add(colorRing);
				colorGrid.Children.Add(innerColorRing);

				colorBoxItem.Children.Add(colorGrid);
				colorBoxItem.Children.Add(new TextBlock()
				{
					Text = $"{ColorsTable.ElementAt(i).Key}",
					Margin = new Thickness(8, 0, 0, 0),
					VerticalAlignment = VerticalAlignment.Center
				});

				colors.Items.Add(colorBoxItem);
			}

			#endregion

			// Start selected color or default red
			colors.SelectedIndex = (area == null) ? 0 : area.Color - 1;

			StackPanel panel = new StackPanel();
			panel.Children.Add(name);
			panel.Children.Add(description);
			panel.Children.Add(colors);

			ContentDialog dialog = new ContentDialog()
			{
				Title = (area == null) ? ResourceLoader.GetString("/CalendarsVM/CalendarAdding") : ResourceLoader.GetString("/CalendarsVM/CalendarChanging"),
				Content = panel,
				PrimaryButtonText = (area == null) ? ResourceLoader.GetString("/CalendarsVM/Create") : ResourceLoader.GetString("/CalendarsVM/Change"),
				CloseButtonText = ResourceLoader.GetString("/CalendarsVM/Later"),
				DefaultButton = ContentDialogButton.Primary,
			};

			var result = await dialog.ShowAsync();

			if (result == ContentDialogResult.Primary)
			{
				if (area == null && string.IsNullOrEmpty(name.Text?.Trim()))
				{
					await new MessageDialog(ResourceLoader.GetString("/CalendarsVM/CalendarAddingError"), ResourceLoader.GetString("/CalendarsVM/CalendarAddingErrorTitle")).ShowAsync();

					return null;
				}
				else
				{
					if (string.IsNullOrEmpty(name.Text?.Trim()))
					{
						await new MessageDialog(ResourceLoader.GetString("/CalendarsVM/CalendarChangingError"), ResourceLoader.GetString("/CalendarsVM/CalendarChangingErrorTitle")).ShowAsync();

						return null;
					}
				}

				// New area object for adding into Database
				return new Area()
				{
					Name = name.Text.Trim(),
					Description = description.Text.Trim(),
					Color = colors.SelectedIndex + 1,
				};
			}
			else
			{
				return area;
			}
		}

		/// <summary>
		/// Get the color brush from string
		/// </summary>
		/// <param name="color">Input string color</param>
		/// <returns>New color <see cref="Brush"/></returns>
		private SolidColorBrush GetColorFromString(string color)
		{
			var colorString = color.Replace("#", string.Empty);

			var r = byte.Parse(colorString.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
			var g = byte.Parse(colorString.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
			var b = byte.Parse(colorString.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

			Color localColor = Color.FromArgb(255, r, g, b);
			return new SolidColorBrush(localColor);
		}

		#region Searching calendars

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
			if (Calendars == null) return;

			SearchSuggestions.Clear();

			// Remove all calendars for adding relevant filter
			Calendars.Clear();

			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

			// Select all calendars
			if (string.IsNullOrEmpty(SearchTerm))
			{
				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					foreach (var i in db.Areas.Where(i => i.EmailOfOwner == (string)localSettings.Values["email"] && !i.IsDelete).Select(i => i))
					{
						Calendars.Add(i);
					}
				}
			}

			else
			{
				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					foreach (var i in db.Areas
						.Where(i => (i.Name.ToLowerInvariant().Contains(SearchTerm.ToLowerInvariant()) ||
									i.Description.ToLowerInvariant().Contains(SearchTerm.ToLowerInvariant())) &&
									i.EmailOfOwner == (string)localSettings.Values["email"] &&
									!i.IsDelete)
						.Select(i => i))
					{
						if (!SearchSuggestions.Contains(i.Name))
						{
							SearchSuggestions.Add(i.Name);
						}

						Calendars.Add(i);
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

			if (Calendars != null)
			{
				Calendars.Clear();

				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

					foreach (var i in db.Areas
						.Where(i => i.Name.ToLowerInvariant().Contains(SearchTerm.ToLowerInvariant())
							&& !i.IsDelete
							&& i.EmailOfOwner == (string)localSettings.Values["email"])
						.Select(i => i))
					{
						Calendars.Add(i);
					}
				}
			}
		}

		#endregion
	}
}