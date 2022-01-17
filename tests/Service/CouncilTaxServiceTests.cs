using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using revs_bens_service.Services.Benefits;
using revs_bens_service.Services.CouncilTax;
using revs_bens_service.Utils.StorageProvider;
using StockportGovUK.NetStandard.Gateways.CivicaService;
using StockportGovUK.NetStandard.Models.Civica.CouncilTax;
using StockportGovUK.NetStandard.Models.RevsAndBens;
using Xunit;

namespace revs_bens_service_tests.Service
{
    public class CouncilTaxServiceTests
    {
        private readonly CouncilTaxService _service;
        private readonly Mock<ICivicaServiceGateway> _mockGateway = new Mock<ICivicaServiceGateway>();
        private readonly Mock<ICacheProvider> _cache = new Mock<ICacheProvider>();
        private readonly Mock<IBenefitsService> _mockBenefitsService = new Mock<IBenefitsService>();

        #region Test models

        private readonly string _mockCouncilTaxAccountsResponse = JsonConvert.SerializeObject(new List<CtaxActDetails>
        {
            new CtaxActDetails
            {
                AccountStatus = "status",
                CtaxActAddress = "address",
                CtaxActRef = "123",
                CtaxBalance = "100.00"
            }
        });

        private readonly string _mockCouncilTaxAccountResponse = JsonConvert.SerializeObject(new CouncilTaxAccountResponse
        {
            PersonName = new StockportGovUK.NetStandard.Models.Civica.CouncilTax.PersonName
            {
                Forenames = "Test",
                Surname = "Test"
            },
            AccountDetails = new AccountDetail
            {
                ActPayGrp = new ActPayGrp
                {
                    PaymentMethod = "DD",
                    DirectDebit = "yes"
                },
                BankDetails = new BankAccountDetailsResponse
                {
                    AccountNumber = "12345678",
                    AccountName = "testName"
                }
            },
            CouncilTaxAccountBalance = 120.00M,
            FinancialDetails = new FinancialDetailsResponse
            {
                YearTotals = new List<YearTotalResponse>
                {
                    new YearTotalResponse
                    {
                        TaxYear = 2018,
                        TotalWriteoffs = 0.00M,
                        TotalRefunds = 0.00M,
                        BalanceOutstanding = 0.00M,
                        TotalBenefits = 0.00M,
                        TotalCharge = 120.00M,
                        TotalCosts = 0.00M,
                        TotalPayments = 120.00M,
                        TotalPenalties = 0.00M,
                        TotalTransfers = 0.00M,
                        YearSummaries = new List<YearSummaryResponse>()
                    }
                }
            },
            CouncilTaxAccountReference = "123",
            CtxActClosed = "FALSE"
        });

        private readonly string _mockTransactionsResponse = JsonConvert.SerializeObject(new List<Transaction>
        {
            new Transaction
            {
                Date = new Date
                {
                    Text = "12-12-2018"
                },
                Amount = "100.00",
                PlaceDetail = new PlaceDetail
                {
                    PostCode = "SK1 3XE"
                },
                TranType = "LEVY",
                SubCode = "CASH"
            }
        });

        private readonly string _mockPaymentsScheduleResponse = JsonConvert.SerializeObject(new List<Installment>
        {
                new Installment
                {
                    DateDue = "12-01-2019",
                    AmountDue = 60.00M,
                    IsDirectDebit = "false"
                },
                new Installment
                {
                    DateDue = "12-12-2018",
                    AmountDue = 100.00M,
                    IsDirectDebit = "true"
                }
        });

        private readonly string _mockPlacesResponse = JsonConvert.SerializeObject(new Place
        {
            Band = new StockportGovUK.NetStandard.Models.RevsAndBens.Band
            {
                Text = "A"
            },
            Address1 = "address1",
            Address2 = "address2"
        });

        #endregion

        public CouncilTaxServiceTests()
        {
            _service = new CouncilTaxService(_mockGateway.Object, _cache.Object, _mockBenefitsService.Object);

            _mockGateway
                .Setup(_ => _.GetAccounts(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(_mockCouncilTaxAccountsResponse)
                });

            _mockGateway
                .Setup(_ => _.GetAccount(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(_mockCouncilTaxAccountResponse)
                });

            _mockGateway
                .Setup(_ => _.GetAllTransactionsForYear(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(_mockTransactionsResponse)
                });

            _mockGateway
                .Setup(_ => _.GetPaymentSchedule(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(_mockPaymentsScheduleResponse)
                });

            _mockGateway
                .Setup(_ => _.GetCurrentProperty(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(_mockPlacesResponse)
                });

            _mockGateway
                .Setup(_ => _.GetDocuments(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[]")
                });

            _mockBenefitsService
                .Setup(_ => _.IsBenefitsClaimant(It.IsAny<string>()))
                .ReturnsAsync(false);

            _mockGateway
                .Setup(_ => _.GetDocumentForAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[]")
                });
        }

        [Fact]
        public async void GetCouncilTaxDetails_ShouldCallGateway()
        {
            // Act
            await _service.GetCouncilTaxDetails("123", "5001111234", 2018);

            // Assert
            _mockGateway.Verify(_ => _.GetAccount(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockGateway.Verify(_ => _.GetAllTransactionsForYear(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            _mockGateway.Verify(_ => _.GetPaymentSchedule(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            _mockGateway.Verify(_ => _.GetCurrentProperty(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockBenefitsService.Verify(_ => _.IsBenefitsClaimant(It.IsAny<string>()), Times.Once);
        }

        [Theory]
        [InlineData(" 123456 ", "123456")]
        [InlineData("     123456", "123456")]
        [InlineData("123456    ", "123456")]
        public async void GetCouncilTaxDetails_ShouldUseTrimmedAccountReference(string accountReference, string expectedAccountReference)
        {
            // Act
            await _service.GetCouncilTaxDetails("123", accountReference, 2018);

            // Assert
            _mockGateway.Verify(_ => _.GetAccount(It.IsAny<string>(), expectedAccountReference), Times.Once);
            _mockGateway.Verify(_ => _.GetAllTransactionsForYear(It.IsAny<string>(), expectedAccountReference, It.IsAny<int>()), Times.Once);
            _mockGateway.Verify(_ => _.GetCurrentProperty(It.IsAny<string>(), expectedAccountReference), Times.Once);
        }

        [Fact]
        public async void GetCouncilTaxDetails_ShouldReturnCouncilTaxDetailsModel()
        {
            // Act
            var result = await _service.GetCouncilTaxDetails("123", "5001111234", 2018);

            // Assert
            Assert.IsType<CouncilTaxDetailsModel>(result);
        }

        [Fact]
        public async void GetCouncilTaxDetails_ShouldCallCacheProvider()
        {
            // Arrange
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
                .ReturnsAsync("{\"IsDirectDebitCustomer\":true}");

            // Act
            await _service.GetCouncilTaxDetails("123", "5001111234", 2018);

            // Assert
            _mockGateway.Verify(_ => _.GetAccount(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockGateway.Verify(_ => _.GetAllTransactionsForYear(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            _mockGateway.Verify(_ => _.GetPaymentSchedule(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            _mockGateway.Verify(_ => _.GetCurrentProperty(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockBenefitsService.Verify(_ => _.IsBenefitsClaimant(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetDocumentForAccount_ShouldCallCacheProvider()
        {
            // Act
            await _service.GetDocumentForAccount("personReference", "accountReference", "documentId");

            // Assert
            _cache.Verify(_ => _.GetStringAsync(It.IsAny<string>()), Times.Once);
            _cache.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetDocumentForAccount_ShouldNotCallGatewayOrCacheProvider_IfCacheAvailable()
        {
            // Arrange
            var cacheResponse = JsonConvert.SerializeObject(new byte[1]);
            _cache
                .Setup(_ => _.GetStringAsync(It.IsAny<string>()))
                .ReturnsAsync(cacheResponse);

            // Act
            await _service.GetDocumentForAccount("personReference", "accountReference", "documentId");

            // Assert
            _mockGateway.Verify(_ => _.GetDocumentForAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _cache.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData(" 123456 ", "123456")]
        [InlineData("    123456", "123456")]
        [InlineData("123456    ", "123456")]
        public async Task GetDocumentForAccount_ShouldCallGateway_WithTrimmedAccountReference(string accountReference, string expectedAccountReference)
        {
            // Act
            await _service.GetDocumentForAccount("personReference", accountReference, "documentId");

            // Assert
            _mockGateway.Verify(_ => _.GetDocumentForAccount(It.IsAny<string>(), expectedAccountReference, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetDocumentForAccount_ShouldReturnNull_IfDocumentNotFound()
        {
            // Arrange
            _mockGateway
                .Setup(_ => _.GetDocumentForAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            // Act
            var result = await _service.GetDocumentForAccount("personReference", "accountReference", "documentId");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDocumentForAccount_ShouldReturnByteArray()
        {
            // Act
            var result = await _service.GetDocumentForAccount("personReference", "accountReference", "documentId");

            // Assert
            Assert.IsType<byte[]>(result);
        }
    }
}
