namespace revs_bens_service.Services.Models
{
    public class CouncilTaxSummaryResponse
    {
        public FinancialDetailsResponseItem FinancialDetails { get; set; }
    }

    public class FinancialDetailsResponseItem
    {
        public RecYrTotalsResponseItem RecYrTotals { get; set; }
    }

    public class RecYrTotalsResponseItem
    {
        public string TotalCharge { get; set; }

        public string TotalPayments { get; set; }

        public string TotalBenefits { get; set; }

        public string BalanceOutstanding { get; set; }
    }

    public class TransactionResponseItem
    {
        public string Amount { get; set; }

        public string TranType { get; set; }
    }
}