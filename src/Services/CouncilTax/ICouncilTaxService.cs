using System.Collections.Generic;
using StockportGovUK.NetStandard.Models.RevsAndBens;
using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Civica.CouncilTax;

namespace revs_bens_service.Services.CouncilTax
{
    public interface ICouncilTaxService
    {
        Task<CouncilTaxDetailsModel> GetBaseCouncilTaxAccount(string personReference);

        Task<List<CouncilTaxAccountDetails>> GetCouncilTaxAccounts(string personReference);

        Task<string> GetCurrentCouncilTaxAccountNumber(string personReference);

        Task<CouncilTaxDetailsModel> GetReducedCouncilTaxDetails(string personReference, string accountReference, int year);

        Task<CouncilTaxDetailsModel> GetCouncilTaxDetails(string personReference, string accountReference, int year);

        Task<byte[]> GetDocumentForAccount(string personReference, string accountReference, string documentId);

        Task<List<CouncilTaxDocument>> GetDocumentsForPerson(string personReference);
    }
}
