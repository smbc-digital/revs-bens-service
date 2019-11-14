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

        public async Task<dynamic> GetBenefitsDetails(string personReference)
        {
            var benefitClaims = new List<dynamic>();
            var benefitsResponse = await _civicaServiceGateway.GetBenefits(personReference);
            var benefitsDetails = benefitsResponse.Parse<ClaimsSummaryResponse>().ResponseContent;

            if (benefitsDetails.Claims == null)
            {
                return null;
            }

            foreach (var claim in benefitsDetails.Claims.Summary)
            {
                dynamic model = new ExpandoObject();

                var response = await _civicaServiceGateway.GetBenefitDetails(personReference, claim.Number, claim.PlaceRef);
                var t = await response.Content.ReadAsStringAsync();
                var u = response.Parse<ClaimDetails>().ResponseContent;
                model.ClaimDetails = response.Parse<ClaimDetails>().ResponseContent;
                model.ClaimDetails.NextPayment.PaymentSchedule = SetPaymentStatus(
                    model.ClaimDetails.NextPayment.Amount,
                    model.ClaimDetails.BenefitEntitlement,
                    model.ClaimDetails.NextPayment.PaymentSchedule
                );
                model.ClaimDetails.BenefitsCombination = SetBenefitsCombination(
                    model.ClaimDetails.BenefitEntitlement,
                    model.ClaimDetails.ClaimStatus
                );

                var documents = await _civicaServiceGateway.GetDocuments(personReference);
                model.Documents = response.Parse<List<Document>>().ResponseContent;

                var housingPaymentHistory = await _civicaServiceGateway.GetHousingBenefitPaymentHistory(personReference);
                model.HousingPaymentsHistory = response.Parse<dynamic>().ResponseContent;

                var ctaxPaymentHistory = await _civicaServiceGateway.GetCtaxBenefitPaymentHistory(personReference);
                var content = response.Parse<dynamic>().ResponseContent;
                model.CouncilTaxPaymentPaymentHistory = content;
                model.CouncilTaxCurrentSummary = BuildCouncilTaxSupportSummary(personReference, content.PaymentList.PaymentDetails);


                benefitClaims.Add(model);
            }

            return benefitClaims.FirstOrDefault(_ => _.ClaimDetails.ClaimStatus == "Current");
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
        private async Task<CtaxBenefitsSummary> BuildCouncilTaxSupportSummary(string personReference, IEnumerable<CtaxBenefitsPaymentDetail> councilTaxSupportPayments)
        {
            var currentTaxYear = ToFinancialYear(DateTime.Now);

            var currentYearPayments = councilTaxSupportPayments
                    .Where(_ => ToFinancialYear(DateTime.Parse(_.PeriodStart)) == currentTaxYear)
                    .ToList();

            var accountReference = currentYearPayments.FirstOrDefault() != null
                ? currentYearPayments.First().CouncilTaxReference
                : "N/a";

            var ctaxBenefitsSummaryEntity = new CtaxBenefitsSummary
            {
                TaxYear = currentTaxYear,
                AccountReference = accountReference,
            };

            var ctaxSummaryResponse = await _civicaServiceGateway.GetAccountDetailsForYear(personReference, accountReference, currentTaxYear);
            var ctaxSummary = ctaxSummaryResponse.Parse<CouncilTaxSummaryResponse>().ResponseContent;

            if (ctaxSummary.FinancialDetails == null || ctaxSummary.FinancialDetails.RecYrTotals == null)
            {
                return ctaxBenefitsSummaryEntity;
            }

            var yearTotal = ctaxSummary.FinancialDetails.RecYrTotals;
            ctaxBenefitsSummaryEntity.TotalBill = yearTotal.TotalCharge == null ? "0.00" : yearTotal.TotalCharge.Trim();
            ctaxBenefitsSummaryEntity.TotalBenefits = yearTotal.TotalBenefits == null ? "0.00" : yearTotal.TotalBenefits.Trim();
            ctaxBenefitsSummaryEntity.BalanceOutstanding = yearTotal.BalanceOutstanding == null ? "0.00" : yearTotal.BalanceOutstanding.Trim();

            return ctaxBenefitsSummaryEntity;
        }

        private int ToFinancialYear(DateTime date) => date.Month < 4 ? date.Year - 1 : date.Year;

        private string SetPaymentStatus(string amount, BenefitEntitlementResponse benefitEntitlement, string paymentSchedule)
        {
            var weeks = 0;

            switch (paymentSchedule.ToLower())
            {
                case "weekly":
                    return "Expected";
                case "fortnightly":
                    weeks = 2;
                    break;
                case "four weekly":
                    weeks = 4;
                    break;
                default:
                    return "Expected";
            }

            var rentType = benefitEntitlement.PrivateRent ?? benefitEntitlement.CouncilRent;
            var housingBenefit = rentType != null
                ? rentType.WeeklyBenefit
                : "0.00";

            decimal nextPaymentAmount;
            decimal.TryParse(amount, out nextPaymentAmount);

            decimal expectedPayment;
            decimal.TryParse(housingBenefit, out expectedPayment);

            var actualPayment = nextPaymentAmount / weeks;

            if (actualPayment == expectedPayment)
            {
                return "Expected";
            }

            return actualPayment < expectedPayment
                ? "Reduced"
                : "Increased";
        }

        private BenefitsCombinationEnum SetBenefitsCombination(BenefitEntitlementResponse benefitEntitlementResponse, string claimStatus)
        {
            var benefitsCombo = string.Empty;

            var ctax = benefitEntitlementResponse.CouncilTax != null
                ? benefitEntitlementResponse.CouncilTax.WeeklyBenefit
                : "0.00";

            var rentType = benefitEntitlementResponse.PrivateRent ?? benefitEntitlementResponse.CouncilRent;
            var housingBenefit = rentType != null
                ? rentType.WeeklyBenefit
                : "0.00";

            benefitsCombo += housingBenefit == "0.00" && claimStatus == "Current" ? string.Empty : "[HB]";
            benefitsCombo += ctax == "0.00" ? string.Empty : "[CTS]";

            switch (benefitsCombo)
            {
                case "[HB]":
                    return BenefitsCombinationEnum.HousingBenefitOnly;
                case "[CTS]":
                    return BenefitsCombinationEnum.CouncilTaxSupportOnly;
                case "[HB][CTS]":
                    return BenefitsCombinationEnum.AllBenefits;
                default:
                    return BenefitsCombinationEnum.HousingBenefitOnly;
            }
        }

        public enum BenefitsCombinationEnum
        {
            AllBenefits = 0,
            HousingBenefitOnly = 1,
            CouncilTaxSupportOnly = 2
        }
    }
}
