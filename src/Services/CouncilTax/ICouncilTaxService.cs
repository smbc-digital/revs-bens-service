using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Models.Civica.CouncilTax;

namespace revs_bens_service.Services.CouncilTax
{
    public interface ICouncilTaxService
    {
        Task<CouncilTaxDetailsModel> GetCouncilTaxDetails(string personReference, string accountReference, int year);
    }
}
