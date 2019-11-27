using revs_bens_service.Services.Models;
using StockportGovUK.NetStandard.Models.Models.Civica.CouncilTax;

namespace revs_bens_service.Services.CouncilTax.Mappers
{
    public static class PropertyMapper
    {
        public static CouncilTaxDetailsModel MapCurrentProperty(this Places propertyResponse, CouncilTaxDetailsModel model)
        {
            model.LiabilityPeriodStart = propertyResponse.ChargeDetails?.Dates?.Start;
            model.LiabilityPeriodEnd = propertyResponse.ChargeDetails?.Dates?.End;
            model.TaxBand = propertyResponse.Band.Text;
            model.Property = $"{propertyResponse.Address1}, {propertyResponse.Address2}";

            return model;
        }
    }
}
