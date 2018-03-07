using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace TimeTrace.ViewModel.MainViewModel.MapEventsViewModel
{
	/// <summary>
	/// Class for transition of Frame and target data
	/// </summary>
	/// <typeparam name="T">Transited data</typeparam>
    public class TransitionData<T> where T : class
    {
		/// <summary>
		/// Current frame of View
		/// </summary>
		public Frame Frame { get; private set; }
		/// <summary>
		/// Current data class
		/// </summary>
		public T Data { get; private set; }

		public TransitionData(Frame frame, T data)
		{
			Frame = frame;
			Data = data;
		}
	}
}
