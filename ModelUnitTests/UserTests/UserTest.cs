using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models.UserModel;

namespace ModelUnitTests.UserTests
{
	[TestClass]
    internal class UserTest
    {
		/// <summary>
		/// Checking correct email
		/// </summary>
		[TestMethod]
		public void CorrectEmailValidation()
		{
			User user = new User("test@gmail.com", "pass");

			Assert.AreEqual(true, user.EmailCorrectCheck());
		}

		/// <summary>
		/// Checking incorrect email
		/// </summary>
		[TestMethod]
		public void InCorrectEmailValidation()
		{
			User user = new User("test.gmail.com", "pass");
			Assert.AreNotEqual(true, user.EmailCorrectCheck());

			user.Email = "test@gmail";
			Assert.AreNotEqual(true, user.EmailCorrectCheck());
		}
	}
}
