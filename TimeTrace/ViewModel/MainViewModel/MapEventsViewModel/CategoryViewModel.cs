using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
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
	public class CategoryViewModel : BaseViewModel
	{
		#region Properties

		/// <summary>
		/// Available colors for choosing
		/// </summary>
		private Dictionary<string, string> ColorsTable { get; set; }

		private ObservableCollection<Button> buttonCollection;
		/// <summary>
		/// Binded collection of buttons
		/// </summary>
		public ObservableCollection<Button> ButtonCollection
		{
			get => buttonCollection;
			set
			{
				buttonCollection = value;
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

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		/// <param name="mainPanel">Panel of frame</param>
		public CategoryViewModel()
		{
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

			ButtonCollection = new ObservableCollection<Button>();
			AreaSuggestList = new ObservableCollection<string>();

			using (MapEventContext db = new MapEventContext())
			{
				foreach (var i in db.Areas.Select(i => i))
				{
					ButtonCollection.Add(NewCategoryButtonCreate(i));
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
				using (var db = new MapEventContext())
				{
					db.Areas.Add(newArea);
					db.SaveChanges();
				}

				ButtonCollection.Add(NewCategoryButtonCreate(newArea));
			}
			else
			{
				await new MessageDialog("Не заполнено имя для нового календаря", "Ошибка добавления нового календаря").ShowAsync();
			}
		}

		/// <summary>
		/// Selecting an event category
		/// </summary>
		public void CategorySelect(object sender, RoutedEventArgs e)
		{
			using (MapEventContext db = new MapEventContext())
			{
				(Application.Current as App).AppFrame.Navigate(typeof(ProjectListPage), db.Areas.First(i => i.Id == (string)(sender as Button).Tag));
			}
		}

		public void CategoriesBulkRemoval()
		{

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
					ButtonCollection.Remove(ButtonCollection.First(i => (string)(i as Button).Tag == selectedArea.Id));

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

				// If area was edit
				if (editedArea.Name != selectedArea.Name || editedArea.Description != selectedArea.Description || editedArea.Color != selectedArea.Color)
				{
					selectedArea.Name = editedArea.Name;
					selectedArea.Description = editedArea.Description;
					selectedArea.Color = editedArea.Color;

					// Update area in database
					db.Areas.Update(selectedArea);
					db.SaveChanges();

					// Update current button
					var index = ButtonCollection.IndexOf(ButtonCollection.First(i => (string)i.Tag == selectedArea.Id));
					ButtonCollection.RemoveAt(index);
					ButtonCollection.Insert(index, NewCategoryButtonCreate(selectedArea));
				}
			}
		}

		/// <summary>
		/// Filtration of input filters
		/// </summary>
		/// <param name="sender">Input filter</param>
		/// <param name="args">Event args</param>
		public void CategoryFilter(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (ButtonCollection == null) return;

			if (args.CheckCurrent())
			{
				AreaSuggestList.Clear();
			}

			// Remove all buttons for adding relevant filter
			ButtonCollection.Clear();

			// Select all calendars
			if (string.IsNullOrEmpty(sender.Text))
			{
				AreaSuggestList.Clear();

				using (MapEventContext db = new MapEventContext())
				{
					foreach (var i in db.Areas.Select(i => i))
					{
						ButtonCollection.Add(NewCategoryButtonCreate(i));
					}
				}
			}

			else
			{
				using (MapEventContext db = new MapEventContext())
				{
					foreach (var i in db.Areas.Where(i => i.Name.ToLowerInvariant().Contains(sender.Text.ToLowerInvariant())).Select(i => i))
					{
						if (!AreaSuggestList.Contains(i.Name))
						{
							AreaSuggestList.Add(i.Name);
						}

						ButtonCollection.Add(NewCategoryButtonCreate(i));
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

			if (ButtonCollection != null)
			{
				using (MapEventContext db = new MapEventContext())
				{
					var term = args.QueryText.ToLower();
					foreach (var i in db.Areas.Where(i => i.Name.ToLower().Contains(term)).Select(i => i))
					{
						ButtonCollection.Add(NewCategoryButtonCreate(i));
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
				PlaceholderText = "Название новой категории",
				Text = area?.Name ?? string.Empty,
				Margin = new Thickness(0, 0, 0, 10),
				MaxLength = 30,
				SelectionStart = area?.Name?.Length ?? 0,
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
				if (area == null && string.IsNullOrEmpty(name.Text))
				{
					return null;
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