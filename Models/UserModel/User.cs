using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Models.FileSystemRequests;

namespace Models.UserModel
{
	/// <inheritdoc />
	/// <summary>
	/// User class
	/// </summary>
	[DataContract]
	public class User : INotifyPropertyChanged
	{
		#region Fields

		private string email;
		private string password;

		#endregion

		#region Properties

		[DataMember]
		[Required]
		[EmailAddress]
		public string Email
		{
			get => email?.Trim();
			set
			{
				email = value;
				OnPropertyChanged();
			}
		}

		[DataMember]
		[StringLength(30, MinimumLength = 8)]
		public string Password
		{
			get => password?.Trim();
			set
			{
				password = value;
				OnPropertyChanged();
			}
		}

		[DataMember]
		public string Token { get; set; }

		#endregion

		/// <summary>
		/// Validation of object
		/// </summary>
		/// <returns>Null is error of validation. Empty list is good validation</returns>
		public IList<string> Validation()
		{
			var results = new List<ValidationResult>();
			var context = new ValidationContext(this);

			IList<string> listOfErrors = new List<string>();

			if (!Validator.TryValidateObject(this, context, results, true))
			{
				foreach (var error in results)
				{
					listOfErrors.Add(error.ErrorMessage);
				}
			}

			return listOfErrors;
		}

		#region Constructors

		/// <summary>
		/// Constructor for set email and password as empty
		/// </summary>
		public User()
		{
			LoadUserDataFromFileAsync().GetAwaiter().GetResult();
		}

		/// <inheritdoc />
		/// <summary>
		/// Initializing new <seealso cref="T:Models.UserModel.User" />
		/// </summary>
		/// <param name="email">Email</param>
		/// <param name="password">Password</param>
		public User(string email, string password) : this()
		{
			Email = email;
			Password = password;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Async loading user data from local files
		/// </summary>
		public async Task LoadUserDataFromFileAsync() => await new FileSystemRequests.FileSystemRequests().LoadUserEmailAndTokenFromFileAsync(this);

		#endregion

		#region Hash encryption

		/// <summary>
		/// Hash algorithm SHA512 UTF8-UTF8
		/// </summary>
		/// <returns>Hash of Email + Password</returns>
		public string GetHashEncrypt()
		{
			SHA512 hash = SHA512.Create();

			var result = hash.ComputeHash(Encoding.UTF8.GetBytes(Email.ToLowerInvariant() + Password));
			var res = string.Empty;

			foreach (var item in result)
			{
				res += item.ToString();
			}

			return res;
		}

		/// <summary>
		/// Hash algorithm SHA512 UTF8-UTF8
		/// </summary>
		/// <param name="text">Text for encrypting</param>
		/// <returns>Hash of parameters strings</returns>
		public string GetHashEncrypt(string text)
		{
			SHA512 hash = SHA512.Create();
			var result = hash.ComputeHash(Encoding.UTF8.GetBytes(text));
			return Encoding.UTF8.GetString(result);
		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			return $"{Email}: {Password}";
		}

		#endregion

		#region MVVM

		public event PropertyChangedEventHandler PropertyChanged = delegate { };
		private void OnPropertyChanged([CallerMemberName]string prop = "")
		{
			PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}

		#endregion
	}
}