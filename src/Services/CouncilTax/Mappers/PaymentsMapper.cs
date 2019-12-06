using StockportGovUK.NetStandard.Models.Civica.CouncilTax;
using StockportGovUK.NetStandard.Models.RevsAndBens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace revs_bens_service.Services.CouncilTax.Mappers
{
    public static class PaymentsMapper
    {
        public static CouncilTaxDetailsModel MapPayments(this List<StockportGovUK.NetStandard.Models.Civica.CouncilTax.Instalment> paymentResponse, CouncilTaxDetailsModel model)
        {
            model.UpcomingPayments = paymentResponse.Select(_ => new StockportGovUK.NetStandard.Models.RevsAndBens.InstallmentModel
            {
                Amount = Math.Abs(_.AmountDue),
                Date = DateTime.Parse(_.DateDue),
                IsDirectDebit = _.IsDirectDebit.Equals("Y")
            }).ToList();

            return model;
        }
    }
}
