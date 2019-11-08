using revs_bens_service.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace revs_bens_service.Services
{
    public interface ICouncilTaxService
    {
        Task<IEnumerable<TransactionModelExtension>> GetAllTransactionsForYear(string personReference, string accountReference, int year);
    }
}
