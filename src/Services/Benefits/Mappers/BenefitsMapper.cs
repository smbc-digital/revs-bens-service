using System.Collections.Generic;
using System.Linq;
using StockportGovUK.NetStandard.Models.RevsAndBens;
using StockportGovUK.NetStandard.Models.Enums;

namespace revs_bens_service.Services.Benefits.Mappers
{
    public static class BenefitsMapper
    {
        public static ClaimDetails MapToClaimDetails(this BenefitsClaim claim) =>
            new ClaimDetails
            {
                PersonName = $"{claim.PersonName.Forenames} {claim.PersonName.Surname}",
                Number = claim.Number,
                Status = ParseStatusCode(claim.Status),
                NextPayment = SetNextPayment(claim),
                Address = string.Join(", ", new[] { claim.Address1, claim.Address2, claim.Address3, claim.Address4, claim.Postcode }.Where(_ => !string.IsNullOrEmpty(_))),
                CurrentEntitlement = ParseBenefitsEntitlement(claim.BenefitEntitlement),
                BenefitsCombination = SetBenefitsCombination(claim.BenefitEntitlement, claim.Status)
            };

        public static List<BenefitsDocument> MapToDocuments(this List<CouncilTaxDocument> documents) =>
            documents.Select(_ => new BenefitsDocument
            {
                AccountReference = _.AccountReference,
                DateCreated = _.DateCreated,
                Downloaded = _.Downloaded,
                Id = _.DocumentId,
                Type = _.DocumentType,
                Name = _.DocumentName
            })
            .ToList();

        public static List<Payment> MapToPayments(this List<PaymentDetail> payments) =>
            payments.Select(_ => new Payment
            {
                DatePaid = _.DatePaid,
                Amount = _.PayAmount,
                PeriodStart = _.PeriodStart,
                PeriodEnd = _.PeriodEnd,
                Type = _.PayType,
                Payee = _.Payee,
                OnAct = _.OnAct,
                CouncilTaxReference = _.CouncilTaxReference
            })
            .ToList();

        private static ClaimNextPayment SetNextPayment(BenefitsClaim claim) =>
            new ClaimNextPayment
            {
                Amount = claim.NextPayment.Amount,
                Method = claim.NextPayment.Method,
                PaidUpToAmount = claim.NextPayment.PaidUpToAmount,
                Payee = claim.NextPayment.Payee,
                DueDate = claim.NextPayment.PaymentDueDate,
                Schedule = claim.NextPayment.Schedule,
                Status = SetPaymentStatus(claim.NextPayment.Amount, claim.BenefitEntitlement, claim.NextPayment.Schedule)
            };

        private static EPaymentStatus SetPaymentStatus(
            string amount,
            BenefitEntitlement benefitEntitlement,
            string paymentSchedule)
        {
            var weeks = 0;

            switch (paymentSchedule.ToLower())
            {
                case "weekly":
                    return EPaymentStatus.Expected;
                case "fortnightly":
                    weeks = 2;
                    break;
                case "four weekly":
                    weeks = 4;
                    break;
                default:
                    return EPaymentStatus.Expected;
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
                return EPaymentStatus.Expected;

            return actualPayment < expectedPayment
                ? EPaymentStatus.Reduced
                : EPaymentStatus.Increased;
        }

        private static string ParseStatusCode(string statusCode) =>
            statusCode switch
            {
                "1" => "Current",
                "2" => "Future",
                "3" => "Cancelled",
                "4" => "Pending",
                "5" => "Suspended",
                "6" => "Defective",
                _   => "Unknown"
            };

        private static CurrentEntitlement ParseBenefitsEntitlement(BenefitEntitlement benefitsEntitlement)
        {
            if (benefitsEntitlement == null)
                return new CurrentEntitlement();

            var ctax = benefitsEntitlement.CouncilTax != null
                ? benefitsEntitlement.CouncilTax.WeeklyBenefit
                : "0.00";

            var rentType = benefitsEntitlement.PrivateRent ?? benefitsEntitlement.CouncilRent;
            var housingBenefit = "0.00";

            if (rentType != null)
                housingBenefit = rentType.WeeklyBenefit;

            return new CurrentEntitlement
            {
                WeeklyHousingBenefitEntitlement = housingBenefit,
                WeeklyCtaxBenefitEntitlement = ctax
            };
        }

        private static BenefitsCombinationEnum SetBenefitsCombination(
            BenefitEntitlement benefitEntitlement,
            string claimStatus)
        {
            var benefitsCombo = string.Empty;

            var ctax = benefitEntitlement.CouncilTax != null
                ? benefitEntitlement.CouncilTax.WeeklyBenefit
                : "0.00";

            var rentType = benefitEntitlement.PrivateRent ?? benefitEntitlement.CouncilRent;
            var housingBenefit = rentType != null
                ? rentType.WeeklyBenefit
                : "0.00";

            benefitsCombo += housingBenefit == "0.00" && ParseStatusCode(claimStatus) == "Current" ? string.Empty : "[HB]";
            benefitsCombo += ctax == "0.00" ? string.Empty : "[CTS]";

            return benefitsCombo switch
            {
                "[HB]"      => BenefitsCombinationEnum.HousingBenefitOnly,
                "[CTS]"     => BenefitsCombinationEnum.CouncilTaxSupportOnly,
                "[HB][CTS]" => BenefitsCombinationEnum.AllBenefits,
                _           => BenefitsCombinationEnum.HousingBenefitOnly
            };
        }
    }
}