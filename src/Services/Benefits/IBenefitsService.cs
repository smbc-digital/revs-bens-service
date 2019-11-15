using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Models.RevsAndBens;

namespace revs_bens_service.Services.HousingBenefits
{
    public interface IBenefitsService
    {
        Task<bool> IsBenefitsClaimant(string personReference);
        
        Task<dynamic> GetBenefitsDetails(string personReference);
    }
}
