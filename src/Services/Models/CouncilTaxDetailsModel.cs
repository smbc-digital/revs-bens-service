using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StockportGovUK.NetStandard.Models.Models.Civica.CouncilTax;

namespace revs_bens_service.Services.Models
{
    public class CouncilTaxDetailsModel
    {
        public string PaymentMethod { get; set; }
        public bool IsDirectDebitCustomer { get; set; }
        public decimal AmountOwing { get; set; }
        public IEnumerable<YearTotalResponse> YearTotals { get; set; } // object
        public string Reference { get; set; }
        public PaymentSummaryResponse PaymentSummary { get; set; } // object
        public bool? IsFinalNotice { get; set; }
        public bool? IsClosed { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string LiabilityPeriodStart { get; set; }
        public string LiabilityPeriodEnd { get; set; }
        public List<InstallmentModel> UpcomingPayments { get; set; }
        public List<TransactionModelExtension> PreviousPayments { get; set; }
        public List<TransactionModelExtension> TransactionHistory { get; set; }
        public bool HasBenefits { get; set; }
    }

    public class InstallmentModel
    {
        [DataType("Date")]
        public DateTime Date { get; set; }

        [DataType("Currency")]
        public decimal Amount { get; set; }

        public bool IsDirectDebit { get; set; }

        public string Description => IsDirectDebit ? "Direct Debit" : "Non Direct Debit";
    }
}
