using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using revs_bens_service.Services.HousingBenefits.Models;
using revs_bens_service.Services.Models;
using revs_bens_service.Utils.Parsers;
using StockportGovUK.AspNetCore.Gateways.CivicaServiceGateway;
using StockportGovUK.NetStandard.Models.Models.RevsAndBens;

namespace revs_bens_service.Services.HousingBenefits
{
    public class BenefitsService : IBenefitsService
    {
        private readonly ICivicaServiceGateway _civicaServiceGateway;
        public BenefitsService(ICivicaServiceGateway civicaServiceGateway)
        {
            _civicaServiceGateway = civicaServiceGateway;
        }

        public async Task<List<Benefits>> GetBenefits(string personReference)
        {
            var benefitClaims = new List<Benefits>();
            var benefitsResponse = await _civicaServiceGateway.GetBenefits(personReference);
            var benefitsDetails = benefitsResponse.Parse<ClaimsSummaryResponse>().ResponseContent;

            foreach (var claim in benefitsDetails.ClaimsList.ClaimSummary)
            {
                dynamic model = new ExpandoObject();

                Task.WaitAll(
                    Task.Run(async () =>
                    {
                        var response = await _civicaServiceGateway.GetBenefitDetails(personReference, claim.ClaimNumber, claim.ClaimPlaceRef);
                        model.ClaimDetails = response.Parse<ClaimDetails>().ResponseContent;
                    }),
                    Task.Run(async () =>
                    {
                        var response = await _civicaServiceGateway.GetDocumentsList(personReference);
                        model.Documents = response.Parse<List<Document>>().ResponseContent;
                    }),
                    Task.Run(async () =>
                    {
                        var response = await _civicaServiceGateway.GetHousingBenefitPaymentHistory(personReference);
                        model.HousingPaymentsHistory = response.Parse<List<HousingBenefitsPaymentDetail>>().ResponseContent;
                    }),
                    Task.Run(async() =>
                    {
                        var response = await _civicaServiceGateway.GetCtaxbenefitPaymentHistory(personReference);
                        var content = response.Parse<List<CtaxBenefitsPaymentDetail>>().ResponseContent;
                        model.CouncilTaxPaymentPaymentHistory = content;
                        model.CouncilTaxCurrentSummary = BuildCouncilTaxSupportSummary(personReference, content);
                    })
                );

                benefitClaims.Add(model);
            }

            return benefitClaims;
        }

        private static string ParseStatusCode(string statusCode)
        {
            switch (statusCode)
            {
                case "1":
                    return "Current";
                case "2":
                    return "Future";
                case "3":
                    return "Cancelled"; // Civica calls this 'Old'
                case "4":
                    return "Pending";
                case "5":
                    return "Suspended";
                case "6":
                    return "Defective"; // Civica calls this 'Unsuccessful'
                default:
                    return "Unknown";
            }
        }

        private static int ParsePaymentFrequencyInWeeks(string paymentSchedule)
        {
            switch (paymentSchedule.ToLower())
            {
                case "weekly":
                    return 1;
                case "fortnightly":
                    return 2;
                case "four weekly":
                    return 4;
                default:
                    return 0;
            }
        }

        // TODO: WTF IS THIS!?!?!?!
        private CtaxBenefitsSummary BuildCouncilTaxSupportSummary(string personReference, IEnumerable<CtaxBenefitsPaymentDetail> councilTaxSupportPayments)
        {
            var currentTaxYear = ToFinancialYear(DateTime.Now);

            var currentYearPayments =
                councilTaxSupportPayments
                    .Where(_ => ToFinancialYear(DateTime.Parse(_.PeriodStart)) == currentTaxYear)
                    .ToList();

            var accountReference = currentYearPayments.FirstOrDefault() != null ? currentYearPayments.First().CouncilTaxReference : "N/a";

            var ctaxBenefitsSummaryEntity = new CtaxBenefitsSummary
            {
                TaxYear = currentTaxYear,
                AccountReference = accountReference,
            };

            var ctaxSummaryResponse = _civicaServiceGateway.GetAccountDetailsForYear(personReference, accountReference, currentTaxYear);

            if (ctaxSummaryResponse.FinancialDetails == null || ctaxSummaryResponse.FinancialDetails.RecYrTotals == null)
            {
                return ctaxBenefitsSummaryEntity;
            }

            var yearTotal = ctaxSummaryResponse.FinancialDetails.RecYrTotals;
            ctaxBenefitsSummaryEntity.TotalBill = yearTotal.TotalCharge == null ? "0.00" : yearTotal.TotalCharge.Trim();
            ctaxBenefitsSummaryEntity.TotalPayments = yearTotal.TotalPayments == null ? "0.00" : yearTotal.TotalPayments.Trim();
            ctaxBenefitsSummaryEntity.TotalBenefits = yearTotal.TotalBenefits == null ? "0.00" : yearTotal.TotalBenefits.Trim();
            ctaxBenefitsSummaryEntity.BalanceOutstanding = yearTotal.BalanceOutstanding == null ? "0.00" : yearTotal.BalanceOutstanding.Trim();

            ctaxBenefitsSummaryEntity.BenefitsBreakdown =
                currentYearPayments.Select(
                    _ =>
                        new CouncilTaxBenefitsPayment
                        {
                            Amount = _.PayAmount,
                            PaymentDate = _.DatePaid,
                            PaymentPeriod = string.Concat(_.PeriodStart, " to ", _.PeriodEnd)
                        }).ToList();

            return ctaxBenefitsSummaryEntity;
        }

        private int ToFinancialYear(DateTime date) => date.Month < 4 ? date.Year -1 : date.Year;
    }
}
