using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TimeTrace.Model;

namespace TimeTrace.ModelView
{
	public class UserViewModel : INotifyPropertyChanged
	{
		private User User { get; set; }

		public UserViewModel()
		{

		}

		public event PropertyChangedEventHandler PropertyChanged = delegate { };
		private void OnPropertyChanged([CallerMemberName]string prop = "")
		{
			PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}
	}
}
