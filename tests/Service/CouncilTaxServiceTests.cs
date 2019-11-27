using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using Moq;
using revs_bens_service.Services.CouncilTax;
using revs_bens_service.Services.Models;
using revs_bens_service.Utils.StorageProvider;
using StockportGovUK.AspNetCore.Gateways.CivicaServiceGateway;
using StockportGovUK.NetStandard.Models.Models.Civica.CouncilTax;
using Xunit;

namespace revs_bens_service_tests.Service
{
    public class CouncilTaxServiceTests
    {
        private readonly CouncilTaxService _service;
        private readonly Mock<ICivicaServiceGateway> _mockGateway = new Mock<ICivicaServiceGateway>();
        private readonly Mock<ICacheProvider> _cache = new Mock<ICacheProvider>();

        public CouncilTaxServiceTests()
        {
            _service = new CouncilTaxService(_mockGateway.Object, _cache.Object);
        }

        [Fact]
        public async void GetCouncilTaxDetails_ShouldCallGateway()
        {
            // Arrange
            _mockGateway
                .Setup(_ => _.GetAccount(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{AccountDetails:{ActPayGrp:{PaymentMethod:'DD',DirectDebit:'yes'},BankDetails:null},CouncilTaxAccountBalance:0.00,FinancialDetails:{YearTotals: null},CouncilTaxAccountReference:'123',CtaxActClosed:'FALSE'}")
                });

            _mockGateway
                .Setup(_ => _.GetAllTransactionsForYear(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{Transaction:[{Date:{Text:'12-12-2018'},Amount:'100.00',DAmount:100.00,PlaceDetail:null,TranType:'test',SubCode:'test'}]}")
                });

            _mockGateway
                .Setup(_ => _.GetPaymentSchedule(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{InstalmentList:[{DateDue:'12-12-2018',AmountDue:100.00,IsDirectDebit:'true'}],PaymentMethod:'test'}")
                });

            _mockGateway
                .Setup(_ => _.GetCurrentProperty(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{ChargeDetails:null}")
                });

            _mockGateway
                .Setup(_ => _.IsBenefitsClaimant(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("false")
                });

            // Act
            await _service.GetCouncilTaxDetails("123", "5001111234", 2018);

            // Assert
            _mockGateway.Verify(_ => _.GetAccount(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockGateway.Verify(_ => _.GetAllTransactionsForYear(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            _mockGateway.Verify(_ => _.GetPaymentSchedule(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            _mockGateway.Verify(_ => _.GetCurrentProperty(It.IsAny<string>()), Times.Once);
            _mockGateway.Verify(_ => _.IsBenefitsClaimant(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async void GetCouncilTaxDetails_ShouldReturnModel()
        {
            // Arrange
            var model = new CouncilTaxDetailsModel
            {
                PaymentMethod = "Direct Debit",
                IsDirectDebitCustomer = true,
                AmountOwing = 0.00M,
                YearTotals = null,
                Reference = "123",
                PaymentSummary = new PaymentSummaryResponse
                {
                    NextPaymentDate = null,
                    NextPaymentAmount = 0.0M
                },
                IsFinalNotice = null,
                IsClosed = false,
                AccountName = null,
                AccountNumber = null,
                LiabilityPeriodStart = null,
                LiabilityPeriodEnd = null,
                UpcomingPayments = new List<InstallmentModel>
                {
                    new InstallmentModel
                    {
                        Date = new DateTime(2018,12,12),
                        Amount = -100M,
                        IsDirectDebit = true
                    }
                },
                PreviousPayments = new List<TransactionModelExtension>(),
                TransactionHistory = new List<TransactionModelExtension>
                {
                    new TransactionModelExtension
                    {
                        Date = new DateTime(2018, 12, 12),
                        Amount = 100M,
                        Method = "Unknown",
                        Description = "Other",
                        Type = "Credit"
                    }
                },
                HasBenefits = false
            };

            _mockGateway
                .Setup(_ => _.GetAccount(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{AccountDetails:{ActPayGrp:{PaymentMethod:'DD',DirectDebit:'yes'},BankDetails:null},CouncilTaxAccountBalance:0.00,FinancialDetails:{YearTotals: null},CouncilTaxAccountReference:'123',CtaxActClosed:'FALSE'}")
                });

            _mockGateway
                .Setup(_ => _.GetAllTransactionsForYear(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{Transaction:[{Date:{Text:'12-12-2018'},Amount:'100.00',DAmount:100.00,PlaceDetail:null,TranType:'test',SubCode:'test'}]}")
                });

            _mockGateway
                .Setup(_ => _.GetPaymentSchedule(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{InstalmentList:[{DateDue:'12-12-2018',AmountDue:100.00,IsDirectDebit:'true'}],PaymentMethod:'test'}")
                });

            _mockGateway
                .Setup(_ => _.GetCurrentProperty(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{ChargeDetails:null}")
                });

            _mockGateway
                .Setup(_ => _.IsBenefitsClaimant(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("false")
                });

            // Act
            var result = await _service.GetCouncilTaxDetails("123", "5001111234", 2018);

            // Assert
            Assert.IsType<CouncilTaxDetailsModel>(result);
        }

        [Fact]
        public async void GetCouncilTaxDetails_ShouldCallCacheProvider()
        {
            // Arrange
            _mockGateway
                .Setup(_ => _.GetAccount(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{AccountDetails:{ActPayGrp:{PaymentMethod:'DD',DirectDebit:'yes'},BankDetails:null},CouncilTaxAccountBalance:0.00,FinancialDetails:{YearTotals: null},CouncilTaxAccountReference:'123',CtaxActClosed:'FALSE'}")
                });

            _mockGateway
                .Setup(_ => _.GetAllTransactionsForYear(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{Transaction:[{Date:{Text:'12-12-2018'},Amount:'100.00',DAmount:100.00,PlaceDetail:null,TranType:'test',SubCode:'test'}]}")
                });

            _mockGateway
                .Setup(_ => _.GetPaymentSchedule(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{InstalmentList:[{DateDue:'12-12-2018',AmountDue:100.00,IsDirectDebit:'true'}],PaymentMethod:'test'}")
                });

            _mockGateway
                .Setup(_ => _.GetCurrentProperty(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{ChargeDetails:null}")
                });

            _mockGateway
                .Setup(_ => _.IsBenefitsClaimant(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("false")
                });

            _cache
                .Setup(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>()));

            // Act
            await _service.GetCouncilTaxDetails("123", "5001111234", 2018);

            // Assert
            _cache.Verify(_ => _.GetStringAsync(It.IsAny<string>()), Times.Once);
            _cache.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async void GetCouncilTaxDetails_ShouldNotCallGatewayWhenCacheAvailable()
        {
            // Arrange
            _cache
                .Setup(_ => _.GetStringAsync(It.IsAny<string>()))
                .ReturnsAsync("{IsDirectDebitCustomer:true}");

            // Act
            await _service.GetCouncilTaxDetails("123", "5001111234", 2018);

            // Assert
            _mockGateway.Verify(_ => _.GetAccount(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockGateway.Verify(_ => _.GetAllTransactionsForYear(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            _mockGateway.Verify(_ => _.GetPaymentSchedule(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            _mockGateway.Verify(_ => _.GetCurrentProperty(It.IsAny<string>()), Times.Never);
            _mockGateway.Verify(_ => _.IsBenefitsClaimant(It.IsAny<string>()), Times.Never);
        }
    }
}
