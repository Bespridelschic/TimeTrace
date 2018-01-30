using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Windows.UI.Popups;

namespace TimeTrace.ViewModel.MainViewModel
{
	public class HomeViewModel : BaseViewModel
	{
		#region Properties

		private string experience;
		/// <summary>
		/// Total user experience time
		/// </summary>
		public string Experience
		{
			get { return experience; }
			set
			{
				experience = value;
				OnPropertyChanged();
			}
		}

		private string userName;
		/// <summary>
		/// Home Username view
		/// </summary>
		public string UserName
		{
			get { return userName; }
			set
			{
				userName = value;
				OnPropertyChanged();
			}
		}


		private int numContacts;
		/// <summary>
		/// Number of contacts
		/// </summary>
		public int NumContacts
		{
			get { return numContacts; }
			set
			{
				numContacts = value;
				OnPropertyChanged();
			}
		}

		private int numEvents;
		/// <summary>
		/// Total number of created events
		/// </summary>
		public int NumEvents
		{
			get { return numEvents; }
			set
			{
				numEvents = value;
				OnPropertyChanged();
			}
		}

		#endregion

		public HomeViewModel()
		{
			GetUserInfo();

			NumContacts = 0;
			NumEvents = 0;
			Experience = "0";
		}

		public async void GetUserInfo()
		{
			var userData = await Model.UserRequests.GetUserInfoPostRequestAsync();
			JObject jsonString = JObject.Parse(userData);

			NumContacts = (int?)jsonString["status"] ?? 0;
			var customer = (string)jsonString["customer"];

			JObject customerString = JObject.Parse(customer);

			Experience = $"{(int)DateTime.Now.Subtract((DateTime)customerString["created_at"]).TotalDays} дней";
			UserName = (string)customerString["firstName"] ?? ((string)customerString["email"]).Substring(0, ((string)customerString["email"]).IndexOf('@'));
		}
	}
}
