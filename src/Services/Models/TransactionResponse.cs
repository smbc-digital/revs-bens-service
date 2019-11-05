using System.Collections.Generic;

namespace revs_bens_service.Services.Models
{
    public class TransactionResponse
    {
        public List<Transaction> Transaction { get; set; }
    }

    public class Date
    {
        public string NumericEquiv { get; set; }
        public string Text { get; set; }
    }

    public class Period
    {
        public string End { get; set; }
        public string Start { get; set; }
    }

    public class PlaceDetail
    {
        public string Valid { get; set; }
        public string PlaceRef { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Address4 { get; set; }
        public string Address5 { get; set; }
        public string PostCode { get; set; }
    }

    public class Transaction
    {
        public Date Date { get; set; }
        public string Amount { get; set; }
        public decimal DAmount => decimal.Parse(Amount.Trim());
        public string Claim { get; set; }
        public Period Period { get; set; }
        public PlaceDetail PlaceDetail { get; set; }
        public string Number { get; set; }
        public string TranType { get; set; }
        public string Year { get; set; }
        public string ChargeType { get; set; }
        public string SubCode { get; set; }
    }
}
