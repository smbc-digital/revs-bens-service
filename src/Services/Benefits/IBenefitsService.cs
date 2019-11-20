using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.RevsAndBens;

namespace revs_bens_service.Services.Benefits
{
    public interface IBenefitsService
    {
        Task<bool> IsBenefitsClaimant(string personReference);
        
        Task<Claim> GetBenefits(string personReference);
    }
}
