using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrace.ViewModel
{
	/// <summary>
	/// Abstract base class of ViewModel
	/// </summary>
	public abstract class BaseViewModel : INotifyPropertyChanged
	{
		/// <summary>
		/// Event
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		/// <summary>
		/// Event registration
		/// </summary>
		/// <param name="property">Caller properties name</param>
		protected void OnPropertyChanged([CallerMemberName]string property = "")
		{
			PropertyChanged(this, new PropertyChangedEventArgs(property));
		}
	}
}
