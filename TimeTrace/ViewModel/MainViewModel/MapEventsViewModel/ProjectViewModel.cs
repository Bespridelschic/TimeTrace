using System;
using System.Collections.Generic;
using System.Diagnostics;
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

				(Application.Current as App).AppFrame.Navigate(typeof(PersonalEventCreatePage), Proj.Id);
			}
		}
	}
}
