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
	/// Абстрактный базовый класс ViewModel
	/// </summary>
	public abstract class BaseViewModel : INotifyPropertyChanged
	{
		/// <summary>
		/// Событие
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		/// <summary>
		/// Регистрация события
		/// </summary>
		/// <param name="property">Имя свойства</param>
		protected void OnPropertyChanged([CallerMemberName]string property = "")
		{
			PropertyChanged(this, new PropertyChangedEventArgs(property));
		}
	}
}
