using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models.UserModel;

namespace ModelUnitTests.UserTests
{
	[TestClass]
    public class UserTest
    {
		/// <summary>
		/// Checking correct email
		/// </summary>
		[TestMethod]
		public void CorrectEmailValidation()
		{
			User user = new User("test@gmail.com", "1234567890");
			Assert.AreEqual(0, user.Validation().Count);
		}

		/// <summary>
		/// Checking incorrect email
		/// </summary>
		[TestMethod]
		public void IncorrectEmailValidation()
		{
			User user = new User("test.gmail.com", "pass");
			Assert.AreEqual(2, user.Validation().Count);
		}
	}
}
