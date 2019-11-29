using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Moq;
using Newtonsoft.Json;
using revs_bens_service.Services.CouncilTax;
using revs_bens_service.Services.Models;
using revs_bens_service.Utils.StorageProvider;
using StockportGovUK.AspNetCore.Gateways.CivicaServiceGateway;
using StockportGovUK.NetStandard.Models.Civica.CouncilTax;
using StockportGovUK.NetStandard.Models.RevsAndBens;
using Xunit;
using Band = StockportGovUK.NetStandard.Models.Civica.CouncilTax.Band;
using Date = revs_bens_service.Services.Models.Date;
using Instalment = StockportGovUK.NetStandard.Models.Civica.CouncilTax.Instalment;
using PlaceDetail = revs_bens_service.Services.Models.PlaceDetail;
using Transaction = revs_bens_service.Services.Models.Transaction;

namespace revs_bens_service_tests.Service
{
    public class CouncilTaxServiceTests
    {
        private readonly CouncilTaxService _service;
        private readonly Mock<ICivicaServiceGateway> _mockGateway = new Mock<ICivicaServiceGateway>();
        private readonly Mock<ICacheProvider> _cache = new Mock<ICacheProvider>();

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

        private readonly string _mockTransactionsResponse = JsonConvert.SerializeObject(new TransactionResponse
        {
            Transaction = new List<Transaction>
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
            }
        });

        private readonly string _mockPaymentsScheduleResponse = JsonConvert.SerializeObject(new CouncilTaxPaymentScheduleResponse
        {
            InstalmentList = new List<Instalment>
            {
                new Instalment
                {
                    DateDue = "12-01-2019",
                    AmountDue = 60.00M,
                    IsDirectDebit = "false"
                },
                new Instalment
                {
                    DateDue = "12-12-2018",
                    AmountDue = 100.00M,
                    IsDirectDebit = "true"
                }
            }
        });

        private readonly string _mockPlacesResponse = JsonConvert.SerializeObject(new Places
        {
            ChargeDetails = null,
            Band = new Band
            {
                Text = "A"
            },
            Address1 = "address1",
            Address2 = "address2"
        });

        #endregion

        public CouncilTaxServiceTests()
        {
            _service = new CouncilTaxService(_mockGateway.Object, _cache.Object);

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
                .Setup(_ => _.GetCurrentProperty(It.IsAny<string>()))
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

            _mockGateway
                .Setup(_ => _.IsBenefitsClaimant(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("false")
                });
        }

        [Fact]
        public async void GetCouncilTaxDetails_ShouldCallGateway()
        {
            // Arrange

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
        public async void GetCouncilTaxDetails_ShouldReturnCouncilTaxDetailsModel()
        {
            // Arrange

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
