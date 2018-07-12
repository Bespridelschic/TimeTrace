using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels.Navigation
{
    public class NavigationService : INavigationService<object>
    {
		public Stack<object> BackStack { get; }

		public void NavigateTo(object page)
		{
			throw new NotImplementedException();
		}

		public void NavigateBack()
		{
			throw new NotImplementedException();
		}
	}
}
