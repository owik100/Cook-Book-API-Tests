using Cook_Book_API.Controllers;
using Cook_Book_API.Data;
using Cook_Book_API.Data.DbModels;
using Cook_Book_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cook_Book_API_Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly AccountController _accountController;

        private readonly ILogger<AccountController> _logger;
        private readonly Mock<UserManager<IdentityUser>> _userManager;

        private List<IdentityUser> _users = new List<IdentityUser>
        {
          new ApplicationUser { Id = "1",UserName="User1", Email ="user1@bv.com" },
          new ApplicationUser { Id = "2" ,UserName="User2", Email ="user2@bv.com"}
        };

        public AccountControllerTests()
        { 
            //Arrange
            _logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<AccountController>();

            _userManager = MockUserManager<IdentityUser>(_users);
            _accountController = new AccountController(_userManager.Object, _logger);

        }
        public static Mock<UserManager<TUser>> MockUserManager<TUser>(List<TUser> ls) where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<TUser>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

            mgr.Setup(x => x.DeleteAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.CreateAsync(It.IsAny<TUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Callback<TUser, string>((x, y) => ls.Add(x));
            mgr.Setup(x => x.UpdateAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);

            return mgr;
        }

        [Fact]
        public async Task CreateAUser()
        {
            RegisterModel registerModel = new RegisterModel
            {
                Email = "New@test.com",
                UserName = "Kazimierz",
                Password = "P@ssw0rd!",
                ConfirmPassword = "P@ssw0rd!"
            };

            //Act
            var result = await _accountController.Register(registerModel);

            //Assert
            Assert.Equal(3, _users.Count);
            Assert.Equal("Microsoft.AspNetCore.Mvc.OkResult", result.ToString());
        }

    }
}
