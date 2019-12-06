using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using revs_bens_service.Services.CouncilTax.Mappers;
using revs_bens_service.Utils.Parsers;
using StockportGovUK.NetStandard.Models.Civica.CouncilTax;
using StockportGovUK.NetStandard.Models.RevsAndBens;
using Xunit;
using Instalment = StockportGovUK.NetStandard.Models.Civica.CouncilTax.Instalment;

namespace revs_bens_service_tests.Service.Mapper
{
    public class PaymentsMapperTests
    {
        private CouncilTaxPaymentScheduleResponse model = new CouncilTaxPaymentScheduleResponse
        {
            InstalmentList = new List<Instalment>
            {
                new Instalment
                {
                    DateDue = "12-01-2019",
                    AmountDue = 60.00M,
                    IsDirectDebit = "N"
                },
                new Instalment
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
            result = model.InstalmentList.MapPayments(result);

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
