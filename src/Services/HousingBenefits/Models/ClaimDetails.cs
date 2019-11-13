using System.Linq;

namespace revs_bens_service.Services.HousingBenefits.Models
{
    public class ClaimDetails
    {
        public string ClaimNumber { get; set; }

        public string ClaimAddress => string.Join(",", new[] { ClaimAddress1, ClaimAddress2, ClaimAddress3, ClaimAddress4, ClaimPostcode}.Where(s => !string.IsNullOrEmpty(s)));

        private string _claimStatus { get; set; }

        public string ClaimStatus {
            get => ParseStatusCode(_claimStatus);

            set
            {
                _claimStatus = value;
            }
        }

        public string ClaimAddress1 { get; set; }

        public string ClaimAddress2 { get; set; }

        public string ClaimAddress3 { get; set; }

        public string ClaimAddress4 { get; set; }

        public string ClaimPostcode { get; set; }

        public NextPaymentModel NextPayment { get; set; }

        private string ParseStatusCode(string statusCode)
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
    }

    public class NextPaymentModel
    {
        public string PaymentDueDate { get; set; }
        
        public string Amount { get; set; }

        public string Payee { get; set; }

        public string PaymentMethod { get; set; }

        public string PaymentSchedule { get; set; }

        public int PaymentFrequencyInWeeks => ParsePaymentFrequency(PaymentSchedule);

        private int ParsePaymentFrequency(string paymentSchedule)
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
    }
}