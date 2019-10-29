using System.Threading.Tasks;

namespace revs_bens_service.Services.Dashboard
{
    public interface IPersonService
    {
        Task<bool> IsBenefitsClaimant(string personReference);
    }
}
