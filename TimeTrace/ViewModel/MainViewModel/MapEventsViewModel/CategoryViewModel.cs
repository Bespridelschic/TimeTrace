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

		#endregion

		public CategoryViewModel(VariableSizedWrapGrid mainPanel)
		{
			MainGridPanel = mainPanel;

			using (MapEventContext db = new MapEventContext())
			{
				var res = db.Areas.Select(i => i);

				foreach (var i in res)
				{
					Dictionary<string, string> colorsTable = new Dictionary<string, string>()
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

					Button button = new Button()
					{
						Tag = i.Id,
						BorderBrush = GetColorFromString(colorsTable.ElementAt(i.Color - 1).Value),
						Content = i,
					};
					button.Click += CategorySelect;

					MainGridPanel.Children.Add(button);
				}
			}
		}

		private SolidColorBrush GetColorFromString(string color)
		{
			var colorString = color.Replace("#", string.Empty);

			var r = byte.Parse(colorString.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
			var g = byte.Parse(colorString.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
			var b = byte.Parse(colorString.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

			Color localColor = Color.FromArgb(255, r, g, b);
			return new SolidColorBrush(localColor);
		}

		/// <summary>
		/// Creation of new category
		/// </summary>
		public async void CategoryCreate()
		{
			#region Text boxes

			TextBox name = new TextBox()
			{
				Header = "Название",
				PlaceholderText = "Название новой категории",
				Margin = new Thickness(0, 0, 0, 10),
				MaxLength = 30,
			};

			TextBox description = new TextBox()
			{
				Header = "Описание",
				PlaceholderText = "Краткое описание категории",
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

			Dictionary<string, string> colorsTable = new Dictionary<string, string>()
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

			// Building list of colors for choosing
			for (int i = 0; i < colorsTable.Count; i++)
			{
				var brush = GetColorFromString(colorsTable.ElementAt(i).Value);

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
					Text = $"{colorsTable.ElementAt(i).Key}",
					Margin = new Thickness(8, 0, 0, 0),
					VerticalAlignment = VerticalAlignment.Center
				});

				colors.Items.Add(colorBoxItem);
			}

			#endregion

			// Start selected color is red
			colors.SelectedIndex = 0;

			StackPanel panel = new StackPanel();
			panel.Children.Add(name);
			panel.Children.Add(description);
			panel.Children.Add(colors);

			ContentDialog dialog = new ContentDialog()
			{
				Title = "Создание новой категории",
				Content = panel,
				PrimaryButtonText = "Создать",
				CloseButtonText = "Отложить",
				DefaultButton = ContentDialogButton.Primary,
			};

			var result = await dialog.ShowAsync();

			if (result == ContentDialogResult.Primary)
			{
				if (string.IsNullOrEmpty(name.Text) || string.IsNullOrEmpty(description.Text))
				{
					await (new MessageDialog("Заполните все поля", "Ошибка создания категории")).ShowAsync();
					CategoryCreate();

					return;
				}

				// New area object for adding into DB
				Area newArea = new Area()
				{
					Name = name.Text,
					Description = description.Text,
					Color = colors.SelectedIndex + 1,
				};

				// Adding area into DB
				using (MapEventContext db = new MapEventContext())
				{
					db.Areas.Add(newArea);
					db.SaveChanges();
				}

				// Creating area button for adding in page
				Button newButton = new Button()
				{
					BorderBrush = GetColorFromString(colorsTable.ElementAt(colors.SelectedIndex).Value),
					Content = newArea,
					Tag = newArea.Id,
				};
				newButton.Click += CategorySelect;

				MainGridPanel.Children.Add(newButton);
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
	}
}