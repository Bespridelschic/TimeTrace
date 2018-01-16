using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TimeTrace.Model
{
	/// <summary>
	/// User class
	/// </summary>
	public class User : INotifyPropertyChanged
	{
		#region Properties

		private string email;
		[JsonProperty(PropertyName = "email")]
		public string Email
		{
			get { return email; }
			set
			{
				email = value;
				OnPropertyChanged("Email");
			}
		}

		private string password;
		[JsonProperty(PropertyName = "password")]
		public string Password
		{
			get { return password; }
			set
			{
				password = value;
				OnPropertyChanged("Password");
			}
		}

		private string lastName;
		[JsonProperty(PropertyName = "lastName")]
		public string LastName
		{
			get { return lastName; }
			set
			{
				lastName = value;
				OnPropertyChanged("LastName");
			}
		}

		private string firstName;
		[JsonProperty(PropertyName = "firstName")]
		public string FirstName
		{
			get { return firstName; }
			set
			{
				firstName = value;
				OnPropertyChanged("FirstName");
			}
		}

		private string middleName;
		[JsonProperty(PropertyName = "middleName")]
		public string MiddleName
		{
			get { return middleName; }
			set
			{
				middleName = value;
				OnPropertyChanged("MiddleName");
			}
		}

		private string birthday;
		[JsonProperty(PropertyName = "birthday")]
		public string Birthday
		{
			get { return birthday; }
			set
			{
				birthday = value;
				OnPropertyChanged("Birthday");
			}
		}

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
		/// <returns>Уровень сложности пароля</returns>
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
			if (Regex.Match(Password, @"/[a-z]/", RegexOptions.ECMAScript).Success &&
				Regex.Match(Password, @"/[A-Z]/", RegexOptions.ECMAScript).Success)
				score++;
			if (Regex.Match(Password, @"/.[!,@,#,$,%,^,&,*,?,_,~,-,£,(,)]/", RegexOptions.ECMAScript).Success)
				score++;

			return (PasswordScore)score;
		}

		#endregion

		#region Constructors

		public User()
		{
			Email = string.Empty;
			Password = string.Empty;

			LastName = string.Empty;
			FirstName = string.Empty;
			MiddleName = string.Empty;
			Birthday = string.Empty;
		}

		/// <summary>
		/// Initializing new <seealso cref="User"/>
		/// </summary>
		/// <param name="email">Email</param>
		/// <param name="password">Password</param>
		/// <param name="firstName">Name</param>
		/// <param name="lastName">Last name</param>
		/// <param name="middleName">Middle name</param>
		/// <param name="birthday">Birthday</param>
		public User(string email, string password, string firstName = "", string lastName = "", string middleName = "", string birthday = "")
		{
			Email = email;
			Password = password;

			LastName = lastName;
			FirstName = firstName;
			MiddleName = middleName;
			Birthday = birthday;
		}

		#endregion

		/// <summary>
		/// Hash algorithm SHA512 UTF8-UTF8
		/// </summary>
		/// <returns>Hash of password</returns>
		public string GetHashEncrypt()
		{
			SHA512 hash = SHA512.Create();
			var result = hash.ComputeHash(Encoding.UTF8.GetBytes(password));
			return Encoding.UTF8.GetString(result);
		}

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