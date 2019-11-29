using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Moq;
using Newtonsoft.Json;
using revs_bens_service.Services.Benefits;
using revs_bens_service.Utils.StorageProvider;
using StockportGovUK.AspNetCore.Gateways.CivicaServiceGateway;
using StockportGovUK.NetStandard.Models.RevsAndBens;
using Xunit;

namespace revs_bens_service_tests.Service
{
    public class BenefitsServiceTests
    {
        private readonly BenefitsService _service;
        private readonly Mock<ICivicaServiceGateway> _mockGateway = new Mock<ICivicaServiceGateway>();
        private readonly Mock<ICacheProvider> _cache = new Mock<ICacheProvider>();

        #region Test Models

        private readonly string mockListBenefitClaimSummary = JsonConvert.SerializeObject(new List<BenefitsClaimSummary>
        {
            new BenefitsClaimSummary
            {
                Status = "Current",
                PlaceReference = "123",
                Address = "address",
                Number = "123",
                PersonType = "type"
            }
        });

        private readonly string mockBenefitsClaim = JsonConvert.SerializeObject(new BenefitsClaim
        {
            Status = "1",
            Type = "type",
            Number = "1234",
            NextPayment = new CivicaNextPayment
            {
                Amount = "20.00",
                Method = "cash",
                PaidUpToAmount = "20.00",
                Payee = "payee",
                PaymentDueDate = "12-12-2018",
                Schedule = "schedule",
                Status = "status"
            },
            Address1 = "address1",
            Address2 = "address2",
            Postcode = "postcode",
            BenefitEntitlement = new BenefitEntitlement
            {
                CouncilTax = new CouncilTaxEntitlement
                {
                    WeeklyBenefit = "10.00"
                },
                PrivateRent = new HousingBenefitEntitlement
                {
                    WeeklyBenefit = "10.00"
                }
            }
        });

        private readonly string mockCouncilTaxDocument = JsonConvert.SerializeObject(new List<CouncilTaxDocument>
        {
            new CouncilTaxDocument
            {
                ClaimNumber = "123",
                DateCreated = "12-12-2018",
                Downloaded = "yes",
                DocumentId = "123",
                DocumentType = "Notif",
                DocumentName = "Name"
            }
        });

        private readonly string mockListPaymentDetail = JsonConvert.SerializeObject(new List<PaymentDetail>());

        private readonly string mockReceivedYearTotal = JsonConvert.SerializeObject(null);

        #endregion

        public BenefitsServiceTests()
        {
            _service = new BenefitsService(_mockGateway.Object, _cache.Object);
        }

        [Fact]
        public async void IsBenefitsClaimant_ShouldCallGateway()
        {
            // Arrange
            _mockGateway
                .Setup(_ => _.IsBenefitsClaimant(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("true")
                });

            // Act
            await _service.IsBenefitsClaimant(It.IsAny<string>());

            // Assert
            _mockGateway.Verify(_ => _.IsBenefitsClaimant(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async void IsBenefitsClaimant_ShouldThrowError()
        {
            // Arrange
            _mockGateway
                .Setup(_ => _.IsBenefitsClaimant(It.IsAny<string>()))
                .Throws<Exception>();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.IsBenefitsClaimant(It.IsAny<string>()));
        }

        [Fact]
        public async void GetBenefits_ShouldCallGateway()
        {
            // Arrange
            _mockGateway
                .Setup(_ => _.GetBenefits(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(mockListBenefitClaimSummary)
                });

            _mockGateway
                .Setup(_ => _.GetBenefitDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(mockBenefitsClaim)
                });

            _mockGateway
                .Setup(_ => _.GetDocuments(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(mockCouncilTaxDocument)
                });

            _mockGateway
                .Setup(_ => _.GetHousingBenefitPaymentHistory(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(mockListPaymentDetail)
                });

            _mockGateway
                .Setup(_ => _.GetCouncilTaxBenefitPaymentHistory(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(mockListPaymentDetail)
                });

            _mockGateway
                .Setup(_ => _.GetAccountDetailsForYear(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(mockReceivedYearTotal)
                });

            // Act
            await _service.GetBenefits(It.IsAny<string>());

            // Assert
            _mockGateway.Verify(_ => _.GetBenefits(It.IsAny<string>()), Times.Once);
            _mockGateway.Verify(_ => _.GetBenefitDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockGateway.Verify(_ => _.GetDocuments(It.IsAny<string>()), Times.Once);
            _mockGateway.Verify(_ => _.GetHousingBenefitPaymentHistory(It.IsAny<string>()), Times.Once);
            _mockGateway.Verify(_ => _.GetCouncilTaxBenefitPaymentHistory(It.IsAny<string>()), Times.AtLeastOnce);
            _mockGateway.Verify(_ => _.GetAccountDetailsForYear(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }
    }
}
