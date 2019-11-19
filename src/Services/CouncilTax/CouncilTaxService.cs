using revs_bens_service.Services.Models;
using revs_bens_service.Utils.Parsers;
using StockportGovUK.AspNetCore.Gateways.CivicaServiceGateway;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Dynamic;
using System.Linq;
using revs_bens_service.Services.CouncilTax.Mappers;
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
        public async Task<CouncilTaxDetailsModel> GetCouncilTaxDetails(string personReference, string accountReference, int year)
        {
            var model = new CouncilTaxDetailsModel();

            var accountResponse = await _gateway.GetAccount(personReference, accountReference);
            model = accountResponse.Parse<CouncilTaxAccountResponse>().ResponseContent.MapAccount(model);

            var transactionsResponse = await _gateway.GetAllTransactionsForYear(personReference, accountReference, year);
            model = transactionsResponse.Parse<TransactionResponse>().ResponseContent.MapTransactions(model);

            var paymentResponse = await _gateway.GetPaymentSchedule(personReference, year.ToString());
            model = paymentResponse.Parse<CouncilTaxPaymentScheduleResponse>().ResponseContent.MapPayments(model);

            var currentPropertyResponse = await _gateway.GetCurrentProperty(personReference);
            model = currentPropertyResponse.Parse<Places>().ResponseContent.MapCurrentProperty(model);

            var isBenefitsResponse = await _gateway.IsBenefitsClaimant(personReference);
            model.HasBenefits = isBenefitsResponse.Parse<bool>().ResponseContent;

            return model;
        }
    }
}
