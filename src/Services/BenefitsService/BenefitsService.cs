using System;
using System.Threading.Tasks;
using StockportGovUK.AspNetCore.Gateways.CivicaServiceGateway;
using StockportGovUK.NetStandard.Models.Models.RevsAndBens;

namespace revs_bens_service.Services.BenefitsService
{
    public class BenefitsService
    {
        private readonly ICivicaServiceGateway _gateway;
        public BenefitsService(ICivicaServiceGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<Benefits> GetBenefitDetails(string personReference)
        {
            var response = await _gateway.GetBenefitSummary(personReference);

            if (response.IsSuccessStatusCode)
            {
                return response.Parse<Benefits>().ResponseContent;
            }

            throw new Exception($"GetBenefitsSummary({personReference}) failed with status code: {response.StatusCode}");
        }
    }
}
