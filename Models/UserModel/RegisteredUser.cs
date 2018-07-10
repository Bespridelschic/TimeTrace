using System.ComponentModel.DataAnnotations;

namespace Models.UserModel
{
	/// <summary>
	/// User heir for registration
	/// </summary>
    public sealed class RegisteredUser : User
    {
		[Compare(nameof(Password))]
		public string ConfirmPassword { get; set; }

		/// <summary>
		/// Empty constructor for base constructor call
		/// </summary>
		public RegisteredUser()
		{
			
		}

		/// <inheritdoc />
		/// <summary>
		/// Initializing new <seealso cref="T:Models.UserModel.User" />
		/// </summary>
		/// <param name="email">Email</param>
		/// <param name="password">Password</param>
		public RegisteredUser(string email, string password) : base(email, password)
		{
			
		}
	}
}