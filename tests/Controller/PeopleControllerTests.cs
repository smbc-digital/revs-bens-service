using Microsoft.AspNetCore.Mvc;
using Moq;
using revs_bens_service.Controllers;
using revs_bens_service.Services.Benefits;
using revs_bens_service.Services.CouncilTax;
using revs_bens_service.Services.Models;
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
            _controller = new PeopleController(_mockBenefitsService.Object, _mockCouncilTaxService.Object);
        }

        [Fact]
        public async void IsBenefitsClaimant_ShouldCallBenefitsService()
        {
            // Arrange
            _mockBenefitsService
                .Setup(_ => _.IsBenefitsClaimant(It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<bool>());

            // Act
            await _controller.IsBenefitsClaimant(It.IsAny<string>());

            // Assert
            _mockBenefitsService.Verify(_ => _.IsBenefitsClaimant(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async void IsBenefitsClaimant_ShouldReturnOk()
        {
            // Arrange
            _mockBenefitsService
                .Setup(_ => _.IsBenefitsClaimant(It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<bool>());

            // Act
            var result = await _controller.IsBenefitsClaimant(It.IsAny<string>());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async void IsBenefitsClaimant_ShouldReturnBool()
        {
            // Arrange
            _mockBenefitsService
                .Setup(_ => _.IsBenefitsClaimant(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.IsBenefitsClaimant(It.IsAny<string>());

            // Assert
            var resultObject = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<bool>(resultObject.Value);
        }

        [Fact]
        public async void GetBenefits_ShouldCallBenefitsService()
        {
            // Arrange
            _mockBenefitsService
                .Setup(_ => _.GetBenefits(It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<Claim>());

            // Act
            await _controller.GetBenefits(It.IsAny<string>());

            // Assert
            _mockBenefitsService.Verify(_ => _.GetBenefits(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async void GetBenefits_ShouldReturnOk()
        {
            // Arrange
            _mockBenefitsService
                .Setup(_ => _.GetBenefits(It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<Claim>());

            // Act
            var result = await _controller.GetBenefits(It.IsAny<string>());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async void GetBenefits_ShouldReturnClaim()
        {
            // Arrange
            _mockBenefitsService
                .Setup(_ => _.GetBenefits(It.IsAny<string>()))
                .ReturnsAsync(new Claim());

            // Act
            var result = await _controller.GetBenefits(It.IsAny<string>());

            // Assert
            var resultObject = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<Claim>(resultObject.Value);
        }

        [Fact]
        public async void GetCouncilTaxDetails_ShouldCallCouncilTaxService()
        {
            // Arrange
            _mockCouncilTaxService
                .Setup(_ => _.GetCouncilTaxDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(It.IsAny<CouncilTaxDetailsModel>());

            // Act
            await _controller.GetCouncilTaxDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>());

            // Assert
            _mockCouncilTaxService.Verify(_ => _.GetCouncilTaxDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async void GetCouncilTaxDetails_ShouldReturnOk()
        {
            // Arrange
            _mockCouncilTaxService
                .Setup(_ => _.GetCouncilTaxDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(It.IsAny<CouncilTaxDetailsModel>());

            // Act
            var result = await _controller.GetCouncilTaxDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async void GetCouncilTaxDetails_ShouldReturnCouncilTaxDetailsModel()
        {
            // Arrange
            _mockCouncilTaxService
                .Setup(_ => _.GetCouncilTaxDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new CouncilTaxDetailsModel());

            // Act
            var result = await _controller.GetCouncilTaxDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>());

            // Assert
            var resultObject = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<CouncilTaxDetailsModel>(resultObject.Value);
        }
    }
}
