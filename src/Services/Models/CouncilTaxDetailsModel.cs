using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StockportGovUK.NetStandard.Models.Models.RevsAndBens;

namespace revs_bens_service.Services.Models
{
    public class CouncilTaxDetailsModel
    {
        public string PaymentMethod { get; set; }
        public string IsDirectDebitCustomer { get; set; }
        public string AmountOwing { get; set; }
        public string YearTotals { get; set; }
        public string Reference { get; set; }
        public string PaymentSummary { get; set; }
        public string IsFinalNotice { get; set; }
        public string IsClosed { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string LiabilityPeriodStart { get; set; }
        public string LiabilityPeriodEnd { get; set; }
        public List<InstallmentModel> UpcomingPayments { get; set; }
        public List<TransactionModel> PreviousPayments { get; set; }
        public List<TransactionModel> TransactionHistory { get; set; }
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
