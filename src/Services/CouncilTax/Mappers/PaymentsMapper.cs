﻿using StockportGovUK.NetStandard.Models.Civica.CouncilTax;
using System;
using System.Linq;
using CouncilTaxDetailsModel = StockportGovUK.NetStandard.Models.RevsAndBens.CouncilTaxDetailsModel;

namespace revs_bens_service.Services.CouncilTax.Mappers
{
    public static class PaymentsMapper
    {
        public static CouncilTaxDetailsModel MapPayments(this CouncilTaxPaymentScheduleResponse paymentResponse, CouncilTaxDetailsModel model)
        {
            model.UpcomingPayments = paymentResponse.InstalmentList.Select(_ => new StockportGovUK.NetStandard.Models.RevsAndBens.InstallmentModel
            {
                Amount = Math.Abs(_.AmountDue),
                Date = DateTime.Parse(_.DateDue),
                IsDirectDebit = _.IsDirectDebit.Equals("Y")
            }).ToList();

            return model;
        }
    }
}
