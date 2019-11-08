using System.Collections.Generic;

namespace revs_bens_service.Services.Models
{
    public class TransactionResponse
    {
        public List<Transaction> Transaction { get; set; }
    }

    public class Date
    {
        public string Text { get; set; }
    }

    public class PlaceDetail
    {
        public string PostCode { get; set; }
    }

    public class Transaction
    {
        public Date Date { get; set; }
        public string Amount { get; set; }
        public decimal DAmount => decimal.Parse(Amount.Trim());
        public PlaceDetail PlaceDetail { get; set; }
        public string TranType { get; set; }
        public string SubCode { get; set; }
    }
}
