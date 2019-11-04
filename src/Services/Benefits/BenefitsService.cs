using System.Threading.Tasks;
using StockportGovUK.AspNetCore.Gateways.CivicaServiceGateway;

namespace revs_bens_service.Services.Benefits
{
    public class BenefitsService
    {
        private readonly ICivicaServiceGateway _gateway;
        public BenefitsService(ICivicaServiceGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<bool> BenefitDetails(string personReference)
        {
        
        }
    }
}
