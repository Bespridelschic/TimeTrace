using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ViewModels.Dialogs
{
	/// <summary>
	/// Notifying the user of new dialog
	/// </summary>
	public interface IDialogNotification
    {
		/// <summary>
		/// Source for subscribing for a new dialog
		/// </summary>
		event Func<string[], string, bool> DialogNotification;

		/// <summary>
		/// Invocation of dialog notification
		/// </summary>
		/// <param name="data">List of <seealso cref="string"/> data</param>
		/// <param name="title">Title of message</param>
		/// <returns></returns>
		Task MessageInvokeAsync(List<string> data, string title);
	}
}
