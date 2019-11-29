using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using revs_bens_service.Services.CouncilTax.Mappers;
using revs_bens_service.Services.Models;
using revs_bens_service.Utils.Parsers;
using StockportGovUK.NetStandard.Models.Models.Civica.CouncilTax;
using Xunit;

namespace revs_bens_service_tests.Service.Mapper
{
    public class AccountMapperTests
    {
        private CouncilTaxAccountResponse model = new CouncilTaxAccountResponse
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
        };

        [Fact]
        public void MapAccount_ShouldReturnMappedAccount()
        {
            // Arrange
            var result = new CouncilTaxDetailsModel();

            // Act
            result = model.MapAccount(result);
            var actualYearTotals = result.YearTotals.ToList();

            // Assert
            Assert.Equal("Direct Debit", result.PaymentMethod);
            Assert.True(result.IsDirectDebitCustomer);
            Assert.Equal(120M, result.AmountOwing);
            Assert.Equal(2018, actualYearTotals[0].TaxYear);
            Assert.Equal("123", result.Reference);
            Assert.Equal(0.00M, result.PaymentSummary.NextPaymentAmount);
            Assert.Null(result.PaymentSummary.NextPaymentDate);
            Assert.Equal("5678", result.AccountNumber);
            Assert.Equal("testName", result.AccountName);
            Assert.False(result.IsFinalNotice);
            Assert.False(result.IsClosed);
        }

        [Fact]
        public void MapAccount_ShouldReturnMappedAccountFinalNoticeAndClosedWithPaymentSummary()
        {
            // Arrange
            var result = new CouncilTaxDetailsModel();
            model.FinancialDetails.YearTotals[0].YearSummaries = new List<YearSummaryResponse>
            {
                new YearSummaryResponse
                {
                    Stage = new StageResponse
                    {
                        StageCode = "test",
                        StageDate = "12-12-2018"
                    },
                    NextPayment = new PaymentSummaryResponse
                    {
                        NextPaymentDate = "12-12-2018",
                        NextPaymentAmount = 80.00M
                    }
                }
            };
            model.CtxActClosed = "TRUE";

            // Act
            result = model.MapAccount(result);
            var actualYearTotals = result.YearTotals.ToList();

            // Assert
            Assert.Equal("Direct Debit", result.PaymentMethod);
            Assert.True(result.IsDirectDebitCustomer);
            Assert.Equal(120M, result.AmountOwing);
            Assert.Equal(2018, actualYearTotals[0].TaxYear);
            Assert.Equal("123", result.Reference);
            Assert.Equal(80.00M, result.PaymentSummary.NextPaymentAmount);
            Assert.Equal("12-12-2018",result.PaymentSummary.NextPaymentDate);
            Assert.Equal("5678", result.AccountNumber);
            Assert.Equal("testName", result.AccountName);
            Assert.True(result.IsFinalNotice);
            Assert.True(result.IsClosed);
        }
    }
}
