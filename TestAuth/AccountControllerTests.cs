using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using WebAPI.Model;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using WebAPI.Utilities;

namespace AuthServer.Tests
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly AccountController _accountController;

        public AccountControllerTests()
        {
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                new Mock<IUserStore<IdentityUser>>().Object, null, null, null, null, null, null, null, null);

            _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
                _userManagerMock.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object,
                null, null, null, null);

            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(config => config["Jwt:Key"]).Returns("wQjEX5n1Jh8Qg6Q+1Xq5/4TQ5xU1Vn9N8Lg/aO/wuRg=");
            _configurationMock.Setup(config => config["Jwt:Issuer"]).Returns("https://auth.yourdomain.com");
            _configurationMock.Setup(config => config["Jwt:Audience"]).Returns("https://api.yourdomain.com");

            _accountController = new AccountController(_userManagerMock.Object, _signInManagerMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenUserIsCreated()
        {
            // Arrange
            var registerModel = new RegisterModel { Username = "testuser", Email = "testuser@example.com", Password = "Password123!" };
            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _accountController.Register(registerModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value as dynamic;
            Assert.Equal(ResponseMessages.AccounCreate, value);
        }

        [Fact]
        public async Task Login_ReturnsOk_WithJwtToken()
        {
            // Arrange
            var loginModel = new LoginModel { Username = "testuser", Password = "Password123!" };
            _signInManagerMock.Setup(sm => sm.PasswordSignInAsync(loginModel.Username, loginModel.Password, false, false))
                .ReturnsAsync(SignInResult.Success);
            _userManagerMock.Setup(um => um.FindByNameAsync(loginModel.Username))
                .ReturnsAsync(new IdentityUser { UserName = loginModel.Username });

            // Act
            var result = await _accountController.Login(loginModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var token = okResult.Value;
            Assert.NotNull(token);
        }
    }
}
