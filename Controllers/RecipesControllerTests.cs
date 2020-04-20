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
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            _imageHelper.Setup(x => x.GetImagePath(It.IsAny<string>())).Returns((Directory.GetCurrentDirectory() +   @"\deleteMe.jpeg"));

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
              .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
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
                    UserId = "User1",
                    IsPublic = false,
                    },
                    new Recipe
                    {
                         RecipeId = 2,
                    Name = "Herbata",
                    Ingredients = new List<string> { "Woda", "Herbata" },
                    Instruction = "Zagotuj wodę. Włóż esencję herbaty.",
                    NameOfImage = "herbata.jpeg",
                    UserId = "User1",
                      IsPublic = false,
                    },
                    new Recipe
                    {
                           RecipeId = 3,
                    Name = "Zupa",
                    Ingredients = new List<string> { "Woda", "Ziemniaki" },
                    Instruction = "Ugotuj ziemniaki w wodzie.",
                    NameOfImage = "pyszna_zupa.jpeg",
                    UserId = "User2",
                      IsPublic = false,
                    },
                    new Recipe
                    {
                         RecipeId = 4,
                    Name = "Frytki",
                    Ingredients = new List<string> { "Ziemniaki", "Sól", "Olej" },
                    Instruction = "Pokrój i usmaż ziemniaki. Posól.",
                    NameOfImage = "frytki.jpeg",
                    UserId = "User2",
                      IsPublic = true,
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
        public void IsRecipesControllerReturnsGetRecipesById()
        {
            //Act
            var result = _recipesController.GetRecipes(1);

            //Assert
            Assert.Equal("1", result.Result.Value.RecipeId);
            Assert.Equal("Kanapka", result.Result.Value.Name);
            Assert.Equal(2, result.Result.Value.Ingredients.ToList().Count());

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
        public void IsRecipesControllerReturnsGetPublicRecipes()
        {
            //Act
            var result = _recipesController.GetPublicRecipes();

            //Assert
            Assert.Equal(1, result.Value.Count);
            Assert.Equal("Frytki", result.Value[0].Name);
            Assert.Equal("Pokrój i usmaż ziemniaki. Posól.", result.Value[0].Instruction);
            Assert.Equal("frytki.jpeg", result.Value[0].NameOfImage);
            Assert.Equal(3, result.Value[0].Ingredients.ToList().Count());
        }


        [Fact]
        public void IsRecipesControllerPostRecipesWithoutImage()
        {
            //Arrange
            RecipeAPIModel recipeAPIModel = new RecipeAPIModel
            {
                Name = "Sałatka",
                Ingredients = new List<string> { "Sałata" },
                Instruction = "Pokrój sałatę.",
            };

            //Act
            var result = _recipesController.PostRecipes(recipeAPIModel);

            //Assert
            Assert.Equal("Microsoft.AspNetCore.Mvc.OkResult", result.Result.Result.ToString());
        }

        //Nie było zdjęcia - nie wysyłamy zdjęcia
        [Fact]
        public void IsRecipesControllerPutRecipesWithoutOldNewImage()
        {
            //Arrange
            RecipeAPIModel recipeAPIModel = new RecipeAPIModel
            {
                RecipeId = "1",
                Name = "KanapkaEDIT",
                Ingredients = new List<string> { "Chleb", "Masło" },
                Instruction = "Posmaruj chleb masłem. Smacznego!",
            };

            //Usun plik na potrzeby symulacji nie posiadania starego pliku
            if (File.Exists(Directory.GetCurrentDirectory() + @"\deleteMe.jpeg"))
            {
                File.Delete(Directory.GetCurrentDirectory() + @"\deleteMe.jpeg");
            }

            //Act
            var result = _recipesController.PutRecipes(1, recipeAPIModel);

            //Assert
            Assert.Equal("Microsoft.AspNetCore.Mvc.OkResult", result.Result.ToString());
        }

        [Fact]
        public void IsRecipesControllerDeleteRecipe()
        {
            //Act
            var result = _recipesController.DeleteRecipes(1);

            //Assert
            Assert.Equal("Microsoft.AspNetCore.Mvc.OkResult", result.Result.ToString());
        }

       

        [Fact]
        public void IsRecipesControllerGetPhoto()
        {
            //Arrange
            var fileMock = new Mock<IFormFile>();
            //Setup mock file using a memory stream
            var content = "Hello World from a Fake File";
            var fileName = "PysznaHerbata.jpeg";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(x => x.OpenReadStream()).Returns(ms);
            fileMock.Setup(x => x.FileName).Returns(fileName);
            fileMock.Setup(x => x.Length).Returns(ms.Length);
            fileMock.Setup(x => x.CopyToAsync(It.IsAny<Stream>(), CancellationToken.None)).Returns(Task.CompletedTask);

            var file = fileMock.Object;

            //Stworz plik na potrzeby symulacji pobrania pliku
            using (var fileStream = new FileStream(Directory.GetCurrentDirectory() + @"\deleteMe.jpeg", FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            //Act
            var result = _recipesController.GetPhoto("herbata.jpeg");

            //Assert
            Assert.Equal("Microsoft.AspNetCore.Mvc.FileStreamResult", result.ToString());
        }

        #region Unused


        ////Jest zdjęcie - nie wysyłamy zdjęcie
        //[Fact]
        //public void IsRecipesControllerPutRecipesWithoutNewImage()
        //{
        //    //Arrange
        //    RecipeAPIModel recipeAPIModel = new RecipeAPIModel
        //    {
        //        RecipeId = "2",
        //        Name = "HerbataEDIT",
        //        Ingredients = new List<string> { "Woda", "Herbata" },
        //        Instruction = "Zagotuj wodę. Włóż esencję herbaty.",
        //    };

        //    //Arrange
        //    var fileMock = new Mock<IFormFile>();
        //    //Setup mock file using a memory stream
        //    var content = "Hello World from a Fake File";
        //    var fileName = "salatka.jpeg";
        //    var ms = new MemoryStream();
        //    var writer = new StreamWriter(ms);
        //    writer.Write(content);
        //    writer.Flush();
        //    ms.Position = 0;
        //    fileMock.Setup(x => x.OpenReadStream()).Returns(ms);
        //    fileMock.Setup(x => x.FileName).Returns(fileName);
        //    fileMock.Setup(x => x.Length).Returns(ms.Length);
        //    fileMock.Setup(x => x.CopyToAsync(It.IsAny<Stream>(), CancellationToken.None)).Returns(Task.CompletedTask);

        //    var file = fileMock.Object;

        //    //Stworz plik na potrzeby symulacji posiadania starego pliku
        //    using (var fileStream = new FileStream(Directory.GetCurrentDirectory() + @"\deleteMe.jpeg", FileMode.Create))
        //    {
        //        file.CopyTo(fileStream);
        //    }

        //    //Act
        //    var result = _recipesController.PutRecipes(2, recipeAPIModel);

        //    //Assert
        //    Assert.Equal("Microsoft.AspNetCore.Mvc.OkResult", result.Result.ToString());
        //}

        ////Jest zdjęcie - Wysyłamy Zdjęcie
        //[Fact]
        //public void IsRecipesControllerPutRecipesWithOldNewImage()
        //{
        //    //Arrange
        //    var fileMock = new Mock<IFormFile>();
        //    //Setup mock file using a memory stream
        //    var content = "Hello World from a Fake File";
        //    var fileName = "PysznaHerbata.jpeg";
        //    var ms = new MemoryStream();
        //    var writer = new StreamWriter(ms);
        //    writer.Write(content);
        //    writer.Flush();
        //    ms.Position = 0;
        //    fileMock.Setup(x => x.OpenReadStream()).Returns(ms);
        //    fileMock.Setup(x => x.FileName).Returns(fileName);
        //    fileMock.Setup(x => x.Length).Returns(ms.Length);
        //    fileMock.Setup(x => x.CopyToAsync(It.IsAny<Stream>(), CancellationToken.None)).Returns(Task.CompletedTask);

        //    var file = fileMock.Object;

        //    //Stworz plik na potrzeby symulacji usuniecia starego pliku i zapisu nowego
        //    using (var fileStream = new FileStream(Directory.GetCurrentDirectory() + @"\deleteMe.jpeg", FileMode.Create))
        //    {
        //        file.CopyTo(fileStream);
        //    }

        //    RecipeAPIModel recipeAPIModel = new RecipeAPIModel
        //    {
        //        RecipeId = "2",
        //        Name = "HerbataEDIT",
        //        Ingredients = new List<string> { "Woda", "Herbata" },
        //        Instruction = "Zagotuj wodę. Włóż esencję herbaty.",
        //        NameOfImage = "PysznaHerbata.jpeg",
        //        Image = file
        //    };

        //    //Act
        //    var result = _recipesController.PutRecipes(2, recipeAPIModel);

        //    //Assert
        //    Assert.Equal("Microsoft.AspNetCore.Mvc.OkResult", result.Result.ToString());
        //}

        //[Fact]
        //public void IsRecipesControllerDeleteRecipeWithImage()
        //{
        //    //Arrange
        //    var fileMock = new Mock<IFormFile>();
        //    //Setup mock file using a memory stream
        //    var content = "Hello World from a Fake File";
        //    var fileName = "PysznaHerbata.jpeg";
        //    var ms = new MemoryStream();
        //    var writer = new StreamWriter(ms);
        //    writer.Write(content);
        //    writer.Flush();
        //    ms.Position = 0;
        //    fileMock.Setup(x => x.OpenReadStream()).Returns(ms);
        //    fileMock.Setup(x => x.FileName).Returns(fileName);
        //    fileMock.Setup(x => x.Length).Returns(ms.Length);
        //    fileMock.Setup(x => x.CopyToAsync(It.IsAny<Stream>(), CancellationToken.None)).Returns(Task.CompletedTask);

        //    var file = fileMock.Object;

        //    //Stworz plik na potrzeby symulacji usuniecia starego pliku
        //    using (var fileStream = new FileStream(Directory.GetCurrentDirectory() + @"\deleteMe.jpeg", FileMode.Create))
        //    {
        //        file.CopyTo(fileStream);
        //    }

        //    //Act
        //    var result = _recipesController.DeleteRecipes(2);

        //    //Assert
        //    Assert.Equal("Microsoft.AspNetCore.Mvc.OkResult", result.Result.ToString());
        //}

        //[Fact]
        //public void IsRecipesControllerPostRecipesWithImage()
        //{
        //    //Arrange
        //    var fileMock = new Mock<IFormFile>();
        //    //Setup mock file using a memory stream
        //    var content = "Hello World from a Fake File";
        //    var fileName = "salatka.jpeg";
        //    var ms = new MemoryStream();
        //    var writer = new StreamWriter(ms);
        //    writer.Write(content);
        //    writer.Flush();
        //    ms.Position = 0;
        //    fileMock.Setup(x => x.OpenReadStream()).Returns(ms);
        //    fileMock.Setup(x => x.FileName).Returns(fileName);
        //    fileMock.Setup(x => x.Length).Returns(ms.Length);
        //    fileMock.Setup(x => x.CopyToAsync(It.IsAny<Stream>(), CancellationToken.None)).Returns(Task.CompletedTask);

        //    var file = fileMock.Object;

        //    RecipeAPIModel recipeAPIModel = new RecipeAPIModel
        //    {
        //        Name = "Sałatka",
        //        Ingredients = new List<string> { "Sałata" },
        //        Instruction = "Pokrój sałatę.",
        //        Image = file
        //    };

        //    //Act
        //    var result = _recipesController.PostRecipes(recipeAPIModel);

        //    //Assert
        //    Assert.Equal("Microsoft.AspNetCore.Mvc.OkResult", result.Result.Result.ToString());
        //}
        #endregion
    }
}
