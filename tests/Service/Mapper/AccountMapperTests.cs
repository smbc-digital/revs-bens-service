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
        [Fact]
        public void MapAccount_ShouldReturnMappedAccount()
        {
            // Arrange
            var result = new CouncilTaxDetailsModel();
            var gatewayResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{AccountDetails:{ActPayGrp:{PaymentMethod:'DD',DirectDebit:'yes'},BankDetails:{AccountNumber:'12345678', AccountName:'testName'}},CouncilTaxAccountBalance:120.00,FinancialDetails:{YearTotals: [{TaxYear:2018,BalanceOutstanding:0.00,TotalCharge:1200.00,TotalPayments:1200.00,TotalBenefits:0.00,TotalCosts:0.00,TotalRefunds:0.00,TotalWriteoffs:0.00,TotalTransfers:0.00,TotalPenalties:0.00,YearSummaries:[]}]},CouncilTaxAccountReference:'123',CtaxActClosed:'FALSE'}")
            };

            var parsedResponse = gatewayResponse.Parse<CouncilTaxAccountResponse>();

            // Act
            result = parsedResponse.ResponseContent.MapAccount(result);
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
            var gatewayResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{AccountDetails:{ActPayGrp:{PaymentMethod:'DD',DirectDebit:'yes'},BankDetails:{AccountNumber:'12345678',AccountName:'testName'}},CouncilTaxAccountBalance:120.00,FinancialDetails:{YearTotals:[{TaxYear:2018,BalanceOutstanding:0.00,TotalCharge:1200.00,TotalPayments:1200.00,TotalBenefits:0.00,TotalCosts:0.00,TotalRefunds:0.00,TotalWriteoffs:0.00,TotalTransfers:0.00,TotalPenalties:0.00,YearSummaries:[{Stage:{StageCode:'test',StageDate:'12-12-2018'},NextPayment:{NextPaymentDate:'12-12-2018',NextPaymentAmount:'80.00'}}]}]},CouncilTaxAccountReference:'123',CtxActClosed:'TRUE'}")
            };

            var parsedResponse = gatewayResponse.Parse<CouncilTaxAccountResponse>();

            // Act
            result = parsedResponse.ResponseContent.MapAccount(result);
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
