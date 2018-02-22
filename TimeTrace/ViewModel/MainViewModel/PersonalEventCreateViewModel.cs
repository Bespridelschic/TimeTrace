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
using System.Runtime.Serialization.Json;
using System.IO;
using TimeTrace.Model.DBContext;

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
					CurrentMapEvent.EndDate = null;
					CurrentMapEvent.EndTime = TimeSpan.Parse("00:00");
				}

				OnPropertyChanged();
			}
		}

		public Frame Frame { get; set; }

		#endregion

		/// <summary>
		/// Standart constructor
		/// </summary>
		public PersonalEventCreateViewModel(string areaId)
		{
			CurrentMapEvent = new MapEvent(areaId);
			CurrentMapEvent.StartDate = DateTime.Now;
			MinDate = DateTime.Today;
			IsNotAllDay = false;

			BindingObjectIndex = 0;
		}

		/// <summary>
		/// Creating a new event
		/// </summary>
		/// <returns>Result of event creation</returns>
		public async Task EventCreate()
		{
			if (string.IsNullOrEmpty(CurrentMapEvent.Name) || CurrentMapEvent.StartDate == null || (CurrentMapEvent.EndDate == null && IsNotAllDay))
			{
				await (new MessageDialog("Не заполнено одно из обязательных полей", "Ошибка создания нового события")).ShowAsync();

				return;
			}

			if (!IsNotAllDay)
			{
				CurrentMapEvent.EndTime = TimeSpan.Parse("23:59");
				CurrentMapEvent.EndDate = CurrentMapEvent.StartDate;
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

			using (MapEventContext db = new MapEventContext())
			{
				db.MapEvents.Add(CurrentMapEvent);
				db.SaveChanges();
			}
		}

		/// <summary>
		/// Selecting an event category
		/// </summary>
		public void CategorySelect()
		{
			Frame.Navigate(typeof(PersonalEventCreatePage), Frame);
		}

		/// <summary>
		/// Cancell event creation and back to categories
		/// </summary>
		public void BackToCategories()
		{
			Frame.Navigate(typeof(CategorySelectPage), Frame);
		}
	}
}