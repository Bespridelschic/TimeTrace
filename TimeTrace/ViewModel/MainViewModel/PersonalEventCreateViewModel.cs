using System;
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
		/// Min date - current day
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
		/// Enabled of place binding
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

		public Frame Frame { get; set; }

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		public PersonalEventCreateViewModel()
		{
			CurrentMapEvent = new MapEvent();
			MinDate = DateTime.Today;

			BindingObjectIndex = 0;
		}

		/// <summary>
		/// Creating a new event
		/// </summary>
		/// <returns>Result of event creation</returns>
		public async Task EventCreate()
		{
			if (string.IsNullOrEmpty(CurrentMapEvent.Name) || CurrentMapEvent.EventDate == null)
			{
				await (new MessageDialog("Не заполнено одно из обязательных полей", "Ошибка создания нового события")).ShowAsync();

				return;
			}

			XDocument doc = new XDocument(
				new XElement("Events",
					new XElement("Sport", new XAttribute("Name", $"{CurrentMapEvent.Name}"),
						new XElement("Description", $"{CurrentMapEvent.Description}"),
						new XElement("Place", $"{CurrentMapEvent.Place}"),
						new XElement("User", $"{CurrentMapEvent.UserBind.LastName}"),
						new XElement("Date", $"{CurrentMapEvent.FullEventDate}"),
						new XElement("Duration", $"{CurrentMapEvent.EventDuration}"),
						new XElement("Interval", $"{CurrentMapEvent.EventInterval}")
					)
				)
			);
			await (new MessageDialog($"{doc.ToString()}")).ShowAsync();
		}

		/// <summary>
		/// Selecting an event category
		/// </summary>
		public void CategorySelect()
		{
			Frame.Navigate(typeof(NewEventCreatePage));
		}
	}
}