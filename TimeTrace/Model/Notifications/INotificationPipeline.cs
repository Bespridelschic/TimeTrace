using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TimeTrace.Model.Events;

namespace TimeTrace.Model.Notifications
{
	public interface INotificationPipeline<T> where T : Category
	{
		/// <summary>
		/// Insert <see cref="T"/> into pipeline
		/// </summary>
		/// <param name="obj"></param>
		void Insert(T obj);

		/// <summary>
		/// Eject from pipeline
		/// </summary>
		/// <returns></returns>
		T Eject();

		/// <summary>
		/// Send push notification
		/// </summary>
		/// <param name="name"></param>
		/// <param name="dateTime"></param>
		void Notify(string name, DateTime dateTime);
	}
}
