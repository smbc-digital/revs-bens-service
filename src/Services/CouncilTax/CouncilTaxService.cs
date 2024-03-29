﻿using System.Net;
using Newtonsoft.Json;
using revs_bens_service.Services.Benefits;
using revs_bens_service.Services.CouncilTax.Mappers;
using revs_bens_service.Utils.Parsers;
using revs_bens_service.Utils.StorageProvider;
using StockportGovUK.NetStandard.Gateways.CivicaService;
using StockportGovUK.NetStandard.Gateways.Models.Civica.CouncilTax;
using StockportGovUK.NetStandard.Gateways.Models.RevsAndBens;
using PersonName = StockportGovUK.NetStandard.Gateways.Models.Civica.CouncilTax.PersonName;
using Transaction = revs_bens_service.Services.Models.Transaction;

namespace revs_bens_service.Services.CouncilTax
{
    public class CouncilTaxService : ICouncilTaxService
    {
        private readonly ICivicaServiceGateway _gateway;
        private readonly ICacheProvider _cacheProvider;
        private readonly IBenefitsService _benefitsService;
        private readonly string _current = "CURRENT";

        public CouncilTaxService(ICivicaServiceGateway gateway, ICacheProvider cacheProvider, IBenefitsService benefitsService)
        {
            _gateway = gateway;
            _cacheProvider = cacheProvider;
            _benefitsService = benefitsService;
        }

        public async Task<CouncilTaxDetailsModel> GetBaseCouncilTaxAccount(string personReference)
        {
            var key = $"{personReference}-{DateTime.Now.Year}-{CacheKeys.CouncilTaxDetails}";
            var cacheResponse = await _cacheProvider.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cacheResponse))
                return JsonConvert.DeserializeObject<CouncilTaxDetailsModel>(cacheResponse);

            var accountsResponse = await _gateway.GetAccounts(personReference);
            if (!accountsResponse.IsSuccessStatusCode)
            {
                throw accountsResponse.StatusCode switch
                {
                    HttpStatusCode.NotFound => new ArgumentException(accountsResponse.ReasonPhrase),
                    _ => new Exception($"GetBaseCouncilTaxAccount({personReference}) failed with status code: {accountsResponse.StatusCode}")
                };
            }
            var model = accountsResponse.Parse<List<CtaxActDetails>>().ResponseContent.MapAccounts(new CouncilTaxDetailsModel());

            if (!model.Accounts.Any())
            {
                var personResponse = await _gateway.GetPerson(personReference);
                var person = personResponse.Parse<PersonName>().ResponseContent;
                model.PersonName = $"{person.Forenames} {person.Surname}";
                return model;
            }

            var reference = model.Accounts.Any(_ => _.Status.Equals(_current))
                ? model.Accounts.First(_ => _.Status.Equals(_current)).Reference
                : model.Accounts.First().Reference;

            var account = await _gateway.GetAccount(personReference, reference);
            model = account.Parse<CouncilTaxAccountResponse>().ResponseContent.MapAccount(model, DateTime.Now.Year);

            _ = _cacheProvider.SetStringAsync(key, JsonConvert.SerializeObject(model));

            return model;
        }

        public async Task<List<CouncilTaxAccountDetails>> GetCouncilTaxAccounts(string personReference)
        {
            var key = $"{personReference}-{DateTime.Now.Year}-{CacheKeys.CouncilTaxAccounts}";
            var cacheResponse = await _cacheProvider.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cacheResponse))
                return JsonConvert.DeserializeObject<CouncilTaxDetailsModel>(cacheResponse).Accounts.ToList();

            var accountsResponse = await _gateway.GetAccounts(personReference);
            var model = accountsResponse.Parse<List<CtaxActDetails>>().ResponseContent.MapAccounts(new CouncilTaxDetailsModel());

            _ = _cacheProvider.SetStringAsync(key, JsonConvert.SerializeObject(model));

            return model.Accounts.ToList();
        }

        public async Task<string> GetCurrentCouncilTaxAccountNumber(string personReference)
        {
            var accounts = await GetCouncilTaxAccounts(personReference);

            if (!accounts.Any())
                return string.Empty;

            var reference = accounts.Any(_ => _.Status.Equals(_current))
                ? accounts.First(_ => _.Status.Equals(_current)).Reference
                : accounts.First().Reference;

            return reference;
        }

        public async Task<PersonName> GetPerson(string personReference)
        {
            var person = await _gateway.GetPerson(personReference);
            return person.Parse<PersonName>().ResponseContent;
        }

        public async Task<CouncilTaxDetailsModel> GetReducedCouncilTaxDetails(string personReference, string accountReference, int year)
        {
            var trimmedAccountReference = accountReference.Trim();
            var key = $"{personReference}-{trimmedAccountReference}-{year}-{CacheKeys.ReducedCouncilTaxDetails}";
            var cacheResponse = await _cacheProvider.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cacheResponse))
                return JsonConvert.DeserializeObject<CouncilTaxDetailsModel>(cacheResponse);

            var model = new CouncilTaxDetailsModel();

            var accountResponse = await _gateway.GetAccount(personReference, trimmedAccountReference);
            model = accountResponse.Parse<CouncilTaxAccountResponse>().ResponseContent.MapAccount(model, year);

            model.Accounts = await GetCouncilTaxAccounts(personReference);

            var documents = await GetDocumentsForPerson(personReference);
            model = documents.DocumentsMapper(model, year);

            model.HasBenefits = await _benefitsService.IsBenefitsClaimant(personReference);

            model.TransactionHistory = new List<TransactionModel>();
            model.UpcomingPayments = new List<InstallmentModel>();
            model.PreviousPayments = new List<TransactionModel>();

            _ = _cacheProvider.SetStringAsync(key, JsonConvert.SerializeObject(model));

            return model;
        }

        public async Task<CouncilTaxDetailsModel> GetCouncilTaxDetails(string personReference, string accountReference, int year)
        {
            var trimmedAccountReference = accountReference.Trim();
            var key = $"{personReference}-{trimmedAccountReference}-{year}-{CacheKeys.CouncilTaxDetails}";
            var cacheResponse = await _cacheProvider.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cacheResponse))
                return JsonConvert.DeserializeObject<CouncilTaxDetailsModel>(cacheResponse);

            var model = new CouncilTaxDetailsModel();

            var accountResponse = await _gateway.GetAccount(personReference, trimmedAccountReference);
            model = accountResponse.Parse<CouncilTaxAccountResponse>().ResponseContent.MapAccount(model, year);

            model.Accounts = await GetCouncilTaxAccounts(personReference);

            var transactionsResponse = await _gateway.GetAllTransactionsForYear(personReference, trimmedAccountReference, year);
            model = transactionsResponse.Parse<List<Transaction>>().ResponseContent.MapTransactions(model);

            var paymentResponse = await _gateway.GetPaymentSchedule(personReference, year);
            model = paymentResponse.Parse<List<Installment>>().ResponseContent.MapPayments(model);

            var currentPropertyResponse = await _gateway.GetCurrentProperty(personReference, trimmedAccountReference);
            model = currentPropertyResponse.Parse<Place>().ResponseContent.MapCurrentProperty(model);

            var documents = await GetDocumentsForPerson(personReference);
            model = documents.DocumentsMapper(model, year);

            model.HasBenefits = await _benefitsService.IsBenefitsClaimant(personReference);

            _ = _cacheProvider.SetStringAsync(key, JsonConvert.SerializeObject(model));

            return model;
        }

        public async Task<byte[]> GetDocumentForAccount(string personReference, string accountReference, string documentId)
        {
            var trimmedAccountReference = accountReference.Trim();
            var key = $"{personReference}-{trimmedAccountReference}-{documentId}-{CacheKeys.CouncilTaxDetails}";
            var cacheResponse = await _cacheProvider.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cacheResponse))
                return JsonConvert.DeserializeObject<byte[]>(cacheResponse);

            var response = await _gateway.GetDocumentForAccount(personReference, trimmedAccountReference, documentId);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            var document = await response.Content.ReadAsByteArrayAsync();

            _ = _cacheProvider.SetStringAsync(key, JsonConvert.SerializeObject(document));

            return document;
        }

        public async Task<List<CouncilTaxDocument>> GetDocumentsForPerson(string personReference)
        {
            var key = $"{personReference}-{CacheKeys.Documents}";

            var cacheResponse = await _cacheProvider.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cacheResponse))
                return JsonConvert.DeserializeObject<List<CouncilTaxDocument>>(cacheResponse);

            var response = await _gateway.GetDocuments(personReference);

            if (response.StatusCode.Equals(HttpStatusCode.NotFound))
                return null;

            var documents = response.Parse<List<CouncilTaxDocument>>().ResponseContent;

            _ = _cacheProvider.SetStringAsync(key, JsonConvert.SerializeObject(documents));

            return documents;
        }
    }
}
