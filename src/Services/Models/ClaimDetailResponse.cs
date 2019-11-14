using System.Xml.Serialization;

namespace revs_bens_service.Services.Models
{
    [XmlRoot("HBClaimDetails")]
    public class ClaimDetailResponse
    {
        [XmlElement("Error")]
        public string Error { get; set; }

        [XmlElement("HBClaimRef")]
        public string ClaimNumber { get; set; }

        [XmlElement("HBPlaceRef")]
        public string ClaimPlaceRef { get; set; }

        [XmlElement("HBClaimStatus")]
        public string ClaimStatus { get; set; }

        [XmlElement("HBPersonType")]
        public string PersonType { get; set; }

        [XmlElement("HBClaimType")]
        public string ClaimType { get; set; }

        [XmlElement("NextPayment")]
        public NextPaymentResponse NextPayment { get; set; }

        [XmlElement("BenefitEntitlement")]
        public BenefitEntitlementResponse BenefitEntitlement { get; set; }

        [XmlElement("ClaimAddress1")]
        public string ClaimAddress1 { get; set; }

        [XmlElement("ClaimAddress2")]
        public string ClaimAddress2 { get; set; }

        [XmlElement("ClaimAddress3")]
        public string ClaimAddress3 { get; set; }

        [XmlElement("ClaimAddress4")]
        public string ClaimAddress4 { get; set; }

        [XmlElement("ClaimPostcode")]
        public string ClaimPostcode { get; set; }
    }

    [XmlRoot("NextPayment")]
    public class NextPaymentResponse
    {
        [XmlElement("Date")]
        public string PaymentDueDate { get; set; }

        [XmlElement("Amount")]
        public string Amount { get; set; }

        [XmlElement("Payee")]
        public string Payee { get; set; }

        [XmlElement("PaidUptoAmount")]
        public string PaidUptoAmount { get; set; }

        [XmlElement("PaymentSchedule")]
        public string PaymentSchedule { get; set; }

        [XmlElement("PaymentMethod")]
        public string PaymentMethod { get; set; }
    }

    [XmlRoot("BenefitEntitlement")]
    public class BenefitEntitlementResponse
    {
        [XmlElement("CTX")]
        public CouncilTaxEntitlementResponse CouncilTax { get; set; }

        [XmlElement("PrivateRent")]
        public HousingBenefitEntitlement PrivateRent { get; set; }

        [XmlElement("CouncilRent")]
        public HousingBenefitEntitlement CouncilRent { get; set; }
    }

    [XmlRoot("CTX")]
    public class CouncilTaxEntitlementResponse
    {
        [XmlAttribute("CtxActRef")]
        public string CouncilTaxReference { get; set; }

        [XmlAttribute("PeriodStart")]
        public string PeriodStart { get; set; }

        [XmlAttribute("PeriodEnd")]
        public string PeriodEnd { get; set; }

        [XmlAttribute("WeeklyBenefit")]
        public string WeeklyBenefit { get; set; }
    }

    [XmlRoot("PrivateRent")]
    public class HousingBenefitEntitlement
    {
        [XmlAttribute("PeriodStart")]
        public string PeriodStart { get; set; }

        [XmlAttribute("PeriodEnd")]
        public string PeriodEnd { get; set; }

        [XmlAttribute("WeeklyBenefit")]
        public string WeeklyBenefit { get; set; }
    }
}
