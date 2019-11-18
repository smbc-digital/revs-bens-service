using System;
using System.Collections.Generic;
using System.Linq;
using StockportGovUK.NetStandard.Models.RevsAndBens;

namespace revs_bens_service.Services.Benefits.Mappers
{
    public static class BenefitsMapper
    {
        public static ClaimDetails MapToClaimDetails(this BenefitsClaim claim)
        {
            var response = new ClaimDetails
            {
                Number = claim.Number,
                Status = ParseStatusCode(claim.Status),
                NextPayment = SetNextPayment(claim),
                Address = string.Join(", ", new [] { claim.Address1, claim.Address2, claim.Address3, claim.Address4, claim.Postcode }.Where(_ => !string.IsNullOrEmpty(_))),
                CurrentEntitlement = ParseBenefitsEntitlement(claim.BenefitEntitlement),
                BenefitsCombination = SetBenefitsCombination(claim.BenefitEntitlement, claim.Status)
            };

            return response;
        }

        public static List<BenefitsDocument> MapToDocuments(this List<CouncilTaxDocument> documents)
        {
            return documents.Select(_ => new BenefitsDocument
            {
                AccountReference = _.AccountReference,
                DateCreated = DateTime.Parse(_.DateCreated),
                Downloaded = _.Downloaded.ToLower() == "yes",
                Id = _.DocumentId,
                Type = _.DocumentType,
                Name = _.DocumentName
            })
            .ToList();
        }

        public static List<Payment> MapToPayments(this List<PaymentDetail> payments)
        {
            return payments.Select(_ => new Payment
            {
                DatePaid = DateTime.Parse(_.DatePaid),
                Amount = Convert.ToDecimal(_.PayAmount),
                PeriodStart = DateTime.Parse(_.PeriodStart),
                PeriodEnd = DateTime.Parse(_.PeriodEnd),
                Type = _.PayType,
                Payee = _.Payee,
                OnAct = _.OnAct.ToLower() == "yes",
                CouncilTaxReference = _.CouncilTaxReference
            })
            .ToList();
        }

        private static NextPayment SetNextPayment(BenefitsClaim claim)
        {
            return new NextPayment
            {
                Amount = Convert.ToDecimal(claim.NextPayment.Amount),
                Method = claim.NextPayment.Method,
                PaidUpToAmount = Convert.ToDecimal(claim.NextPayment.PaidUpToAmount),
                Payee = claim.NextPayment.Payee,
                DueDate = !string.IsNullOrEmpty(claim.NextPayment.PaymentDueDate) ? DateTime.Parse(claim.NextPayment.PaymentDueDate) : new DateTime(),
                Schedule = claim.NextPayment.Schedule,
                Status = SetPaymentStatus(claim.NextPayment.Amount, claim.BenefitEntitlement, claim.NextPayment.Schedule)
            };
        }

        private static string SetPaymentStatus(string amount, BenefitEntitlement benefitEntitlement, string paymentSchedule)
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

        private static string ParseStatusCode(string statusCode)
        {
            switch (statusCode)
            {
                case "1":
                    return "Current";
                case "2":
                    return "Future";
                case "3":
                    return "Cancelled";
                case "4":
                    return "Pending";
                case "5":
                    return "Suspended";
                case "6":
                    return "Defective";
                default:
                    return "Unknown";
            }
        }

        private static CurrentEntitlement ParseBenefitsEntitlement(BenefitEntitlement benefitsEntitlement)
        {
            if (benefitsEntitlement == null)
            {
                return new CurrentEntitlement();
            }

            var ctax = benefitsEntitlement.CouncilTax != null
                ? Convert.ToDecimal(benefitsEntitlement.CouncilTax.WeeklyBenefit)
                : (decimal)0.00;

            var rentType = benefitsEntitlement.PrivateRent ?? benefitsEntitlement.CouncilRent;
            var housingBenefit = (decimal)0.00;

            if (rentType != null)
            {
                housingBenefit = Convert.ToDecimal(rentType.WeeklyBenefit);
            }

            return new CurrentEntitlement
            {
                WeeklyHousingBenefitEntitlement = housingBenefit,
                WeeklyCtaxBenefitEntitlement = ctax
            };
        }

        private static BenefitsCombinationEnum SetBenefitsCombination(BenefitEntitlement benefitEntitlement, string claimStatus)
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
    }
}