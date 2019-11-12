using System;
using System.Threading.Tasks;
using revs_bens_service.Utils.Parsers;
using StockportGovUK.AspNetCore.Gateways.CivicaServiceGateway;

namespace revs_bens_service.Services.Dashboard
{
    public class PeopleService : IPeopleService
    {
        private readonly ICivicaServiceGateway _gateway;
        public PeopleService(ICivicaServiceGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<bool> IsBenefitsClaimant(string personReference)
        {
            var response = await _gateway.IsBenefitsClaimant(personReference);

            if (response.IsSuccessStatusCode)
            {
                return response.Parse<bool>().ResponseContent;
            }

            throw new Exception($"IsBenefistClaimant({personReference}) failed with status code: {response.StatusCode}");
        }
    }
}
