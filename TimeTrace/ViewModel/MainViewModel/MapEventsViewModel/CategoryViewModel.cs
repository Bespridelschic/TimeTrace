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

namespace TimeTrace.ViewModel.MainViewModel.MapEventsViewModel
{
	/// <summary>
	/// View model of category (calendar) creation
	/// </summary>
	public class CategoryViewModel : BaseViewModel
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

		private ObservableCollection<string> areaSuggestList;
		/// <summary>
		/// Filter tips
		/// </summary>
		public ObservableCollection<string> AreaSuggestList
		{
			get => areaSuggestList;
			set
			{
				areaSuggestList = value;
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
		/// <param name="mainPanel">Panel of frame</param>
		public CategoryViewModel()
		{
			MultipleSelection = ListViewSelectionMode.Single;

			ColorsTable = new Dictionary<string, string>()
			{
				{ "Красный", "#d50000" },
				{ "Розовый", "#E67C73" },
				{ "Оранжевый", "#F4511E"},
				{ "Жёлтый", "#F6BF26" },
				{ "Светло-зелёный", "#33B679" },
				{ "Зелёный", "#0B8043" },
				{ "Голубой", "#039BE5" },
				{ "Синий", "#3F51B5" },
				{ "Светло-фиолетовый", "#7986CB" },
				{ "Фиолетовый", "#8E24AA" },
				{ "Чёрный", "#616161" }
			};

			Calendars = new ObservableCollection<Area>();
			AreaSuggestList = new ObservableCollection<string>();

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
					Text = "Вы уверены что хотите удалить календари и все внутренние проекты и события для:",
					TextWrapping = TextWrapping.WrapWholeWords
				});

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

					await new MessageDialog($"Календари со всеми проектами и событиями внутри были удалёны",
									"Операция завершена успешно").ShowAsync();
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
					Title = "Подтверждение удаления",
					Content = $"Вы уверены что хотите удалить календарь {Calendars[selectedArea.Value].Name}?\n" +
								$"Удаление приведен к потере всех проектов и событий внутри календаря!",
					PrimaryButtonText = "Удалить",
					CloseButtonText = "Отмена",
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

						await new MessageDialog($"Календарь {Calendars[SelectedArea.Value].Name} со всеми проектами и событиями внутри был удалён",
							"Операция завершена успешно").ShowAsync();

						// Remove Area from UI panel
						Calendars.Remove(Calendars[SelectedArea.Value]);
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
									db.MapEvents.Update(innerMaps);
								}

								innerProjects.Color = Calendars[SelectedArea.Value].Color;
								db.Projects.Update(innerProjects);
							}
						}

						Calendars[SelectedArea.Value].UpdateAt = DateTime.UtcNow;

						// Update area in database
						db.Areas.Update(Calendars[SelectedArea.Value]);
						db.SaveChanges();

						/*// Update current button
						Calendars.RemoveAt(SelectedArea.Value);
						Calendars.Insert(SelectedArea.Value, selectedArea);*/
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
				Header = "Название",
				PlaceholderText = "Название нового календаря",
				Text = area?.Name ?? string.Empty,
				Margin = new Thickness(0, 0, 0, 10),
				MaxLength = 30,
				SelectionStart = area?.Name?.Length ?? 0,
			};

			TextBox description = new TextBox()
			{
				Header = "Описание",
				PlaceholderText = "Краткое описание календаря",
				Text = area?.Description ?? string.Empty,
				Margin = new Thickness(0, 0, 0, 10),
				MaxLength = 50,
			};

			#endregion

			#region Color box

			ComboBox colors = new ComboBox()
			{
				Header = "Цвет календаря",
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
				Title = (area == null) ? "Создание нового календаря" : "Редактирование календаря",
				Content = panel,
				PrimaryButtonText = (area == null) ? "Создать" : "Изменить",
				CloseButtonText = "Отложить",
				DefaultButton = ContentDialogButton.Primary,
			};

			var result = await dialog.ShowAsync();

			if (result == ContentDialogResult.Primary)
			{
				if (area == null && string.IsNullOrEmpty(name.Text))
				{
					await new MessageDialog("Не заполнено имя для нового календаря", "Ошибка добавления нового календаря").ShowAsync();

					return null;
				}
				else
				{
					if (string.IsNullOrEmpty(name.Text))
					{
						await new MessageDialog("Не заполнено имя изменяемого календаря", "Ошибка изменения календаря").ShowAsync();

						return null;
					}
				}

				// New area object for adding into Database
				return new Area()
				{
					Name = name.Text,
					Description = description.Text,
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

		/// <summary>
		/// Filtration of input filters
		/// </summary>
		/// <param name="sender">Input filter</param>
		/// <param name="args">Event args</param>
		public void CategoryFilter(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (Calendars == null) return;

			if (args.CheckCurrent())
			{
				AreaSuggestList.Clear();
			}

			// Remove all buttons for adding relevant filter
			Calendars.Clear();

			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

			// Select all calendars
			if (string.IsNullOrEmpty(sender.Text))
			{
				AreaSuggestList.Clear();

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
						.Where(i => i.Name.ToLowerInvariant().Contains(sender.Text.ToLowerInvariant()) ||
							i.Description.ToLowerInvariant().Contains(sender.Text.ToLowerInvariant()) &&
							i.EmailOfOwner == (string)localSettings.Values["email"] &&
							!i.IsDelete)
						.Select(i => i))
					{
						if (!AreaSuggestList.Contains(i.Name))
						{
							AreaSuggestList.Add(i.Name);
						}

						Calendars.Add(i);
					}
				}
			}
		}

		/// <summary>
		/// Click on Find button
		/// </summary>
		/// <param name="sender">Object</param>
		/// <param name="args">Args</param>
		public void CategoryFilterQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			AreaSuggestList.Clear();

			if (string.IsNullOrEmpty(args.QueryText))
			{
				return;
			}

			if (Calendars != null)
			{
				Calendars.Clear();

				using (MainDatabaseContext db = new MainDatabaseContext())
				{
					ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
					var term = args.QueryText.ToLower();
					foreach (var i in db.Areas.Where(i => i.Name.ToLower().Contains(term) && !i.IsDelete && i.EmailOfOwner == (string)localSettings.Values["email"]).Select(i => i))
					{
						Calendars.Add(i);
					}
				}
			}
		}

		#endregion
	}
}