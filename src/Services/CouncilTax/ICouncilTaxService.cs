using System.Threading.Tasks;
using revs_bens_service.Services.Models;

namespace revs_bens_service.Services.CouncilTax
{
    public interface ICouncilTaxService
    {
        Task<CouncilTaxDetailsModel> GetCouncilTaxDetails(string personReference, string accountReference, int year);
    }
}
