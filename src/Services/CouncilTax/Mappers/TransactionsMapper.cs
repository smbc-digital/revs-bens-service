using System;
using System.Collections.Generic;
using System.Linq;
using revs_bens_service.Services.Models;
using StockportGovUK.NetStandard.Models.Models.Civica.CouncilTax;

namespace revs_bens_service.Services.CouncilTax.Mappers
{
    public static class TransactionsMapper
    {
        public static CouncilTaxDetailsModel MapTransactions(this TransactionResponse transactionResponse, CouncilTaxDetailsModel model)
        {
            var parsedTransactions = transactionResponse.Transaction.Select(transaction => new TransactionModelExtension
            {
                Date = DateTime.Parse(transaction.Date.Text),
                Amount = Math.Abs(transaction.DAmount),
                Method = Convert(transaction.SubCode),
                Type = IsCredit(transaction.DAmount, transaction.TranType) ? "Credit" : "Debit",
                Description = GetDescription(transaction.TranType, Convert(transaction.SubCode), transaction.PlaceDetail?.PostCode)
            }).Distinct().ToArray();

            model.TransactionHistory = parsedTransactions.Where(t => t.Type != "Charge" && t.Type != "REFUNDS" && t.Type != "PAYMENTS").ToList();
            model.PreviousPayments = parsedTransactions.Where(t => t.Type == "PAYMENTS" || t.Type == "REFUNDS").ToList();

            return model;
        }

        private static bool IsCredit(decimal amount, string type)
        {
            if (Types.Contains(type.ToLower()))
            {
                amount *= -1;
            }

            return amount > 0;
        }

        private static string GetDescription(string transactionType, string method, string postcode)
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

        private static string Convert(string paymentMethod)
        {
            if (!string.IsNullOrEmpty(paymentMethod) && PaymentMethod.ContainsKey(paymentMethod))
            {
                return PaymentMethod[paymentMethod];
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

        private static readonly HashSet<string> Types = new HashSet<string> {
            "discount",
            "exemption",
            "disabled",
            "costs",
            "disregard"
        };

        private static readonly Dictionary<string, string> PaymentMethod = new Dictionary<string, string>() {

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
