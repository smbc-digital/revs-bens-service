using System.Collections.Generic;
using System.Linq;
using revs_bens_service.Services.Models;
using StockportGovUK.NetStandard.Models.Models.Civica.CouncilTax;
using StockportGovUK.NetStandard.Models.RevsAndBens;

namespace revs_bens_service.Services.CouncilTax.Mappers
{
    public static class AccountMapper
    {
        private static readonly HashSet<string> ValidAccountStages = new HashSet<string> { "BIL", "RM1", "RM2" };

        public static CouncilTaxDetailsModel MapAccount(this CouncilTaxAccountResponse accountResponse, CouncilTaxDetailsModel model, int taxYear)
        {
            model.PaymentMethod = accountResponse.AccountDetails.ActPayGrp.PaymentMethod.Contains("DD") ? "Direct Debit" : string.Empty;
            model.IsDirectDebitCustomer = accountResponse.AccountDetails.ActPayGrp.IsDirectDebit();
            model.AmountOwing = accountResponse.CouncilTaxAccountBalance;
            model.YearTotals = accountResponse.FinancialDetails.YearTotals;
            model.Reference = accountResponse.CouncilTaxAccountReference;
            model.PaymentSummary = accountResponse.FinancialDetails
                                       .YearTotals?
                                       .FirstOrDefault()?
                                       .YearSummaries?
                                       .FirstOrDefault()?
                                       .NextPayment ?? new PaymentSummaryResponse();
            model.AccountName = accountResponse.AccountDetails.BankDetails?.AccountName;
            model.AccountNumber = accountResponse.AccountDetails.BankDetails?.AccountNumber;
            model.IsFinalNotice = accountResponse.FinancialDetails.YearTotals?.FirstOrDefault()?.YearSummaries.Any(x => !ValidAccountStages.Contains(x.Stage.StageCode));
            model.IsClosed = accountResponse.CtxActClosed == "TRUE";
            model.HasSpar = accountResponse.FinancialDetails.YearTotals?.FirstOrDefault(_ => _.TaxYear == taxYear)?.SparNo != 0;

            return model;
        }

        public static CouncilTaxDetailsModel MapAccounts(this List<CtaxActDetails> accountsResponse, CouncilTaxDetailsModel model)
        {
            model.Accounts = accountsResponse.Select(_ => new CouncilTaxAccountDetails
            {
                Reference = _.CtaxActRef,
                Address = _.CtaxActAddress,
                Balance = _.CtaxBalance,
                Status = _.AccountStatus
            }).ToList();

            return model;
        }
    }
}
