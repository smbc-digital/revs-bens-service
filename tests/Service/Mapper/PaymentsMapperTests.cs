using System;
using System.Net;
using System.Net.Http;
using revs_bens_service.Services.CouncilTax.Mappers;
using revs_bens_service.Utils.Parsers;
using StockportGovUK.NetStandard.Models.Civica.CouncilTax;
using StockportGovUK.NetStandard.Models.RevsAndBens;
using Xunit;

namespace revs_bens_service_tests.Service.Mapper
{
    public class PaymentsMapperTests
    {
        [Fact]
        public void MapPayments_ShouldReturnModel()
        {
            // Arrange
            var result = new CouncilTaxDetailsModel();
            var gatewayResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{InstalmentList:[{DateDue:'12-01-2019',AmountDue:60.00,IsDirectDebit:'false'}, {DateDue:'12-12-2018',AmountDue:100.00,IsDirectDebit:'true'}],PaymentMethod:'test'}")
            };

            var parsedResponse = gatewayResponse.Parse<CouncilTaxPaymentScheduleResponse>();

            // Act
            result = parsedResponse.ResponseContent.MapPayments(result);

            // Assert
            Assert.Equal(DateTime.Parse("12-01-2019"), result.UpcomingPayments[0].Date);
            Assert.Equal(-60.00M, result.UpcomingPayments[0].Amount);
            Assert.False(result.UpcomingPayments[0].IsDirectDebit);
            Assert.Equal(DateTime.Parse("12-12-2018"), result.UpcomingPayments[1].Date);
            Assert.Equal(-100.00M, result.UpcomingPayments[1].Amount);
            Assert.True(result.UpcomingPayments[1].IsDirectDebit);
        }
    }
}
