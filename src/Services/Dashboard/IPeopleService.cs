using System.Threading.Tasks;

namespace revs_bens_service.Services.Dashboard
{
    public interface IPeopleService
    {
        Task<bool> IsBenefitsClaimant(string personReference);
    }
}
