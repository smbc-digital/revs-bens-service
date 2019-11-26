using revs_bens_service.Services.Models;
using revs_bens_service.Utils.Parsers;
using StockportGovUK.AspNetCore.Gateways.CivicaServiceGateway;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using revs_bens_service.Services.CouncilTax.Mappers;
using revs_bens_service.Utils.StorageProvider;
using StockportGovUK.NetStandard.Models.Models.Civica.CouncilTax;

namespace revs_bens_service.Services.CouncilTax
{
    public class CouncilTaxService : ICouncilTaxService
    {
        private readonly ICivicaServiceGateway _gateway;
        private readonly IDistributedCache _cacheProvider;

        public CouncilTaxService(ICivicaServiceGateway gateway, IDistributedCache cacheProvider)
        {
            _gateway = gateway;
            _cacheProvider = cacheProvider;
        }

        public async Task<CouncilTaxDetailsModel> GetCouncilTaxDetails(string personReference, string accountReference, int year)
        {
            var cacheResponse = await _cacheProvider.GetStringAsync($"{personReference}-{CacheKeys.CouncilTaxDetails}");

            if (!string.IsNullOrEmpty(cacheResponse))
            {
                return JsonConvert.DeserializeObject<CouncilTaxDetailsModel>(cacheResponse);
            }

            var model = new CouncilTaxDetailsModel();

            var accountResponse = await _gateway.GetAccount(personReference, accountReference);
            model = accountResponse.Parse<CouncilTaxAccountResponse>().ResponseContent.MapAccount(model);

            var transactionsResponse = await _gateway.GetAllTransactionsForYear(personReference, accountReference, year);
            model = transactionsResponse.Parse<TransactionResponse>().ResponseContent.MapTransactions(model);

            var paymentResponse = await _gateway.GetPaymentSchedule(personReference, year);
            model = paymentResponse.Parse<CouncilTaxPaymentScheduleResponse>().ResponseContent.MapPayments(model);

            var currentPropertyResponse = await _gateway.GetCurrentProperty(personReference);
            model = currentPropertyResponse.Parse<Places>().ResponseContent.MapCurrentProperty(model);

            var isBenefitsResponse = await _gateway.IsBenefitsClaimant(personReference);
            model.HasBenefits = isBenefitsResponse.Parse<bool>().ResponseContent;

            _ = _cacheProvider.SetStringAsync($"{personReference}-{CacheKeys.CouncilTaxDetails}", JsonConvert.SerializeObject(model));

            return model;
        }
    }
}
