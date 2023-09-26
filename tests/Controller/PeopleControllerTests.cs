using Microsoft.AspNetCore.Mvc;
using Moq;
using revs_bens_service.Controllers;
using revs_bens_service.Services.Benefits;
using revs_bens_service.Services.CouncilTax;
using StockportGovUK.NetStandard.Gateways.Models.RevsAndBens;
using Xunit;

namespace revs_bens_service_tests.Controller
{
    public class PeopleControllerTests
    {
        private readonly PeopleController _controller;
        private readonly Mock<ICouncilTaxService> _mockCouncilTaxService = new Mock<ICouncilTaxService>();
        private readonly Mock<IBenefitsService> _mockBenefitsService = new Mock<IBenefitsService>();

        public PeopleControllerTests()
        {
            _mockBenefitsService
                .Setup(_ => _.GetBenefits(It.IsAny<string>()))
                .ReturnsAsync(new Claim());

            _mockBenefitsService
                .Setup(_ => _.IsBenefitsClaimant(It.IsAny<string>()))
                .ReturnsAsync(true);

            _mockCouncilTaxService
                .Setup(_ => _.GetDocumentForAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new byte[1]);

            _mockCouncilTaxService
                .Setup(_ => _.GetDocumentsForPerson(It.IsAny<string>()))
                .ReturnsAsync(new List<CouncilTaxDocument>());

            _mockCouncilTaxService
                .Setup(_ => _.GetCouncilTaxDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new CouncilTaxDetailsModel());

            _mockCouncilTaxService
                .Setup(_ => _.GetReducedCouncilTaxDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new CouncilTaxDetailsModel());

            _mockCouncilTaxService
                .Setup(_ => _.GetCurrentCouncilTaxAccountNumber(It.IsAny<string>()))
                .ReturnsAsync("123");

            _mockCouncilTaxService
                .Setup(_ => _.GetPerson(It.IsAny<string>()))
                .ReturnsAsync(new StockportGovUK.NetStandard.Gateways.Models.Civica.CouncilTax.PersonName());

            _controller = new PeopleController(_mockBenefitsService.Object, _mockCouncilTaxService.Object);
        }

        [Fact]
        public async Task IsBenefitsClaimant_ShouldCallBenefitsService()
        {
            // Act
            await _controller.IsBenefitsClaimant(It.IsAny<string>());

            // Assert
            _mockBenefitsService.Verify(_ => _.IsBenefitsClaimant(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task IsBenefitsClaimant_ShouldReturnBool()
        {
            // Act
            var result = await _controller.IsBenefitsClaimant(It.IsAny<string>());

            // Assert
            var resultObject = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<bool>(resultObject.Value);
        }

        [Fact]
        public async Task IsBenefitsClaimant_ShouldReturnOk()
        {
            // Act
            var result = await _controller.IsBenefitsClaimant("personReference");

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
        }

        [Fact]
        public async Task IsBenefitsClaimant_ShouldReturnNotFound()
        {
            // Arrange
            _mockBenefitsService
                .Setup(_ => _.IsBenefitsClaimant(It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException());

            // Act
            var result = await _controller.IsBenefitsClaimant("personReference");

            // Assert
            var actionResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, actionResult.StatusCode);
        }

        [Fact]
        public async Task IsBenefitsClaimant_ShouldReturn500()
        {
            // Arrange
            _mockBenefitsService
                .Setup(_ => _.IsBenefitsClaimant(It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _controller.IsBenefitsClaimant("personReference");

            // Assert
            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, actionResult.StatusCode);
        }

        [Fact]
        public async Task GetBenefits_ShouldCallBenefitsService()
        {
            // Act
            await _controller.GetBenefits(It.IsAny<string>());

            // Assert
            _mockBenefitsService.Verify(_ => _.GetBenefits(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetBenefits_ShouldReturnClaim()
        {
            // Act
            var result = await _controller.GetBenefits(It.IsAny<string>());

            // Assert
            var resultObject = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<Claim>(resultObject.Value);
        }

        [Fact]
        public async Task GetPerson_ShouldCallCouncilTaxService()
        {
            // Act
            await _controller.GetPerson(It.IsAny<string>());

            // Assert
            _mockCouncilTaxService.Verify(_ => _.GetPerson(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetPerson_ShouldReturnPersonNameType()
        {
            // Act
            var result = await _controller.GetPerson(It.IsAny<string>());

            // Assert
            var resultObject = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<StockportGovUK.NetStandard.Gateways.Models.Civica.CouncilTax.PersonName>(resultObject.Value);
        }

        [Fact]
        public async Task GetBaseCouncilTaxAccount_ShouldCallService()
        {
            // Act
            await _controller.GetBaseCouncilTaxAccount("personReference");

            // Assert
            _mockCouncilTaxService.Verify(_ => _.GetBaseCouncilTaxAccount(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetBaseCouncilTaxAccount_ShouldReturnOk()
        {
            // Act
            var result = await _controller.GetBaseCouncilTaxAccount("personReference");

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
        }

        [Fact]
        public async Task GetBaseCouncilTaxAccount_ShouldReturnNotFound()
        {
            // Arrange
            _mockCouncilTaxService
                .Setup(_ => _.GetBaseCouncilTaxAccount(It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException());

            // Act
            var result = await _controller.GetBaseCouncilTaxAccount("personReference");

            // Assert
            var actionResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, actionResult.StatusCode);
        }

        [Fact]
        public async Task GetBaseCouncilTaxAccount_ShouldReturn500()
        {
            // Arrange
            _mockCouncilTaxService
                .Setup(_ => _.GetBaseCouncilTaxAccount(It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _controller.GetBaseCouncilTaxAccount("personReference");

            // Assert
            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, actionResult.StatusCode);
        }

        [Fact]
        public async Task GetCurrentCouncilTaxAccountNumber_ShouldCallCouncilTaxService()
        {
            // Act
            await _controller.GetCurrentCouncilTaxAccountNumber("personReference");

            // Assert
            _mockCouncilTaxService.Verify(_ => _.GetCurrentCouncilTaxAccountNumber(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetCurrentCouncilTaxAccountNumber_ShouldReturnString()
        {
            // Act
            var result = await _controller.GetCurrentCouncilTaxAccountNumber("personReference");

            // Assert
            var resultObject = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<string>(resultObject.Value);
        }

        [Fact]
        public async Task GetReducedCouncilTaxDetails_ShouldCallCouncilTaxService()
        {
            // Act
            await _controller.GetReducedCouncilTaxDetails("personReference", "accountReference", 2021);

            // Assert
            _mockCouncilTaxService.Verify(_ => _.GetReducedCouncilTaxDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetReducedCouncilTaxDetails_ShouldReturnCouncilTaxDetailsModel()
        {
            // Act
            var result = await _controller.GetReducedCouncilTaxDetails("personReference", "accountReference", 2021);

            // Assert
            var resultObject = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<CouncilTaxDetailsModel>(resultObject.Value);
        }

        [Fact]
        public async Task GetCouncilTaxDetails_ShouldCallCouncilTaxService()
        {
            // Act
            await _controller.GetCouncilTaxDetails("personReference", "accountReference", 2021);

            // Assert
            _mockCouncilTaxService.Verify(_ => _.GetCouncilTaxDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetCouncilTaxDetails_ShouldReturnCouncilTaxDetailsModel()
        {
            // Act
            var result = await _controller.GetCouncilTaxDetails("personReference", "accountReference", 2021);

            // Assert
            var resultObject = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<CouncilTaxDetailsModel>(resultObject.Value);
        }

        [Fact]
        public async Task GetDocumentForAccount_ShouldCallCouncilTaxService()
        {
            // Act
            await _controller.GetDocumentForAccount("personReference", "accountReference", "documentId");

            // Assert
            _mockCouncilTaxService.Verify(_ => _.GetDocumentForAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetDocumentForAccount_ShouldReturnNotFound_IfDocumentIsNull()
        {
            // Arrange
            _mockCouncilTaxService
                .Setup(_ => _.GetDocumentForAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((byte[]) null);

            // Act
            var result  = await _controller.GetDocumentForAccount("personReference", "accountReference", "documentId");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetDocumentForAccount_ShouldReturnNoContent_IfByteArrayIsEmpty()
        {
            // Arrange
            _mockCouncilTaxService
                .Setup(_ => _.GetDocumentForAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new byte[0]);

            // Act
            var result = await _controller.GetDocumentForAccount("personReference", "accountReference", "documentId");

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetDocumentForAccount_ShouldReturnFileContentResult()
        {
            // Act
            var result = await _controller.GetDocumentForAccount("personReference", "accountReference", "documentId");

            // Assert
            Assert.IsType<FileContentResult>(result);
        }

        [Fact]
        public async Task GetDocumentsForPerson_ShouldCallCouncilTaxService()
        {
            // Act
            await _controller.GetDocumentsForPerson("personReference");

            // Assert
            _mockCouncilTaxService.Verify(_ => _.GetDocumentsForPerson(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetDocumentsForPerson_ShouldReturnNotFound_IfNullResponse()
        {
            // Arrange
            _mockCouncilTaxService
                .Setup(_ => _.GetDocumentsForPerson(It.IsAny<string>()))
                .ReturnsAsync((List<CouncilTaxDocument>) null);

            // Act
            var result = await _controller.GetDocumentsForPerson("personReference");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetDocumentsForPerson_ShouldReturnOk()
        {
            // Act
            var result = await _controller.GetDocumentsForPerson("personReference");

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
