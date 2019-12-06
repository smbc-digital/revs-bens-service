using revs_bens_service.Services.CouncilTax.Mappers;
using StockportGovUK.NetStandard.Models.RevsAndBens;
using Xunit;

namespace revs_bens_service_tests.Service.Mapper
{
    public class PropertyMapperTests
    {
        private Place model = new Place
        {
            Address1 = "address1",
            Address2 = "address2",
            Band = new Band
            {
                Text = "test"
            }
        };

        [Fact]
        public void MapCurrentProperty_ShouldReturnModel()
        {
            // Arrange
            var result = new CouncilTaxDetailsModel();

            // Act
            result = model.MapCurrentProperty(result);

            // Assert
            Assert.Null(result.LiabilityPeriodStart);
            Assert.Null(result.LiabilityPeriodEnd);
            Assert.Equal("test", result.TaxBand);
            Assert.Equal("address1, address2", result.Property);
        }
    }
}
