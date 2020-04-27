using Cook_Book_API.Helpers;
using Cook_Book_API.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Cook_Book_API_Tests.Helpers
{
    public class ImageHelperTests
    {
        private readonly ImageHelper _imageHelper;

        private readonly Mock<IHostEnvironment> _mockIHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ImageHelper> _logger;

        public ImageHelperTests()
        {
            //Arrange
             _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .Build();

            _mockIHostEnvironment = new Mock<IHostEnvironment>();
            _mockIHostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory);

            _logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ImageHelper>();

            _imageHelper = new ImageHelper(_configuration, _mockIHostEnvironment.Object, _logger);
        }

        [InlineData("fotka")]
        [InlineData("ceffb376-0bf3-453b-9780-955bb8835a91.jpeg")]
        [Theory]
        public void IsImageHelperReturnCorrectImagePath(string imageName)
        {
            //Arrange
            string expected = _mockIHostEnvironment.Object.ContentRootPath + "\\wwwroot" + _configuration["ImagePath"] + imageName;
            //Act
            string result = _imageHelper.GetImagePath(imageName);

            //Assert
            Assert.Equal(expected, result);
        }

        [InlineData(".exe")]
        [InlineData(".gif")]
        [InlineData(".jpeg")]
        [Theory]
        public void IsImageHelperCheckCorrectImageExtension(string extension)
        {
            //Arrange
            bool expected = true;
            if (extension == ".exe")
                expected = false;

            //Act
            bool result = _imageHelper.CheckCorrectExtension(extension);

            //Assert
            Assert.Equal(expected, result);
        }
    }
}
