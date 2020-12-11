using StockportGovUK.NetStandard.Models.RevsAndBens;

namespace revs_bens_service.Services.CouncilTax.Mappers
{
    public static class PropertyMapper
    {
        //TODO: LiabilityPeriodStart && LiabilityPeriodEnd never set from civica service -- needs looking into
        public static CouncilTaxDetailsModel MapCurrentProperty(
            this Place propertyResponse,
            CouncilTaxDetailsModel model)
        {
            //model.LiabilityPeriodStart = propertyResponse.ChargeDetails?.Dates?.Start;
            //model.LiabilityPeriodEnd = propertyResponse.ChargeDetails?.Dates?.End;
            model.TaxBand = propertyResponse.Band.Text;
            model.Property = $"{propertyResponse.Address1}, {propertyResponse.Address2}";

            return model;
        }
    }
}
