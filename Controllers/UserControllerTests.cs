﻿using AutoMapper;
using Cook_Book_API;
using Cook_Book_API.Controllers;
using Cook_Book_API.Data;
using Cook_Book_API.Data.DbModels;
using Cook_Book_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace Cook_Book_API_Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly UserController _userController;

        private readonly ILogger<UserController> _logger;
        public UserControllerTests()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
               .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
               .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var user = new ApplicationUser
                {
                    Id = "User1",                  
                    UserName = "Jan",
                    Email = "User1@email.com",
                };

                var user2 = new ApplicationUser
                {
                    Id = "User2",
                    UserName = "Krzysztof",
                    Email = "Krzysztof@email.com",
                };

                context.Users.Add(user);
                context.Users.Add(user2);
                context.SaveChanges();
            }

            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                 new Claim(ClaimTypes.NameIdentifier, "User1"),
            }, "mock"));


            var config = new MapperConfiguration(opts =>
            {
                opts.AddProfile(new MappingProfile());

            });

            var mapper = config.CreateMapper(); // Use this mapper to instantiate your class


            _logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<UserController>();
            _userController = new UserController(new ApplicationDbContext(options), _logger, mapper);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userClaimsPrincipal }
            };
        }

        [InlineData("User1", "Jan", "User1@email.com")]
        [Theory]
        public void IsUserControllerReturnCorrectUserInfo(string userID, string userName, string Email)
        {
            //Act
            var result = _userController.GetUserInfo();

            //Assert
            Assert.Equal(userID, result.Id);
            Assert.Equal(userName, result.UserName);
            Assert.Equal(Email, result.Email);
        }


        [InlineData("User1")]
        [Theory]
        public void IsUserControllerUpdateUser(string userID)
        {
            //Arrange
            LoggedUserModel loggedUserModel = new LoggedUserModel
            {
                Id = "User1",
                Email = "User1@email.com",
                UserName = "Jan",
                FavouriteRecipes = new List<string> { "100", "101" }
            };

            //Act
            var result = _userController.PutUser(userID, loggedUserModel);

            //Assert

            Assert.Equal("Microsoft.AspNetCore.Mvc.OkResult", result.Result.ToString());
        }
    }
}
