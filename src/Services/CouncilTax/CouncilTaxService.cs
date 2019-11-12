using revs_bens_service.Services.Models;
using revs_bens_service.Utils.Parsers;
using StockportGovUK.AspNetCore.Gateways.CivicaServiceGateway;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace revs_bens_service.Services.CouncilTax
{
    public class CouncilTaxService : ICouncilTaxService
    {
        private readonly ICivicaServiceGateway _gateway;

        public CouncilTaxService(ICivicaServiceGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<IEnumerable<TransactionModelExtension>> GetAllTransactionsForYear(string personReference, string accountReference, int year)
        {
            var response = await _gateway.GetAllTransactionsForYear(personReference, accountReference, year);
            var transactions = response.Parse<TransactionResponse>().ResponseContent.Transaction;

            return transactions.Select(transaction => new TransactionModelExtension
            {
                Date = DateTime.Parse(transaction.Date.Text),
                Amount = Math.Abs(transaction.DAmount),
                Method = Convert(transaction.SubCode),
                Type = IsCredit(transaction.DAmount, transaction.TranType) ? "Credit" : "Debit",
                Description = GetDescription(transaction.TranType, Convert(transaction.SubCode), transaction.PlaceDetail?.PostCode)
            }).Distinct();
        }

        private bool IsCredit(decimal amount, string type)
        {
            if (_types.Contains(type.ToLower()))
            {
                amount *= -1;
            }

            return amount > 0;
        }

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
            if (!string.IsNullOrEmpty(paymentMethod) && _paymentMethod.ContainsKey(paymentMethod))
            {
                return _paymentMethod[paymentMethod];
            }
            return "Unknown";
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

        private static readonly HashSet<string> _types = new HashSet<string> {
            "discount",
            "exemption",
            "disabled",
            "costs",
            "disregard"
        };

        private static readonly Dictionary<string, string> _paymentMethod = new Dictionary<string, string>() {

            {"CASH", "Cash" },
            {"PP", "Cash"},
            {"CCARD","DebitCreditCard" },
            {"DCARD","DebitCreditCard" },
            {"POCH","Cheque" },
            {"D/D CASH","DirectDebit" },
            {"DIRECTDEBIT","DirectDebit" },
            {"S/O","StandingOrder" },
            {"POTHER","StandingOrder" },
            {"DISCOUNT","Discount" },
            {"SUSPENSE","Suspense" },
            {"TRANSFER","Transfer" },
            {"OTHER","Other" },
            {"BAILIF","Bailif" },
        };
    }
}
