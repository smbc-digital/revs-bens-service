using System;
using System.Threading.Tasks;
using StockportGovUK.AspNetCore.Gateways.CivicaServiceGateway;

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
            var response = await _gateway.IsBenefitsClaimant(personReference);

            if (response.IsSuccessStatusCode)
            {
                return bool.Parse(await response.Content.ReadAsStringAsync());
            }

            throw new Exception($"IsBenefistClaimant({personReference}) failed with status code: {response.StatusCode}");
        }
    }
}
