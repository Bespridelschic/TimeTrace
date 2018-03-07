using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTrace.Model.Events;
using TimeTrace.Model.Events.DBContext;
using TimeTrace.View.MainView.PersonalMapsCreatePages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TimeTrace.ViewModel.MainViewModel.MapEventsViewModel
{
	/// <summary>
	/// View model of project list
	/// </summary>
	public class ProjectViewModel : BaseViewModel
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
		/// Reseived area
		/// </summary>
		public Area CurrentArea { get; set; }

		private Project proj;
		/// <summary>
		/// Current project
		/// </summary>
		public Project Proj
		{
			get => proj;
			set
			{
				proj = value;
				OnPropertyChanged();
			}
		}

		#endregion

		public ProjectViewModel()
		{
			Proj = new Project
			{
				Name = "Random name",
			};

			//Proj.AreaId = CurrentArea.Id;
		}

		/// <summary>
		/// Navigate to map event create page
		/// </summary>
		/// <param name="sender">Object sender</param>
		/// <param name="e">Parameters</param>
		public void ProjectSelect(object sender, RoutedEventArgs e)
		{
			using (MapEventContext db = new MapEventContext())
			{
				//var selectedProject = db.Projects.First(i => i.Id == (string)(sender as Button).Tag);
				//TransitionData<Project> data = new TransitionData<Project>(Frame, selectedProject);

				Frame.Navigate(typeof(PersonalEventCreatePage), Frame); // , data
			}
		}

		/// <summary>
		/// Back to categories
		/// </summary>
		public void BackToCategories()
		{
			Frame.Navigate(typeof(CategorySelectPage), Frame);
		}
	}
}
