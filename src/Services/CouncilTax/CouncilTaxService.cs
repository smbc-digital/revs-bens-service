using revs_bens_service.Services.Models;
using revs_bens_service.Utils.Parsers;
using StockportGovUK.AspNetCore.Gateways.CivicaServiceGateway;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Dynamic;
using System.Linq;
using StockportGovUK.NetStandard.Models.Models.Civica.CouncilTax;

namespace revs_bens_service.Services.CouncilTax
{
    public class CouncilTaxService : ICouncilTaxService
    {
        private readonly ICivicaServiceGateway _gateway;

        public CouncilTaxService(ICivicaServiceGateway gateway)
        {
            _gateway = gateway;
        }

        // TODO:: convert GetPaymentSchedule to take year as an int and not a string #consistency 
        /**
         *  return: dynamic object with structure
         *          {
         *               Transactions: 
         *               Accounts: 
         *               Payments: 
         *               Documents: 
         *               HasBenefits: 
         *          }
         */
        public async Task<CouncilTaxDetailsModel> GetCouncilTaxDetails(string personReference, string accountReference, int year)
        {
            dynamic model = new ExpandoObject();

            await _gateway.GetSessionId(personReference);

            await Task.WhenAll(
                Task.Run(async () =>
                {
                    var response = await _gateway.GetAllTransactionsForYear(personReference, accountReference, year);
                    var transactions = response.Parse<TransactionResponse>().ResponseContent.Transaction;
                    ParseTransactions(ref model, transactions);
                }),
                Task.Run(async () =>
                {
                    var response = await _gateway.GetAccount(personReference, accountReference);
                    var account = response.Parse<CouncilTaxAccountResponse>().ResponseContent;
                    ParseAccount(ref model, account);
                }),
                Task.Run(async () =>
                {
                    var response = await _gateway.GetAccounts(personReference);
                    model.Accounts = response.Parse<IEnumerable<CtaxActDetails>>().ResponseContent;
                }),
                Task.Run(async () =>
                {
                    var response = await _gateway.GetPaymentSchedule(personReference, year.ToString());
                    var payments = response.Parse<CouncilTaxPaymentScheduleResponse>().ResponseContent;
                    ParsePayments(ref model, payments.InstalmentList);
                }),
                Task.Run(async () =>
                {
                    var response = await _gateway.GetCurrentProperty(personReference);
                    model.Property = response.Parse<Places>().ResponseContent;
                }),
                Task.Run(async () =>
                {
                    var response = await _gateway.GetDocumentsWithAccountReference(personReference, accountReference);
                    model.Documents = response.Parse<List<CouncilTaxDocumentReference>>().ResponseContent;
                }),
                Task.Run(async () =>
                {
                    var response = await _gateway.IsBenefitsClaimant(personReference);
                    model.HasBenefits = response.Parse<bool>().ResponseContent;
                })
            );

            return GenerateCouncilTaxDetailsModel(model);
        }

        #region Transaction Based Methods
        private void ParsePayments(ref dynamic model, List<Instalment> instalments)
        {
            model.UpcomingPayments = instalments.Select(_ => new InstallmentModel
            {
                Amount = _.AmountDue,
                Date = DateTime.Parse(_.DateDue),
                IsDirectDebit = bool.Parse(_.IsDirectDebit)
            }).ToList();
        }

        private void ParseTransactions(ref dynamic model, IEnumerable<Transaction> transactions)
        {
            var parsedTransactions = transactions.Select(transaction => new TransactionModelExtension
            {
                Date = DateTime.Parse(transaction.Date.Text),
                Amount = Math.Abs(transaction.DAmount),
                Method = Convert(transaction.SubCode),
                Type = IsCredit(transaction.DAmount, transaction.TranType) ? "Credit" : "Debit",
                Description = GetDescription(transaction.TranType, Convert(transaction.SubCode), transaction.PlaceDetail?.PostCode)
            }).Distinct().ToArray();

            model.TransactionHistory = parsedTransactions.Where(t => t.Type != "Charge" && t.Type != "REFUNDS" && t.Type != "PAYMENTS").ToList();
            model.PaymentTransactions = parsedTransactions.Where(t => t.Type == "PAYMENTS" || t.Type == "REFUNDS").ToList();
        }

        private bool IsCredit(decimal amount, string type)
        {
            if (Types.Contains(type.ToLower()))
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
        #endregion

        #region Account Based Methods
        private static readonly HashSet<string> ValidAccountStages = new HashSet<string> { "BIL", "RM1", "RM2" };

        private void ParseAccount(ref dynamic model, CouncilTaxAccountResponse account)
        {
            model.PaymentMethod = account.AccountDetails.ActPayGrp.PaymentMethod.Contains("DD") ? "Direct Debit" : string.Empty;
            model.IsDirectDebitCustomer = account.AccountDetails.ActPayGrp.IsDirectDebit();
            model.AmountOwing = account.CouncilTaxAccountBalance;
            model.YearTotals = account.FinancialDetails.YearTotals;
            model.Reference = account.CouncilTaxAccountReference;
            model.PaymentSummary = account.FinancialDetails
                                       .YearTotals?
                                       .FirstOrDefault()?
                                       .YearSummaries?
                                       .FirstOrDefault()?
                                       .NextPayment ?? new PaymentSummaryResponse();
            model.AccountName = account.AccountDetails.BankDetails?.AccountName;
            model.AccountNumber = account.AccountDetails.BankDetails?.AccountNumber;
            model.IsFinalNotice = account.FinancialDetails.YearTotals?.FirstOrDefault()?.YearSummaries.Any(x => !ValidAccountStages.Contains(x.Stage.StageCode));
            model.IsClosed = account.CtxActClosed == "TRUE";
        }
        #endregion

        #region Parsing To CouncilTaxDetailsModel

        private CouncilTaxDetailsModel GenerateCouncilTaxDetailsModel(dynamic model)
        {

            return new CouncilTaxDetailsModel
            {
                PaymentMethod = model.PaymentMethod,
                IsDirectDebitCustomer = model.IsDirectDebitCustomer,
                AmountOwing = model.AmountOwing,
                YearTotals = model.YearTotals,
                Reference = model.Reference,
                PaymentSummary = model.PaymentSummary,
                IsFinalNotice = model.IsFinalNotice,
                IsClosed = model.IsClosed,
                AccountNumber = model.AccountNumber,
                AccountName = model.AccountName,
                LiabilityPeriodStart = model.Property.ChargeDetails?.Dates?.Start,
                LiabilityPeriodEnd = model.Property.ChargeDetails?.Dates?.End,
                //UpcomingPayments = model.UpcomingPayments,
                TransactionHistory = model.TransactionHistory,
                PreviousPayments = model.PaymentTransactions
            };
        }

        #endregion
    }
}
