using System.Net;
using System.Net.Http;
using revs_bens_service.Services.CouncilTax.Mappers;
using revs_bens_service.Utils.Parsers;
using StockportGovUK.NetStandard.Models.Civica.CouncilTax;
using StockportGovUK.NetStandard.Models.RevsAndBens;
using Xunit;

namespace revs_bens_service_tests.Service.Mapper
{
    public class PropertyMapperTests
    {
        [Fact]
        public void MapCurrentProperty_ShouldReturnModel_WhenDatesNotNull()
        {
            // Arrange
            var result = new CouncilTaxDetailsModel();
            var gatewayResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{ChargeDetails:{Dates:{Start:'01-04-2017',End:'31-03-2018'}}}")
            };

            var parsedResponse = gatewayResponse.Parse<Places>();

            // Act
            result = parsedResponse.ResponseContent.MapCurrentProperty(result);

            // Assert
            Assert.Equal("01-04-2017", result.LiabilityPeriodStart);
            Assert.Equal("31-03-2018", result.LiabilityPeriodEnd);
        }

        [Fact]
        public void MapCurrentProperty_ShouldReturnModel_WhenDatesNull()
        {
            // Arrange
            var result = new CouncilTaxDetailsModel();
            var gatewayResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{ChargeDetails:{Dates:null}}")
            };

            var parsedResponse = gatewayResponse.Parse<Places>();

            // Act
            result = parsedResponse.ResponseContent.MapCurrentProperty(result);

            // Assert
            Assert.Null(result.LiabilityPeriodStart);
            Assert.Null(result.LiabilityPeriodEnd);
        }

        [Fact]
        public void MapCurrentProperty_ShouldReturnModel_WhenChargeDetailsNull()
        {
            // Arrange
            var result = new CouncilTaxDetailsModel();
            var gatewayResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{ChargeDetails:null}")
            };

            var parsedResponse = gatewayResponse.Parse<Places>();

            // Act
            result = parsedResponse.ResponseContent.MapCurrentProperty(result);

            // Assert
            Assert.Null(result.LiabilityPeriodStart);
            Assert.Null(result.LiabilityPeriodEnd);
        }
    }
}
