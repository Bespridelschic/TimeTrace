using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using ViewModels.Annotations;

namespace ViewModels
{
	/// <summary>
	/// Abstract base class of ViewModel
	/// </summary>
	internal abstract class BaseViewModel : INotifyPropertyChanged
	{
		/// <summary>
		/// Called event
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		/// <summary>
		/// Event registration
		/// </summary>
		/// <param name="propertyName">Caller properties name</param>
		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
