using StockportGovUK.NetStandard.Models.RevsAndBens;
using System.Threading.Tasks;

namespace revs_bens_service.Services.CouncilTax
{
    public interface ICouncilTaxService
    {
        Task<CouncilTaxDetailsModel> GetCouncilTaxDetails(string personReference, string accountReference, int year);

        Task<byte[]> GetDocumentForAccount(string personReference, string accountReference, string documentId);
    }
}
