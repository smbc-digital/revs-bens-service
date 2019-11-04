using revs_bens_service.Services.Models;
using revs_bens_service.Utils.Parsers;
using StockportGovUK.AspNetCore.Gateways.CivicaServiceGateway;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace revs_bens_service.Services
{
    public class CouncilTaxService : ICouncilTaxService
    {
        private readonly ICivicaServiceGateway _gateway;

        public CouncilTaxService(ICivicaServiceGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<IEnumerable<TransactionModel>> GetAllTransactionsForYear(string personReference, string accountReference, int year)
        {
            var response = await _gateway.GetAllTransactionsForYear(personReference, accountReference, year);
            var transactions = response.Parse<TransactionResponse>();

            var transactionResponse = new List<TransactionModel>();

            transactions.ResponseContent.Transaction.ForEach(_ => transactionResponse.Add(new TransactionModel
            {
                Date = DateTime.Parse(_.Date.Text),
                Amount = decimal.Parse(_.Amount.Trim()),
                Method = Convert(_.SubCode),
                Type = decimal.Parse(_.Amount.Trim()) > 0 ? "Credit" : "Debit",
                Description = GetDescription(_.TranType, Convert(_.SubCode), _.PlaceDetail?.PostCode)
            }));

            return transactionResponse;
        }

        private static readonly Dictionary<string, string> Mappings = new Dictionary<string, string>()
        {
            { "BENEFITS", "Benefit" },
            { "COSTS", "Costs" },
            { "CTDRS", "Discretionary Relief" },
            { "INTEREST", "Interest" },
            { "LCTRS", "Council Tax Support" },
            { "PENALTY", "Penalty" },
            { "REFUNDS", "Refund" },
            { "TRANSFER", "Transfer" },
            { "WRITEOFF", "Write Off" },
            { "DISABLED", "Disabled Relief" },
            { "EXEMPTION", "Exemption" },
            { "DISCOUNT", "Discount" },
            { "LEVY", "Premium Charge" },
            { "ANNEXE", "Annexe" },
            { "DISREGARD", "Disregard" },
            { "REDUCTION", "Reduction" },
            { "CHARGE", "Charge" }
        };

        private static readonly HashSet<string> PropertyBasedTranTypes = new HashSet<string>()
        {
            "charge",
            "discount",
            "levy",
            "annexe",
            "disregard",
            "exemption",
            "reduction",
            "disabled"
        };

        private string GetDescription(string transactionType, string method, string postcode)
        {
            if (PropertyBasedTranTypes.Contains(transactionType.ToLower()))
            {
                return $"{Mappings[transactionType.ToUpper()]} - {postcode}";
            }

            if (transactionType.Equals("Payments", StringComparison.CurrentCultureIgnoreCase))
            {
                return $"Payment - {method}";
            }

            return Mappings.ContainsKey(transactionType)
                ? Mappings[transactionType]
                : "Other";
        }

        private string Convert(string paymentMethod)
        {
            if (string.IsNullOrEmpty(paymentMethod))
            {
                return "Unknown";
            }

            switch (paymentMethod.Trim().ToUpper())
            {
                case "CASH":
                case "PP":
                    return "Cash";

                case "CCARD":
                case "DCARD":
                    return "DebitCreditCard";

                case "POCH":
                    return "Cheque";

                case "D/D CASH":
                case "DIRECTDEBIT":
                    return "DirectDebit";

                case "S/O":
                case "POTHER":
                    return "StandingOrder";

                case "DISCOUNT":
                    return "Discount";

                case "SUSPENSE":
                    return "Suspense";

                case "TRANSFER":
                    return "Transfer";

                case "OTHER":
                    return "Other";

                case "BAILIF":
                    return "Bailiff";

                default:
                    return "Unknown";
            }
        }
    }
}
