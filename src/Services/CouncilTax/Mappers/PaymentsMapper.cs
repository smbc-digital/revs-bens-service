using System;
using System.Linq;
using revs_bens_service.Services.Models;
using StockportGovUK.NetStandard.Models.Models.Civica.CouncilTax;

namespace revs_bens_service.Services.CouncilTax.Mappers
{
    public static class PaymentsMapper
    {
        public static CouncilTaxDetailsModel MapPayments(this CouncilTaxPaymentScheduleResponse paymentResponse, CouncilTaxDetailsModel model)
        {
            model.UpcomingPayments = paymentResponse.InstalmentList.Select(_ => new InstallmentModel
            {
                Amount = _.AmountDue,
                Date = DateTime.Parse(_.DateDue),
                IsDirectDebit = bool.Parse(_.IsDirectDebit)
            }).ToList();

            return model;
        }
    }
}
