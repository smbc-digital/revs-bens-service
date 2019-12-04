using System;
using System.Collections.Generic;
using revs_bens_service.Services.CouncilTax.Mappers;
using revs_bens_service.Services.Models;
using StockportGovUK.NetStandard.Models.RevsAndBens;
using Xunit;
using Date = revs_bens_service.Services.Models.Date;
using PlaceDetail = revs_bens_service.Services.Models.PlaceDetail;
using Transaction = revs_bens_service.Services.Models.Transaction;

namespace revs_bens_service_tests.Service.Mapper
{
    public class TransactionsMapperTests
    {
        private List<Transaction> model = new List<Transaction>
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
        };

        [Theory]
        [InlineData("PAYMENTS")]
        [InlineData("Charge")]
        [InlineData("REFUNDS")]
        public void MapTransactions_ShouldReturnTransactionHistory_WithEmptyTransactionList(string type)
        {
            // Arrange
            var result = new CouncilTaxDetailsModel();
            model[0].TranType = type;

            // Act
            result = model.MapTransactions(result);

            // Assert
            Assert.Empty(result.TransactionHistory);
        }

        [Theory]
        [InlineData("100.00", "100.00")]
        [InlineData("-100.00", "100.00")]
        public void MapTransactions_ShouldReturnTransactionHistory_WithAmount(string amount, string expectedResult)
        {
            // Arrange
            var result = new CouncilTaxDetailsModel();
            model[0].Amount = amount;

            // Act
            result = model.MapTransactions(result);

            // Assert
            Assert.Equal(decimal.Parse(expectedResult), result.TransactionHistory[0].Amount);
        }

        [Fact]
        public void MapTransactions_ShouldReturnTransactionHistory_WithDate()
        {
            // Arrange
            var result = new CouncilTaxDetailsModel();

            // Act
            result = model.MapTransactions(result);

            // Assert
            Assert.Equal(DateTime.Parse(model[0].Date.Text), result.TransactionHistory[0].Date);
        }

        [Theory]
        [InlineData("CASH", "Cash")]
        [InlineData("PP", "Cash")]
        [InlineData("CCARD", "DebitCreditCard")]
        [InlineData("DCARD", "DebitCreditCard")]
        [InlineData("POCH", "Cheque")]
        [InlineData("D/D CASH", "DirectDebit")]
        [InlineData("DIRECTDEBIT", "DirectDebit")]
        [InlineData("S/O", "StandingOrder")]
        [InlineData("POTHER", "StandingOrder")]
        [InlineData("DISCOUNT", "Discount")]
        [InlineData("SUSPENSE", "Suspense")]
        [InlineData("TRANSFER", "Transfer")]
        [InlineData("OTHER", "Other")]
        [InlineData("BAILIF", "Bailif")]
        [InlineData("RANDOMTEXT", "Unknown")]
        public void MapTransactions_ShouldReturnTransactionHistory_WithMethod(string subCode, string expectedResult)
        {
            // Arrange
            var result = new CouncilTaxDetailsModel();
            model[0].SubCode = subCode;

            // Act
            result = model.MapTransactions(result);

            // Assert
            Assert.Equal(expectedResult, result.TransactionHistory[0].Method);
        }

        [Theory]
        [InlineData("BENEFITS", "Benefit")]
        [InlineData("COSTS", "Costs")]
        [InlineData("CTDRS", "Discretionary Relief")]
        [InlineData("INTEREST", "Interest")]
        [InlineData("LCTRS", "Council Tax Support")]
        [InlineData("PENALTY", "Penalty")]
        [InlineData("TRANSFER", "Transfer")]
        [InlineData("WRITEOFF", "Write Off")]
        [InlineData("DISABLED", "Disabled Relief - SK1 3XE")]
        [InlineData("EXEMPTION", "Exemption - SK1 3XE")]
        [InlineData("DISCOUNT", "Discount - SK1 3XE")]
        [InlineData("LEVY", "Premium Charge - SK1 3XE")]
        [InlineData("ANNEXE", "Annexe - SK1 3XE")]
        [InlineData("DISREGARD", "Disregard - SK1 3XE")]
        [InlineData("REDUCTION", "Reduction - SK1 3XE")]
        [InlineData("RANDOMTEXT", "Other")]
        public void MapTransactions_ShouldReturnTransactionHistory_WithDescription(string type, string expectedResult)
        {
            // Arrange
            var result = new CouncilTaxDetailsModel();
            model[0].TranType = type;

            // Act
            result = model.MapTransactions(result);

            // Assert
            Assert.Equal(expectedResult, result.TransactionHistory[0].Description);
        }
    }
}
