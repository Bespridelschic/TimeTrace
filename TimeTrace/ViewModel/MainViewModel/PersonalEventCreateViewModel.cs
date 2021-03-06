﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TimeTrace.Model;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TimeTrace.View.MainView.PersonalMapsCreatePages;
using Windows.UI.Popups;
using System.Xml.Linq;
using System.Diagnostics;
using System.Runtime.Serialization.Json;
using System.IO;
using TimeTrace.Model.Events.DBContext;
using Windows.UI;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
using TimeTrace.Model.Events;

namespace TimeTrace.ViewModel.MainViewModel
{
	/// <summary>
	/// ViewModel of creation new event
	/// </summary>
	public class PersonalEventCreateViewModel : BaseViewModel
	{
		#region Properties

		private MapEvent currentMapEvent;
		/// <summary>
		/// Current event model
		/// </summary>
		public MapEvent CurrentMapEvent
		{
			get { return currentMapEvent; }
			set
			{
				currentMapEvent = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Min updateAt - current day
		/// </summary>
		public DateTime MinDate { get; set; }

		private bool isSelectMan;
		/// <summary>
		/// Enabled of man binding
		/// </summary>
		public bool IsSelectMan
		{
			get { return isSelectMan; }
			set
			{
				isSelectMan = value;
				OnPropertyChanged();
			}
		}

		private bool isSelectPlace;
		/// <summary>
		/// Enabled of location binding
		/// </summary>
		public bool IsSelectPlace
		{
			get { return isSelectPlace; }
			set
			{
				isSelectPlace = value;
				OnPropertyChanged();
			}
		}

		private int bindingObjectIndex;
		/// <summary>
		/// Binding object of event
		/// </summary>
		public int BindingObjectIndex
		{
			get { return bindingObjectIndex; }
			set
			{
				bindingObjectIndex = value;
				switch (value)
				{
					case 0:
						{
							IsSelectPlace = true;
							IsSelectMan = false;

							break;
						}
					case 1:
						{
							IsSelectMan = true;
							IsSelectPlace = false;

							break;
						}
					case 2:
						{
							IsSelectPlace = true;
							IsSelectMan = true;

							break;
						}
					default:
						throw new Exception("Не определенный индекс привязки события!");
				}
				OnPropertyChanged();
			}
		}

		private DateTimeOffset? startDate;
		/// <summary>
		/// Event start updateAt
		/// </summary>
		public DateTimeOffset? StartDate
		{
			get { return startDate; }
			set
			{
				startDate = value;
				if (StartDate != null)
				{
					CurrentMapEvent.Start = StartDate.Value.Date + StartTime;
				}

				OnPropertyChanged();
			}
		}

		private TimeSpan startTime;
		/// <summary>
		/// Event start time
		/// </summary>
		public TimeSpan StartTime
		{
			get { return startTime; }
			set
			{
				if (startTime != value)
				{
					startTime = value;
					if (StartDate != null)
					{
						CurrentMapEvent.Start = StartDate.Value.Date + StartTime;
					}

					OnPropertyChanged();
				}
			}
		}

		private DateTimeOffset? endDate;
		/// <summary>
		/// Event end updateAt
		/// </summary>
		public DateTimeOffset? EndDate
		{
			get { return endDate; }
			set
			{
				endDate = value;
				if (endDate != null)
				{
					CurrentMapEvent.End = EndDate.Value.Date + EndTime;
				}

				OnPropertyChanged();
			}
		}

		private TimeSpan endTime;
		/// <summary>
		/// Event end time
		/// </summary>
		public TimeSpan EndTime
		{
			get { return endTime; }
			set
			{
				if (endTime != value)
				{
					endTime = value;
					if (endDate != null)
					{
						CurrentMapEvent.End = EndDate.Value.Date + EndTime;
					}

					OnPropertyChanged();
				}
			}
		}

		private bool isNotAllDay;
		/// <summary>
		/// Is all day selected
		/// </summary>
		public bool IsNotAllDay
		{
			get { return isNotAllDay; }
			set
			{
				isNotAllDay = value;

				if (!isNotAllDay)
				{
					EndDate = null;
					EndTime = TimeSpan.Parse("00:00");
				}

				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Local page frame
		/// </summary>
		public Frame Frame { get; set; }

		/// <summary>
		/// Panel of controls
		/// </summary>
		public VariableSizedWrapGrid MainGridPanel { get; set; }

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		/// <param name="areaId">ID of parent area</param>
		/// <param name="mainGridPanel">Owner panel</param>
		public PersonalEventCreateViewModel(string areaId, VariableSizedWrapGrid mainGridPanel)
		{
			CurrentMapEvent = new MapEvent(areaId);
			StartDate = DateTime.Now;

			MinDate = DateTime.Today;
			IsNotAllDay = false;

			BindingObjectIndex = 0;

			MainGridPanel = mainGridPanel;
		}

		/// <summary>
		/// Creating a new event
		/// </summary>
		/// <returns>Result of event creation</returns>
		public async Task EventCreate()
		{
			if (string.IsNullOrEmpty(CurrentMapEvent.Name) || StartDate == null || (EndDate == null && IsNotAllDay))
			{
				await (new MessageDialog("Не заполнено одно из обязательных полей", "Ошибка создания нового события")).ShowAsync();

				return;
			}

			if (!IsNotAllDay)
			{
				EndTime = TimeSpan.Parse("23:59");
				EndDate = StartDate;
			}

			if (CurrentMapEvent.Start > CurrentMapEvent.End)
			{
				await (new MessageDialog("Дата начала не может быть позже даты окончания события", "Ошибка создания нового события")).ShowAsync();

				return;
			}

			CurrentMapEvent.UpdateAt = DateTime.Now;
			CurrentMapEvent.IsDelete = false;

			if (CurrentMapEvent.Location == null)
			{
				CurrentMapEvent.Location = string.Empty;
			}

			if (CurrentMapEvent.UserBind == null)
			{
				CurrentMapEvent.UserBind = string.Empty;
			}

			if (string.IsNullOrEmpty(CurrentMapEvent.Description))
			{
				CurrentMapEvent.Description = null;
			}

			if (string.IsNullOrEmpty(CurrentMapEvent.UserBind))
			{
				CurrentMapEvent.UserBind = null;
			}

			if (string.IsNullOrEmpty(CurrentMapEvent.Location))
			{
				CurrentMapEvent.Location = null;
			}

			using (MapEventContext db = new MapEventContext())
			{
				db.MapEvents.Add(CurrentMapEvent);
				db.SaveChanges();
			}

			NewMapEventNotification(CurrentMapEvent.Name, CurrentMapEvent.Start);
		}

		/// <summary>
		/// Selecting an event category
		/// </summary>
		public void CategorySelect()
		{
			Frame.Navigate(typeof(PersonalEventCreatePage), Frame);
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

			// building list of colors for choosing
			for (int i = 0; i < colorsTable.Count; i++)
			{
				var brush = GetColorFromString(colorsTable.ElementAt(i).Value);

				// ring of color
				Ellipse colorRing = new Ellipse
				{
					Fill = brush,
					Width = 16,
					Height = 16,
				};

				Ellipse innerColorRing = new Ellipse
				{
					Fill = new SolidColorBrush(new Color() { A = 255, R = 255, G = 255, B = 255 }),
					Width = 12,
					Height = 12,
				};

				// item panel with color & name of color
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

				Area newArea = new Area()
				{
					Name = name.Text,
					Description = description.Text,
					Color = colorsTable.ElementAt(colors.SelectedIndex).Value,
				};

				using (MapEventContext db = new MapEventContext())
				{
					db.Areas.Add(newArea);
					db.SaveChanges();
				}

				Button newButton = new Button()
				{
					BorderBrush = GetColorFromString(colorsTable.ElementAt(colors.SelectedIndex).Value),
					Content = newArea,
					Tag = newArea.Id,
				};

				MainGridPanel.Children.Add(newButton);
			}
		}

		/// <summary>
		/// Get color from HEX format to SolidColorBrush
		/// </summary>
		/// <param name="color">HEX string of color</param>
		/// <returns><see cref="SolidColorBrush"/> object</returns>
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
		/// Cancell event creation and back to categories
		/// </summary>
		public void BackToCategories()
		{
			Frame.Navigate(typeof(CategorySelectPage), Frame);
		}

		/// <summary>
		/// Map event create message
		/// </summary>
		private void NewMapEventNotification(string name, DateTime start)
		{
			var toastContent = new ToastContent()
			{
				Visual = new ToastVisual()
				{
					BindingGeneric = new ToastBindingGeneric()
					{
						Children =
						{
							new AdaptiveText()
							{
								Text = "Создано новое событие",
								HintMaxLines = 1
							},
							new AdaptiveText()
							{
								Text = name,
							},
							new AdaptiveText()
							{
								Text = $"Начало в {start.ToShortTimeString()}"
							}
						},
						AppLogoOverride = new ToastGenericAppLogo()
						{
							//Source = "https://picsum.photos/48?image=883",
							Source = @"Assets/user-48.png",
							HintCrop = ToastGenericAppLogoCrop.Circle
						}
					}
				},
				Launch = "app-defined-string"
			};

			// Create the toast notification
			var toastNotif = new ToastNotification(toastContent.GetXml());

			// And send the notification
			ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
		}
	}
}