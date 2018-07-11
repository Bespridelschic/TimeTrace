using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels
{
    public interface INavigationService<T>
	{
		Stack<T> BackStack { get; }

		void NavigateTo(T page);
		void NavigateBack();
	}
}
