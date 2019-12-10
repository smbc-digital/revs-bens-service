﻿using System.Collections.Generic;
using System.Net;
using revs_bens_service.Utils.Parsers;
using StockportGovUK.AspNetCore.Gateways.CivicaServiceGateway;
using System.Threading.Tasks;
using Newtonsoft.Json;
using revs_bens_service.Services.CouncilTax.Mappers;
using revs_bens_service.Utils.StorageProvider;
using StockportGovUK.NetStandard.Models.Civica.CouncilTax;
using CouncilTaxDetailsModel = StockportGovUK.NetStandard.Models.RevsAndBens.CouncilTaxDetailsModel;
using Transaction = revs_bens_service.Services.Models.Transaction;
using StockportGovUK.NetStandard.Models.RevsAndBens;

namespace revs_bens_service.Services.CouncilTax
{
    public class CouncilTaxService : ICouncilTaxService
    {
        private readonly ICivicaServiceGateway _gateway;
        private readonly ICacheProvider _cacheProvider;

        public CouncilTaxService(ICivicaServiceGateway gateway, ICacheProvider cacheProvider)
        {
            _gateway = gateway;
            _cacheProvider = cacheProvider;
        }

        public async Task<CouncilTaxDetailsModel> GetCouncilTaxDetails(string personReference, string accountReference, int year)
        {
            var key = $"{personReference}-{accountReference}-{year}-{CacheKeys.CouncilTaxDetails}";
            var cacheResponse = await _cacheProvider.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cacheResponse))
            {
                var cachedResponse = JsonConvert.DeserializeObject<CouncilTaxDetailsModel>(cacheResponse);
                return cachedResponse;
            }

            var model = new CouncilTaxDetailsModel();

            var accountResponse = await _gateway.GetAccount(personReference, accountReference);
            model = accountResponse.Parse<CouncilTaxAccountResponse>().ResponseContent.MapAccount(model, year);

            var accountsResponse = await _gateway.GetAccounts(personReference);
            model = accountsResponse.Parse<List<CtaxActDetails>>().ResponseContent.MapAccounts(model);

            var transactionsResponse = await _gateway.GetAllTransactionsForYear(personReference, accountReference, year);
            model = transactionsResponse.Parse<List<Transaction>>().ResponseContent.MapTransactions(model);

            var paymentResponse = await _gateway.GetPaymentSchedule(personReference, year);
            model = paymentResponse.Parse<List<StockportGovUK.NetStandard.Models.Civica.CouncilTax.Instalment>>().ResponseContent.MapPayments(model);

            var currentPropertyResponse = await _gateway.GetCurrentProperty(personReference, accountReference);
            model = currentPropertyResponse.Parse<Place>().ResponseContent.MapCurrentProperty(model);

            var documentsResponse = await _gateway.GetDocuments(personReference);
            model = documentsResponse.Parse<List<CouncilTaxDocumentReference>>().ResponseContent.DocumentsMapper(model, year);

            var isBenefitsResponse = await _gateway.IsBenefitsClaimant(personReference);
            model.HasBenefits = isBenefitsResponse.Parse<bool>().ResponseContent;

            _ = _cacheProvider.SetStringAsync(key, JsonConvert.SerializeObject(model));

            return model;
        }

        public async Task<byte[]> GetDocumentForAccount(string personReference, string accountReference, string documentId)
        {
            var key = $"{personReference}-{accountReference}-{documentId}-{CacheKeys.CouncilTaxDetails}";
            var cacheResponse = await _cacheProvider.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cacheResponse))
            {
                var cachedResponse = JsonConvert.DeserializeObject<byte[]>(cacheResponse);
                return cachedResponse;
            }

            var response = await _gateway.GetDocumentForAccount(personReference, accountReference, documentId);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            var document = await response.Content.ReadAsByteArrayAsync();

            _ = _cacheProvider.SetStringAsync(key, JsonConvert.SerializeObject(document));

            return document;
        }
    }
}
