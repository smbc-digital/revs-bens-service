using System.Net;
using System.Threading.Tasks;
using StockportGovUK.NetStandard.Gateways.CivicaService;

namespace revs_bens_service.Services.Availability
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly ICivicaServiceGateway _civicaServiceGateway;

        public AvailabilityService(ICivicaServiceGateway civicaServiceGateway)
        {
            _civicaServiceGateway = civicaServiceGateway;
        }

        public async Task<bool> GetCivicaAvailability()
        {
            var result = await _civicaServiceGateway.GetAvailability();

            return result.StatusCode == HttpStatusCode.OK;
        }
    }
}