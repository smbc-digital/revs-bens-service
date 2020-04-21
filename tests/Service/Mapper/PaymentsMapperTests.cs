using System;
using System.Collections.Generic;
using revs_bens_service.Services.CouncilTax.Mappers;
using StockportGovUK.NetStandard.Models.Civica.CouncilTax;
using StockportGovUK.NetStandard.Models.RevsAndBens;
using Xunit;

namespace revs_bens_service_tests.Service.Mapper
{
    public class PaymentsMapperTests
    {
        private readonly CouncilTaxPaymentScheduleResponse _model = new CouncilTaxPaymentScheduleResponse
        {
            InstallmentList = new List<Installment>
            {
                new Installment
                {
                    DateDue = "12-01-2019",
                    AmountDue = 60.00M,
                    IsDirectDebit = "N"
                },
                new Installment
                {
                    DateDue = "12-12-2018",
                    AmountDue = 100.00M,
                    IsDirectDebit = "Y"
                }
            }
        };

        [Fact]
        public void MapPayments_ShouldReturnModel()
        {
            // Arrange
            var result = new CouncilTaxDetailsModel();

            // Act
            result = _model.InstallmentList.MapPayments(result);

            // Assert
            Assert.Equal(DateTime.Parse("12-01-2019"), result.UpcomingPayments[0].Date);
            Assert.Equal(60.00M, result.UpcomingPayments[0].Amount);
            Assert.False(result.UpcomingPayments[0].IsDirectDebit);
            Assert.Equal(DateTime.Parse("12-12-2018"), result.UpcomingPayments[1].Date);
            Assert.Equal(100.00M, result.UpcomingPayments[1].Amount);
            Assert.True(result.UpcomingPayments[1].IsDirectDebit);
        }
    }
}
