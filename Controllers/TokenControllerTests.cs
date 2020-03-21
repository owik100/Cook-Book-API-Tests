using Cook_Book_API.Controllers;
using Cook_Book_API.Data.DbModels;
using Cook_Book_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cook_Book_API_Tests.Controllers
{
    public class TokenControllerTests
    {
        private readonly TokenController _tokenController;

        private readonly ILogger<TokenController> _logger;
        private readonly IConfiguration _configuration;
        private readonly Mock<UserManager<IdentityUser>> _userManager;

        public TokenControllerTests()
        {
            //Arrange
            _logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<TokenController>();

            _configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.test.json")
               .Build();

            List<IdentityUser> _users = new List<IdentityUser>
            {
               new ApplicationUser { Id = "1",UserName="User1", Email ="user1@gmail.com"  },
               new ApplicationUser { Id = "2" ,UserName="User2", Email ="user2@gmail.com"}
            };

            _userManager = MockUserManager<IdentityUser>(_users);
            _tokenController = new TokenController(_userManager.Object, _configuration, _logger);
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
            mgr.Setup(x => x.CheckPasswordAsync(It.IsAny<TUser>(), It.IsAny<string>())).ReturnsAsync(true);
            mgr.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(ls[0]);

            return mgr;
        }

        [Fact]
        public async Task IsTokenControllerGenerateToken()
        {
            //Act
            var result = await _tokenController.GetToken("User1", "password");
            
            //Assert
            Assert.NotNull(result.Value.UserName);
            Assert.NotNull(result.Value.Access_Token);
        }
    }
}
