using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTrace.Model.Events;
using TimeTrace.Model.Events.DBContext;
using TimeTrace.View.MainView.PersonalMapsCreatePages;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace TimeTrace.ViewModel.MainViewModel.MapEventsViewModel
{
	/// <summary>
	/// View model of category (calendar) creation
	/// </summary>
	public class CategoryViewModel
	{
		#region Properties

		/// <summary>
		/// Local page frame
		/// </summary>
		public Frame Frame { get; set; }

		/// <summary>
		/// Panel of controls
		/// </summary>
		public VariableSizedWrapGrid MainGridPanel { get; set; }

		/// <summary>
		/// Available colors for choosing
		/// </summary>
		private Dictionary<string, string> ColorsTable { get; set; }

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		/// <param name="mainPanel">Panel of frame</param>
		public CategoryViewModel(VariableSizedWrapGrid mainPanel)
		{
			MainGridPanel = mainPanel;

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

			using (MapEventContext db = new MapEventContext())
			{
				foreach (var i in db.Areas.Select(i => i))
				{
					MainGridPanel.Children.Add(NewCategoryButtonCreate(i));
				}
			}
		}

		/// <summary>
		/// Creation of new category
		/// </summary>
		public async void CategoryCreate()
		{
			Area newArea = await CategoryCreationDialogAsync(null);

			if (newArea != null)
			{
				// Adding area into DB
				using (MapEventContext db = new MapEventContext())
				{
					db.Areas.Add(newArea);
					db.SaveChanges();
				}

				MainGridPanel.Children.Add(NewCategoryButtonCreate(newArea));
			}
		}

		/// <summary>
		/// Selecting an event category
		/// </summary>
		public void CategorySelect(object sender, RoutedEventArgs e)
		{
			using (MapEventContext db = new MapEventContext())
			{
				var selectedArea = db.Areas.First(i => i.Id == (string)(sender as Button).Tag);
				TransitionData<Area> data = new TransitionData<Area>(Frame, selectedArea);

				Frame.Navigate(typeof(ProjectListPage), data);
			}
		}

		/// <summary>
		/// Removing of selected category
		/// </summary>
		public async void CategoryRemoveAsync(object sender, RoutedEventArgs e)
		{
			using (MapEventContext db = new MapEventContext())
			{
				// Get selected Area
				var selectedArea = db.Areas.First(i => i.Id == (string)(sender as MenuFlyoutItem).Tag);

				ContentDialog confirmDialog = new ContentDialog()
				{
					Title = "Подтверждение удаления",
					Content = $"Вы уверены что хотите удалить календарь {selectedArea.Name}?\n" +
								$"Удаление приведен к потере всех проектов и событий внутри календаря!",
					PrimaryButtonText = "Удалить",
					CloseButtonText = "Отмена",
					DefaultButton = ContentDialogButton.Close,
				};

				var result = await confirmDialog.ShowAsync();

				if (result == ContentDialogResult.Primary)
				{
					// Remove Area from UI panel
					MainGridPanel.Children.Remove(MainGridPanel.Children.First(i => (string)(i as Button).Tag == selectedArea.Id));
					// Remove Area from Database
					db.Areas.Remove(selectedArea);
					db.SaveChanges();
				}
			}
		}

		/// <summary>
		/// Edit of selected category
		/// </summary>
		public async void CategoryEditAsync(object sender, RoutedEventArgs e)
		{
			using (MapEventContext db = new MapEventContext())
			{
				// Get selected Area
				var selectedArea = db.Areas.First(i => i.Id == (string)(sender as MenuFlyoutItem).Tag);

				// Get edited Area object
				var editedArea = await CategoryCreationDialogAsync(selectedArea);
				selectedArea.Name = editedArea.Name;
				selectedArea.Description = editedArea.Description;
				selectedArea.Color = editedArea.Color;

				if (selectedArea.GetHashCode() != editedArea.GetHashCode())
				{
					db.Areas.Update(selectedArea);
					db.SaveChanges();

					MainGridPanel.Children.First(i => (string)(i as Button).Tag == selectedArea.Id).InvalidateMeasure(); // TODO: Not work
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
				PlaceholderText = "Название новой категории",
				Text = area?.Name ?? string.Empty,
				Margin = new Thickness(0, 0, 0, 10),
				MaxLength = 30,
			};

			TextBox description = new TextBox()
			{
				Header = "Описание",
				PlaceholderText = "Краткое описание категории",
				Text = area?.Description ?? string.Empty,
				Margin = new Thickness(0, 0, 0, 10),
				MaxLength = 50,
			};

			#endregion

			#region Color box

			ComboBox colors = new ComboBox()
			{
				Header = "Цвет категории",
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
				Title = (area == null) ? "Создание новой категории" : "Редактирование категории",
				Content = panel,
				PrimaryButtonText = (area == null) ? "Создать" : "Изменить",
				CloseButtonText = "Отложить",
				DefaultButton = ContentDialogButton.Primary,
			};

			var result = await dialog.ShowAsync();

			if (result == ContentDialogResult.Primary)
			{
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
		/// Factory of buttons
		/// </summary>
		/// <param name="area"></param>
		/// <returns>New button object</returns>
		private Button NewCategoryButtonCreate(Area area)
		{
			// Edit menu item
			MenuFlyoutItem editItem = new MenuFlyoutItem()
			{
				Text = "Редактировать",
				Icon = new SymbolIcon(Symbol.Edit),
				Tag = area.Id,
			};
			editItem.Click += CategoryEditAsync;

			// Remove menu item
			MenuFlyoutItem removeItem = new MenuFlyoutItem()
			{
				Text = "Удалить",
				Icon = new SymbolIcon(Symbol.Delete),
				Tag = area.Id,
			};
			removeItem.Click += CategoryRemoveAsync;

			// Main menu for button
			MenuFlyout menuForButton = new MenuFlyout();
			menuForButton.Items.Add(editItem);
			menuForButton.Items.Add(removeItem);

			// Creating area button for adding in page
			Button newButton = new Button()
			{
				BorderBrush = GetColorFromString(ColorsTable.ElementAt(area.Color - 1).Value),
				Content = area,
				Tag = area.Id,
				ContextFlyout = menuForButton
			};
			newButton.Click += CategorySelect;

			return newButton;
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
	}
}