using System.Threading.Tasks;
using StockportGovUK.AspNetCore.Gateways;

namespace revs_bens_service.Services.Dashboard
{
    public class PersonService : IPersonService
    {
        private readonly ICivicaServiceGateway _gateway;
        public PersonService(ICivicaServiceGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<bool> IsBenefitsClaimant(string personReference)
        {
            return await _gateway.IsBenefitsClaimant(personReference);
        }
    }
}
