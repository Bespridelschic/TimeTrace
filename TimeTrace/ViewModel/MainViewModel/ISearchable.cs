using System.Collections.ObjectModel;

namespace TimeTrace.ViewModel.MainViewModel
{
	/// <summary>
	/// Realization of searching of entities in database
	/// </summary>
	/// <typeparam name="T">Type of searching object</typeparam>
	internal interface ISearchable<T>
	{
		/// <summary>
		/// Term for searching
		/// </summary>
		T SearchTerm { get; set; }

		/// <summary>
		/// Suggestions for searching
		/// </summary>
		ObservableCollection<T> SearchSuggestions { get; set; }

		/// <summary>
		/// Filtration of input terms
		/// </summary>
		void DynamicSearch();

		/// <summary>
		/// User request for searching
		/// </summary>
		void SearchRequest();
	}
}
