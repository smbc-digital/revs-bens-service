using System;
using System.Collections.Generic;
using revs_bens_service.Services.CouncilTax.Mappers;
using revs_bens_service.Services.Models;
using Xunit;

namespace revs_bens_service_tests.Service.Mapper
{
    public class TransactionsMapperTests
    {
        private TransactionResponse model = new TransactionResponse
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
        };

        [Theory]
        [InlineData("PAYMENTS")]
        [InlineData("CHARGE")]
        [InlineData("REFUNDS")]
        public void MapTransactions_ShouldReturnTransactionHistory_WithEmptyTransactionList(string type)
        {
            // Arrange
            var result = new CouncilTaxDetailsModel();
            model.Transaction[0].TranType = type;

            // Act
            result = model.MapTransactions(result);

            // Assert
            Assert.Empty(result.TransactionHistory);
        }

        [Theory]
        [InlineData("BENEFITS", "100.00", "Credit")]
        [InlineData("COSTS", "100.00", "Debit")]
        [InlineData("CTDRS", "100.00", "Credit")]
        [InlineData("INTEREST", "-100.00", "Debit")]
        [InlineData("LCTRS", "100.00", "Credit")]
        [InlineData("PENALTY", "-100.00", "Debit")]
        [InlineData("TRANSFER", "-100.00", "Debit")]
        [InlineData("WRITEOFF", "100.00", "Credit")]
        [InlineData("DISABLED", "-100.00", "Credit")]
        [InlineData("EXEMPTION", "-100.00", "Credit")]
        [InlineData("DISCOUNT", "-100.00", "Credit")]
        [InlineData("LEVY", "-100.00", "Debit")]
        [InlineData("ANNEXE", "100.00", "Credit")]
        [InlineData("DISREGARD", "-100.00", "Credit")]
        [InlineData("REDUCTION", "100.00", "Credit")]
        public void MapTransactions_ShouldReturnTransactionHistory_WithCorrectType(string type, string amount, string expectedResult)
        {
            // Arrange
            var result = new CouncilTaxDetailsModel();
            model.Transaction[0].TranType = type;
            model.Transaction[0].Amount = amount;

            // Act
            result = model.MapTransactions(result);

            // Assert
            Assert.Equal(expectedResult, result.TransactionHistory[0].Type);
            //Assert.Equal("Premium Charge - SK1 3XE", result.TransactionHistory[0].Description);
        }

        [Theory]
        [InlineData("100.00", "100.00")]
        [InlineData("-100.00", "100.00")]
        public void MapTransactions_ShouldReturnTransactionHistory_WithAmount(string amount, string expectedResult)
        {
            // Arrange
            var result = new CouncilTaxDetailsModel();
            model.Transaction[0].Amount = amount;

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
            Assert.Equal(DateTime.Parse(model.Transaction[0].Date.Text), result.TransactionHistory[0].Date);
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
            model.Transaction[0].SubCode = subCode;

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
            model.Transaction[0].TranType = type;

            // Act
            result = model.MapTransactions(result);

            // Assert
            Assert.Equal(expectedResult, result.TransactionHistory[0].Description);
        }
    }
}
