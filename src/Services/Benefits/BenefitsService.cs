using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using revs_bens_service.Services.Benefits.Mappers;
using revs_bens_service.Utils.Parsers;
using revs_bens_service.Utils.StorageProvider;
using StockportGovUK.AspNetCore.Gateways.CivicaServiceGateway;
using StockportGovUK.NetStandard.Models.RevsAndBens;

namespace revs_bens_service.Services.HousingBenefits
{
    public class BenefitsService : IBenefitsService
    {
        private readonly ICivicaServiceGateway _civicaServiceGateway;
        private readonly IDistributedCache _cacheProvider;

        public BenefitsService(ICivicaServiceGateway civicaServiceGateway, IDistributedCache cacheProvider)
        {
            _civicaServiceGateway = civicaServiceGateway;
            _cacheProvider = cacheProvider;
        }

        public async Task<bool> IsBenefitsClaimant(string personReference)
        {
            var response = await _civicaServiceGateway.IsBenefitsClaimant(personReference);

            if (response.IsSuccessStatusCode)
            {
                return response.Parse<bool>().ResponseContent;
            }

            throw new Exception($"IsBenefistClaimant({personReference}) failed with status code: {response.StatusCode}");
        }

        public async Task<Claim> GetBenefitsDetails(string personReference)
        {
            var cacheResponse = await _cacheProvider.GetStringAsync($"{personReference}-{CacheKeys.BenefitDetails}");

            if(!string.IsNullOrEmpty(cacheResponse))
            {
                return JsonConvert.DeserializeObject<Claim>(cacheResponse);
            }

            var benefitClaims = new List<Claim>();
            var benefitsResponse = await _civicaServiceGateway.GetBenefits(personReference);
            var claims = benefitsResponse.Parse<List<BenefitsClaimSummary>>().ResponseContent;

            if (!claims.Any())
            {
                return null;
            }

            foreach (var claim in claims)
            {
                var model = new Claim();

                await Task.WhenAll(
                    Task.Run(async () =>
                    {
                        var response = await _civicaServiceGateway.GetBenefitDetails(personReference, claim.Number, claim.PlaceReference);
                        var benefitsClaim = response.Parse<BenefitsClaim>().ResponseContent;
                        model.Details = benefitsClaim.MapToClaimDetails();
                    }),
                    Task.Run(async () =>
                    {
                        var response = await _civicaServiceGateway.GetDocuments(personReference);
                        var documents = response.Parse<List<CouncilTaxDocument>>().ResponseContent;
                        model.Documents = documents.MapToDocuments().Where(_ => _.Type == "Notif").ToList();
                    }),
                    Task.Run(async () =>
                    {
                        var response = await _civicaServiceGateway.GetHousingBenefitPaymentHistory(personReference);
                        var payments = response.Parse<List<PaymentDetail>>().ResponseContent;
                        model.HousingBenefitPaymentHistory = payments.MapToPayments();
                    }),
                    Task.Run(async () =>
                    {
                        var response = await _civicaServiceGateway.GetCouncilTaxBenefitPaymentHistory(personReference);
                        var payments = response.Parse<List<PaymentDetail>>().ResponseContent;
                        model.CouncilTaxPaymentHistory = payments.MapToPayments();
                        model.BenefitsSummary = await BuildBenefitsSummary(personReference, payments);
                    })
                );

                benefitClaims.Add(model);
            }

            var claimResponse = benefitClaims.FirstOrDefault(_ => _.Details.Status == "Current");

            _ = _cacheProvider.SetStringAsync($"{personReference}-{CacheKeys.BenefitDetails}", JsonConvert.SerializeObject(claimResponse));

            return claimResponse;
        }

        private int ToFinancialYear(DateTime date) => date.Month < 4 ? date.Year - 1 : date.Year;

        private async Task<BenefitsSummary> BuildBenefitsSummary(string personReference, List<PaymentDetail> councilTaxSupportPayments)
        {
            var currentTaxYear = ToFinancialYear(DateTime.Now);

            var currentYearPayments = councilTaxSupportPayments
                    .Where(_ => ToFinancialYear(DateTime.Parse(_.PeriodStart)) == currentTaxYear)
                    .ToList();

            var accountReference = currentYearPayments.FirstOrDefault() != null
                ? currentYearPayments.First().CouncilTaxReference
                : "N/A";

            var response = new BenefitsSummary
            {
                TaxYear = currentTaxYear,
                AccountReference = accountReference,
            };

            var accountResponse = await _civicaServiceGateway.GetAccountDetailsForYear(personReference, accountReference, currentTaxYear);
            var totals = accountResponse.Parse<RecievedYearTotal>().ResponseContent;

            if (totals == null)
            {
                return response;
            }

            response.TotalBill = !string.IsNullOrEmpty(totals.TotalCharge) ? Convert.ToDecimal(totals.TotalCharge) : (decimal)0.00;
            response.TotalBenefits = !string.IsNullOrEmpty(totals.TotalBenefits) ? Convert.ToDecimal(totals.TotalBenefits) : (decimal)0.00;
            response.BalanceOutstanding = !string.IsNullOrEmpty(totals.BalanceOutstanding) ? Convert.ToDecimal(totals.BalanceOutstanding) : (decimal)0.00;

            return response;
        }
    }
}
