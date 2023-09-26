using StockportGovUK.NetStandard.Gateways.Models.RevsAndBens;

namespace revs_bens_service.Services.Benefits
{
    public interface IBenefitsService
    {
        Task<bool> IsBenefitsClaimant(string personReference);

        Task<Claim> GetBenefits(string personReference);
    }
}
