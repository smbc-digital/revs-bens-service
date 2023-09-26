using StockportGovUK.NetStandard.Gateways.Models.RevsAndBens;
using PersonName = StockportGovUK.NetStandard.Gateways.Models.Civica.CouncilTax.PersonName;

namespace revs_bens_service.Services.CouncilTax
{
    public interface ICouncilTaxService
    {
        Task<CouncilTaxDetailsModel> GetBaseCouncilTaxAccount(string personReference);

        Task<List<CouncilTaxAccountDetails>> GetCouncilTaxAccounts(string personReference);

        Task<string> GetCurrentCouncilTaxAccountNumber(string personReference);

        Task<PersonName> GetPerson(string personReference);

        Task<CouncilTaxDetailsModel> GetReducedCouncilTaxDetails(string personReference, string accountReference, int year);

        Task<CouncilTaxDetailsModel> GetCouncilTaxDetails(string personReference, string accountReference, int year);

        Task<byte[]> GetDocumentForAccount(string personReference, string accountReference, string documentId);

        Task<List<CouncilTaxDocument>> GetDocumentsForPerson(string personReference);
    }
}
