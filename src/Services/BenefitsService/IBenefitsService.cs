using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Models.RevsAndBens;

namespace revs_bens_service.Services.BenefitsService
{
    public interface IBenefitsService
    {
        Task<Benefits> GetBenefitDetails(string personReference);
    }
}
