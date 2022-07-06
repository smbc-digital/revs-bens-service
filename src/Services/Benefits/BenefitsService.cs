using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using revs_bens_service.Services.Benefits.Mappers;
using revs_bens_service.Utils.Parsers;
using revs_bens_service.Utils.StorageProvider;
using StockportGovUK.NetStandard.Gateways.CivicaService;
using StockportGovUK.NetStandard.Models.RevsAndBens;

namespace revs_bens_service.Services.Benefits
{
    public class BenefitsService : IBenefitsService
    {
        private readonly ICivicaServiceGateway _civicaServiceGateway;
        private readonly ICacheProvider _cacheProvider;

        public BenefitsService(ICivicaServiceGateway civicaServiceGateway, ICacheProvider cacheProvider)
        {
            _civicaServiceGateway = civicaServiceGateway;
            _cacheProvider = cacheProvider;
        }

        public async Task<bool> IsBenefitsClaimant(string personReference)
        {
            var cacheResponse = await _cacheProvider.GetStringAsync($"{personReference}-{CacheKeys.HasBenefits}");
            
            if (!string.IsNullOrEmpty(cacheResponse))
                return JsonConvert.DeserializeObject<bool>(cacheResponse);

            var response = await _civicaServiceGateway.IsBenefitsClaimant(personReference);

            if (!response.IsSuccessStatusCode)
            {
                throw response.StatusCode switch
                {
                    HttpStatusCode.NotFound => new ArgumentException(response.ReasonPhrase),
                    _ => new Exception($"IsBenefitsClaimant({personReference}) failed with status code: {response.StatusCode}")
                };
            }
            
            _ = _cacheProvider.SetStringAsync($"{personReference}-{CacheKeys.HasBenefits}", JsonConvert.SerializeObject(response.Parse<bool>().ResponseContent));
            return response.Parse<bool>().ResponseContent;
        }

        public async Task<Claim> GetBenefits(string personReference)
        {
            var cacheResponse = await _cacheProvider.GetStringAsync($"{personReference}-{CacheKeys.BenefitDetails}");

            if (!string.IsNullOrEmpty(cacheResponse))
            {
                return JsonConvert.DeserializeObject<Claim>(cacheResponse);
            }

            var benefitsResponse = await _civicaServiceGateway.GetBenefits(personReference);
            var claims = benefitsResponse.Parse<List<BenefitsClaimSummary>>().ResponseContent;

            var claim = new Claim {
                Details = claims.Select( _ => GetDetails(personReference, _.Number, _.PlaceReference).Result).FirstOrDefault(_ => _.Status == "Current"),
                BenefitsSummary = GetBenefitsSummary(personReference).Result,
                Documents = GetDocuments(personReference).Result,
                HousingBenefitPaymentHistory = GetHousingBenefitsPayments(personReference).Result,
                CouncilTaxPaymentHistory = GetCouncilTaxPayments(personReference).Result,
            };

            _ = _cacheProvider.SetStringAsync($"{personReference}-{CacheKeys.BenefitDetails}", JsonConvert.SerializeObject(claim));

            return claim;
        }

        private async Task<ClaimDetails> GetDetails(
            string personReference,
            string claimNumber,
            string placeReference)
        {
            var response = await _civicaServiceGateway.GetBenefitDetails(personReference, claimNumber, placeReference);
            var benefitsClaim = response.Parse<BenefitsClaim>().ResponseContent;

            return benefitsClaim.MapToClaimDetails();
        }

        private async Task<List<BenefitsDocument>> GetDocuments(string personReference)
        {
            var response = await _civicaServiceGateway.GetDocuments(personReference);
            var documents = response.Parse<List<CouncilTaxDocument>>().ResponseContent;

            return documents?.MapToDocuments().Where(_ => _.Type == "Notif").ToList() ?? new List<BenefitsDocument>();
        }

        private async Task<List<Payment>> GetHousingBenefitsPayments(string personReference)
        {
            var response = await _civicaServiceGateway.GetHousingBenefitPaymentHistory(personReference);
            var payments = response.Parse<List<PaymentDetail>>().ResponseContent;

            return payments?.MapToPayments() ?? new List<Payment>();
        }

        private async Task<List<Payment>> GetCouncilTaxPayments(string personReference)
        {
            var response = await _civicaServiceGateway.GetCouncilTaxBenefitPaymentHistory(personReference);
            var payments = response.Parse<List<PaymentDetail>>().ResponseContent;

            return payments?.MapToPayments() ?? new List<Payment>();
        }

        private async Task<BenefitsSummary> GetBenefitsSummary(string personReference)
        {
            var payments = await GetCouncilTaxPayments(personReference);
            var currentTaxYear = ToFinancialYear(DateTime.Now);

            var currentYearPayments = payments
                .Where(_ => ToFinancialYear(DateTime.Parse(_.PeriodStart)) == currentTaxYear)
                .ToList();

            var accountReference = currentYearPayments.FirstOrDefault() != null
                ? currentYearPayments.First().CouncilTaxReference
                : "N/A";

            var response = await _civicaServiceGateway.GetAccountDetailsForYear(personReference, accountReference, currentTaxYear);
            var responseTotals = response.Parse<ReceivedYearTotal>().ResponseContent;

            if (responseTotals == null)
            {
                return new BenefitsSummary
                {
                    TaxYear = currentTaxYear,
                    AccountReference = accountReference,
                };
            }

            return new BenefitsSummary
            {
                TaxYear = currentTaxYear,
                AccountReference = accountReference,
                TotalBill = responseTotals.TotalCharge ?? "0.00",
                TotalBenefits = responseTotals.TotalBenefits ?? "0.00",
                BalanceOutstanding = responseTotals.BalanceOutstanding ?? "0.00"
            };
        }

        private int ToFinancialYear(DateTime date) => date.Month < 4 ? date.Year - 1 : date.Year;
    }
}
