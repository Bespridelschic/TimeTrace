using System;
using System.Threading.Tasks;

namespace ViewModels
{
	/// <summary>
	/// Notifying the user of new messages
	/// </summary>
	public interface IMessageNotification
	{
		/// <summary>
		/// Source for subscribing for a new messages
		/// </summary>
		event Action<string, string> MessageNotification;

		/// <summary>
		/// Invocation of message notification
		/// </summary>
		/// <param name="message">Body of message</param>
		/// <param name="title">Title of message</param>
		/// <returns></returns>
		Task MessageInvokeAsync(string message, string title = "");
	}
}