using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Moq;
using Newtonsoft.Json;
using revs_bens_service.Services.Benefits;
using revs_bens_service.Utils.StorageProvider;
using StockportGovUK.NetStandard.Gateways.CivicaServiceGateway;
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
                PersonName = "Test Test",
                Status = "Current",
                PlaceReference = "123",
                Address = "address",
                Number = "123",
                PersonType = "type"
            }
        });

        private readonly string mockBenefitsClaim = JsonConvert.SerializeObject(new BenefitsClaim
        {
            PersonName = new PersonName{
                Forenames = "Test",
                Surname = "Test"
            },
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
                AccountReference = "123",
                DateCreated = "12-12-2018",
                Downloaded = "yes",
                DocumentId = "123",
                DocumentType = "Notif",
                DocumentName = "Name"
            }
        });

        private readonly string mockListPaymentDetail = JsonConvert.SerializeObject(new List<PaymentDetail>());

        private readonly string mockReceivedYearTotal = JsonConvert.SerializeObject(new RecievedYearTotal
        {
            BalanceOutstanding = "10.2",
            TotalBenefits = "323.25",
            TotalCharge = "10.25",
            TotalPayments = "400.25"
        });

        private readonly string mockListCouncilTaxPayments = JsonConvert.SerializeObject(new List<PaymentDetail>
        {
            new PaymentDetail
            {
                CouncilTaxReference = "500000000",
                OnAct = "test",
                DatePaid = "02-12-2019",
                PayAmount = "20.00",
                Payee = "payee",
                PayType = "test-type",
                PeriodStart = "02-12-2019",
                PeriodEnd = "02-12-2020",
            }
        });
        #endregion

        public BenefitsServiceTests()
        {
            _mockGateway
             .Setup(_ => _.GetBenefits("test"))
             .ReturnsAsync(new HttpResponseMessage
             {
                 StatusCode = HttpStatusCode.OK,
                 Content = new StringContent("test")
             });

            _mockGateway
              .Setup(_ => _.GetBenefits("test-ref"))
              .ReturnsAsync(new HttpResponseMessage
              {
                  StatusCode = HttpStatusCode.OK,
                  Content = new StringContent(mockListBenefitClaimSummary)
              });

            _mockGateway
              .Setup(_ => _.GetBenefitDetails("test-ref", "123", "123"))
              .ReturnsAsync(new HttpResponseMessage
              {
                  StatusCode = HttpStatusCode.OK,
                  Content = new StringContent(mockBenefitsClaim)
              });

            _mockGateway
                .Setup(_ => _.GetDocuments("test-ref"))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(mockCouncilTaxDocument)
                });

            _mockGateway
                .Setup(_ => _.GetHousingBenefitPaymentHistory("test-ref"))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(mockListCouncilTaxPayments)
                });

            _mockGateway
               .Setup(_ => _.GetCouncilTaxBenefitPaymentHistory("test-ref"))
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(mockListCouncilTaxPayments)
               });

            _mockGateway
              .Setup(_ => _.GetAccountDetailsForYear("test-ref", "500000000", 2019))
              .ReturnsAsync(new HttpResponseMessage
              {
                  StatusCode = HttpStatusCode.OK,
                  Content = new StringContent(mockReceivedYearTotal)
              });

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
            // Act
            await _service.GetBenefits("test-ref");

            // Assert
            _mockGateway.Verify(_ => _.GetBenefits("test-ref"), Times.Once);
            _mockGateway.Verify(_ => _.GetBenefitDetails("test-ref", "123", "123"), Times.Once);
            _mockGateway.Verify(_ => _.GetDocuments("test-ref"), Times.Once);
            _mockGateway.Verify(_ => _.GetHousingBenefitPaymentHistory("test-ref"), Times.Once);
            _mockGateway.Verify(_ => _.GetCouncilTaxBenefitPaymentHistory("test-ref"), Times.AtLeastOnce);
            _mockGateway.Verify(_ => _.GetAccountDetailsForYear("test-ref", "500000000", 2019), Times.Once);
        }

        [Fact]
        public async void GetBenefits_ShouldCallCacheProvider_WithGetStringAsync()
        {
            // Arrange
            var model = JsonConvert.SerializeObject(new Claim());
            _cache
                .Setup(_ => _.GetStringAsync(It.IsAny<string>()))
                .ReturnsAsync(model);

            // Act
            await _service.GetBenefits(It.IsAny<string>());

            // Assert
            _cache.Verify(_ => _.GetStringAsync(It.IsAny<string>()), Times.Once);
        }


        [Fact]
        public async void GetBenefits_ShouldReturnNull_IfClaimsAreNull()
        {
            //Act
            var result = await _service.GetBenefits("test");

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async void GetBenefits_GetAccount_WithStatusSetToCurrent()
        {
            // Act
            var result = await _service.GetBenefits("test-ref");

            // Assert
            Assert.Equal("Current", result.Details.Status);
        }

        [Fact]
        public async void GetBenefits_ShoulCallCacheProvider_WithSetStringAsync()
        {
            // Act
            await _service.GetBenefits("test-ref");

            // Assert
            _cache.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>()));
        }
    }
}
