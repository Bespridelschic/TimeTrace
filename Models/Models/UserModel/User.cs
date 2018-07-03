using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Models.Models.FileSystemRequests;

namespace Models.Models.UserModel
{
	/// <inheritdoc />
	/// <summary>
	/// User class
	/// </summary>
	public class User : INotifyPropertyChanged
	{
		#region Fields

		private string email;
		private string password;

		#endregion

		#region Properties

		public string Email
		{
			get => email.Trim();
			set
			{
				email = value;
				OnPropertyChanged();
			}
		}

		public string Password
		{
			get => password.Trim();
			set
			{
				password = value;
				OnPropertyChanged();
			}
		}

		public string Token { get; set; }

		#endregion

		#region Validation of entered data

		/// <summary>
		/// Validation of entered email
		/// </summary>
		/// <returns></returns>
		public bool EmailCorrectCheck()
		{
			string pattern = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
			var res = Regex.Match(Email, pattern);

			if (res.Success)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Password complexity level
		/// </summary>
		public enum PasswordScore
		{
			Blank = 0,
			VeryWeak = 1,
			Weak = 2,
			Medium = 3,
			Strong = 4,
			VeryStrong = 5
		}

		/// <summary>
		/// Checking the complexity of the password
		/// </summary>
		/// <returns>Level of security strong</returns>
		public PasswordScore PasswordSecurityCheck()
		{
			int score = 1;

			if (Password.Length < 1)
				return PasswordScore.Blank;
			if (Password.Length < 4)
				return PasswordScore.VeryWeak;

			if (Password.Length >= 8)
				score++;
			if (Password.Length >= 12)
				score++;
			if (Regex.Match(Password, @"/\d+/", RegexOptions.ECMAScript).Success)
				score++;
			if (Regex.Match(Password, @"/[a-z]/", RegexOptions.ECMAScript).Success
				&& Regex.Match(Password, @"/[A-Z]/", RegexOptions.ECMAScript).Success)
				score++;
			if (Regex.Match(Password, @"/.[!,@,#,$,%,^,&,*,?,_,~,-,£,(,)]/", RegexOptions.ECMAScript).Success)
				score++;

			return (PasswordScore)score;
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for set email and password as empty
		/// </summary>
		public User()
		{
			LoadUserDataFromFile();
		}

		/// <inheritdoc />
		/// <summary>
		/// Initializing new <seealso cref="T:Models.Models.UserModel.User" />
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

		public async void LoadUserDataFromFile()
		{
			await this.LoadUserEmailAndTokenFromFileAsync();
		}

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