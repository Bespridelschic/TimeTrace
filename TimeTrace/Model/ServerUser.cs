using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrace.Model
{
	public class ServerUser : User, INotifyPropertyChanged
	{
		#region Properties

		private DateTime created_at;
		[JsonProperty(PropertyName = "created_at")]
		public DateTime Created_at
		{
			get { return created_at; }
			set
			{
				created_at = value;
				OnPropertyChanged();
			}
		}

		private string status;
		[JsonProperty(PropertyName = "status")]
		public string Status
		{
			get { return status; }
			set
			{
				status = value;
				OnPropertyChanged();
			}
		}

		#endregion

		public ServerUser()
		{

		}

		#region MVVM

		public new event PropertyChangedEventHandler PropertyChanged = delegate { };
		private void OnPropertyChanged([CallerMemberName]string prop = "")
		{
			PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}

		#endregion
	}
}