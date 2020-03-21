using Cook_Book_API.Controllers;
using Cook_Book_API.Data;
using Cook_Book_API.Data.DbModels;
using Cook_Book_API.Interfaces;
using Cook_Book_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace Cook_Book_API_Tests.Controllers
{
    public class RecipesControllerTests
    {
        private RecipesController _recipesController;

        private readonly ILogger<RecipesController> _logger;
        private readonly Mock<IImageHelper> _imageHelper;

        public RecipesControllerTests()
        {
            //Arrange
            _logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<RecipesController>();

            _imageHelper = new Mock<IImageHelper>();
            _imageHelper.Setup(x => x.GetImagePath(It.IsAny<string>())).Returns(@"C:\Users\Jan\Desktop\Obrazek.jpeg");

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
              .UseInMemoryDatabase(databaseName: "ApplicationDbContextTest")
              .Options;

            using (var context = new ApplicationDbContext(options))
            {
                List<ApplicationUser> listUsers = new List<ApplicationUser>
                {
                    new ApplicationUser
                    {
                         Id = "User1",
                    UserName = "Jan",
                    Email = "User1@email.com",
                    },
                    new ApplicationUser
                    {
                        Id = "User2",
                    UserName = "Krzysztof",
                    Email = "Krzysztof@email.com",
                    }
                };

                List<Recipe> listRecipes = new List<Recipe>
                {
                    new Recipe
                    {
                         RecipeId = 1,
                    Name = "Kanapka",
                    Ingredients = new List<string> { "Chleb", "Masło" },
                    Instruction = "Posmaruj chleb masłem. Smacznego!",
                    NameOfImage = "kanapka.jpeg",
                    UserId = "User1"
                    },
                    new Recipe
                    {
                         RecipeId = 2,
                    Name = "Herbata",
                    Ingredients = new List<string> { "Woda", "Herbata" },
                    Instruction = "Zagotuj wodę. Włóż esencję herbaty.",
                    NameOfImage = "herbata.jpeg",
                    UserId = "User1"
                    },
                    new Recipe
                    {
                           RecipeId = 3,
                    Name = "Zupa",
                    Ingredients = new List<string> { "Woda", "Ziemniaki" },
                    Instruction = "Ugotuj ziemniaki w wodzie.",
                    NameOfImage = "pyszna_zupa.jpeg",
                    UserId = "User2"
                    },
                    new Recipe
                    {
                         RecipeId = 4,
                    Name = "Frytki",
                    Ingredients = new List<string> { "Ziemniaki", "Sól", "Olej" },
                    Instruction = "Pokrój i usmaż ziemniaki. Posól.",
                    NameOfImage = "frytki.jpeg",
                    UserId = "User2"
                    }
                };

                context.Users.AddRange(listUsers);
                context.Recipes.AddRange(listRecipes);
                context.SaveChanges();
            }

            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
           {
                 new Claim(ClaimTypes.NameIdentifier, "User1"),
           }, "mock"));

            _recipesController = new RecipesController(new ApplicationDbContext(options), _logger, _imageHelper.Object);

            _recipesController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userClaimsPrincipal }
            };
        }

        [Fact]
        public void IsRecipesControllerReturnsGetUserRecipes()
        {
            //Act
            var result = _recipesController.GetUserRecipes();

            //Assert
            Assert.Equal(2, result.Value.Count);
            Assert.Equal("Kanapka", result.Value[0].Name);
            Assert.Equal("Posmaruj chleb masłem. Smacznego!", result.Value[0].Instruction);
            Assert.Equal("herbata.jpeg", result.Value[1].NameOfImage);
            Assert.Equal(2, result.Value[1].Ingredients.ToList().Count());

        }

        [Fact]
        public void IsRecipesControllerPostRecipes()
        {
            //Arrange
            RecipeAPIModel recipeAPIModel = new RecipeAPIModel
            {
                Name = "Sałatka",
                Ingredients = new List<string> { "Sałata" },
                Instruction = "Pokrój sałatę.",
                NameOfImage = "yumisalat.jpeg"
            };

            //Act
            var result = _recipesController.PostRecipes(recipeAPIModel);

            //Assert
            Assert.Equal("Microsoft.AspNetCore.Mvc.OkResult", result. Result.Result.ToString());
            //Zrobic cos z contextem zeby byl globalny
            //I form
            // Czy dac name of image?
        }
    }
}
