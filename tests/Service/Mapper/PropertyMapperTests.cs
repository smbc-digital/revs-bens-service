using revs_bens_service.Services.CouncilTax.Mappers;
using StockportGovUK.NetStandard.Models.Civica.CouncilTax;
using StockportGovUK.NetStandard.Models.RevsAndBens;
using Xunit;

namespace revs_bens_service_tests.Service.Mapper
{
    public class PropertyMapperTests
    {
        private Places model = new Places
        {
            ChargeDetails = new ChargeDetailsResponse
            {
                Dates = new ChargeDetailsDatesResponse
                {
                    Start = "01-04-2017",
                    End = "31-03-2018"
                }
            }
        };

        [Fact]
        public void MapCurrentProperty_ShouldReturnModel_WhenDatesNotNull()
        {
            // Arrange
            var result = new CouncilTaxDetailsModel();

            // Act
            result = model.MapCurrentProperty(result);

            // Assert
            Assert.Equal("01-04-2017", result.LiabilityPeriodStart);
            Assert.Equal("31-03-2018", result.LiabilityPeriodEnd);
        }

        [Fact]
        public void MapCurrentProperty_ShouldReturnModel_WhenDatesNull()
        {
            // Arrange
            var result = new CouncilTaxDetailsModel();
            model.ChargeDetails.Dates = null;

            // Act
            result = model.MapCurrentProperty(result);

            // Assert
            Assert.Null(result.LiabilityPeriodStart);
            Assert.Null(result.LiabilityPeriodEnd);
        }

        [Fact]
        public void MapCurrentProperty_ShouldReturnModel_WhenChargeDetailsNull()
        {
            // Arrange
            var result = new CouncilTaxDetailsModel();
            model.ChargeDetails = null;

            // Act
            result = model.MapCurrentProperty(result);

            // Assert
            Assert.Null(result.LiabilityPeriodStart);
            Assert.Null(result.LiabilityPeriodEnd);
        }
    }
}
