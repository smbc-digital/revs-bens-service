

using System;

namespace revs_bens_service.Services.Models
{
    public class TransactionModel
    {
        public DateTime Date { get; set; }

        public decimal Amount { get; set; }

        public string Method { get; set; }

        public string Description { get; set; }

        public string Type { get; set; }
    }
}
