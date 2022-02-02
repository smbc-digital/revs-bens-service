using Microsoft.AspNetCore.Mvc;
using Moq;
using revs_bens_service.Controllers;
using revs_bens_service.Services.Benefits;
using revs_bens_service.Services.CouncilTax;
using StockportGovUK.NetStandard.Models.RevsAndBens;
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
                .Setup(_ => _.GetCouncilTaxDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new CouncilTaxDetailsModel());

            _mockCouncilTaxService
                .Setup(_ => _.GetReducedCouncilTaxDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new CouncilTaxDetailsModel());

            _mockCouncilTaxService
                .Setup(_ => _.GetCurrentCouncilTaxAccountNumber(It.IsAny<string>()))
                .ReturnsAsync("123");

            _controller = new PeopleController(_mockBenefitsService.Object, _mockCouncilTaxService.Object);
        }

        [Fact]
        public async void IsBenefitsClaimant_ShouldCallBenefitsService()
        {
            // Act
            await _controller.IsBenefitsClaimant(It.IsAny<string>());

            // Assert
            _mockBenefitsService.Verify(_ => _.IsBenefitsClaimant(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async void IsBenefitsClaimant_ShouldReturnBool()
        {
            // Act
            var result = await _controller.IsBenefitsClaimant(It.IsAny<string>());

            // Assert
            var resultObject = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<bool>(resultObject.Value);
        }

        [Fact]
        public async void GetBenefits_ShouldCallBenefitsService()
        {
            // Act
            await _controller.GetBenefits(It.IsAny<string>());

            // Assert
            _mockBenefitsService.Verify(_ => _.GetBenefits(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async void GetBenefits_ShouldReturnClaim()
        {
            // Act
            var result = await _controller.GetBenefits(It.IsAny<string>());

            // Assert
            var resultObject = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<Claim>(resultObject.Value);
        }

        [Fact]
        public async void GetCurrentCouncilTaxAccountNumber_ShouldCallCouncilTaxService()
        {
            // Act
            await _controller.GetCurrentCouncilTaxAccountNumber("personReference");

            // Assert
            _mockCouncilTaxService.Verify(_ => _.GetCurrentCouncilTaxAccountNumber(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async void GetCurrentCouncilTaxAccountNumber_ShouldReturnString()
        {
            // Act
            var result = await _controller.GetCurrentCouncilTaxAccountNumber("personReference");

            // Assert
            var resultObject = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<string>(resultObject.Value);
        }

        [Fact]
        public async void GetReducedCouncilTaxDetails_ShouldCallCouncilTaxService()
        {
            // Act
            await _controller.GetReducedCouncilTaxDetails("personReference", "accountReference", 2021);

            // Assert
            _mockCouncilTaxService.Verify(_ => _.GetReducedCouncilTaxDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async void GetReducedCouncilTaxDetails_ShouldReturnCouncilTaxDetailsModel()
        {
            // Act
            var result = await _controller.GetReducedCouncilTaxDetails("personReference", "accountReference", 2021);

            // Assert
            var resultObject = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<CouncilTaxDetailsModel>(resultObject.Value);
        }

        [Fact]
        public async void GetCouncilTaxDetails_ShouldCallCouncilTaxService()
        {
            // Act
            await _controller.GetCouncilTaxDetails("personReference", "accountReference", 2021);

            // Assert
            _mockCouncilTaxService.Verify(_ => _.GetCouncilTaxDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async void GetCouncilTaxDetails_ShouldReturnCouncilTaxDetailsModel()
        {
            // Act
            var result = await _controller.GetCouncilTaxDetails("personReference", "accountReference", 2021);

            // Assert
            var resultObject = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<CouncilTaxDetailsModel>(resultObject.Value);
        }

        [Fact]
        public async void GetDocumentForAccount_ShouldCallCouncilTaxService()
        {
            // Act
            await _controller.GetDocumentForAccount("personReference", "accountReference", "documentId");

            // Assert
            _mockCouncilTaxService.Verify(_ => _.GetDocumentForAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async void GetDocumentForAccount_ShouldReturnNotFound_IfDocumentIsNull()
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
        public async void GetDocumentForAccount_ShouldReturnNoContent_IfByteArrayIsEmpty()
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
        public async void GetDocumentForAccount_ShouldReturnFileContentResult()
        {
            // Act
            var result = await _controller.GetDocumentForAccount("personReference", "accountReference", "documentId");

            // Assert
            Assert.IsType<FileContentResult>(result);
        }
    }
}
