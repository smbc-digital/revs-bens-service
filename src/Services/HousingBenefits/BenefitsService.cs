﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using revs_bens_service.Extensions;
using StockportGovUK.AspNetCore.Gateways.CivicaServiceGateway;
using StockportGovUK.NetStandard.Models.Models.RevsAndBens;

namespace revs_bens_service.Services.HousingBenefits
{
    public class BenefitsService
    {
        private readonly ICivicaServiceGateway _gateway;
        public BenefitsService(ICivicaServiceGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<List<Benefits>> GetBenefitDetails(string personReference)
        {
            var response = await _gateway.GetBenefitSummary(personReference);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"GetBenefitsSummary({personReference}) failed with status code: {response.StatusCode}");
                return response.Parse<Benefits>().ResponseContent;
            }

            var claimDetails = PopulateDetailsForAllClaims(response);
            return claimDetails;
        }

        private List<Benefits> PopulateDetailsForAllClaims(ClaimsSummaryResponse claimsSummaryResponse)
        {
            var claimDetails = new List<Benefits>();
            foreach (var claimMetadata in claimsSummaryResponse.ClaimsList.ClaimSummary)
            {
                var claimDetailResponse = _gateway.GetBenefitDetails(claimMetadata.ClaimPlaceRef, claimMetadata.ClaimNumber);
                var entity = claimDetailResponse.ToEntity(claimMetadata.ClaimStatus);
                RetrieveAndAppendHistoryForAllClaims(entity);
                entity.NextPayment.PaymentStatus =
                    IsUnexpectedPaymentAmount(
                        entity.NextPayment.NextPaymentAmount,
                        entity.CurrentEntitlement.WeeklyHousingBenefitEntitlement,
                        entity.NextPayment.PaymentFrequencyInWeeks);

                entity.BenefitsCombination = GetBenefitsCombination(entity.CurrentEntitlement, entity.ClaimStatus);
                claimDetails.Add(entity);
            }

            return claimDetails;
        }

        private static PaymentStatusEnum IsUnexpectedPaymentAmount(string strNextPaymentAmount, string strCurrentWeeklyHousingBenefitEntitlement, int paymentFrequencyInWeeks)
        {
            if (paymentFrequencyInWeeks == 0)
            {
                return PaymentStatusEnum.Expected;
            }

            decimal nextPaymentAmount;
            decimal.TryParse(strNextPaymentAmount, out nextPaymentAmount);

            decimal expectedPayment;
            decimal.TryParse(strCurrentWeeklyHousingBenefitEntitlement, out expectedPayment);

            var actualPayment = nextPaymentAmount / paymentFrequencyInWeeks;

            if (actualPayment > expectedPayment)
            {
                return PaymentStatusEnum.Increased;
            }

            return actualPayment < expectedPayment
                ? PaymentStatusEnum.Reduced
                : PaymentStatusEnum.Expected;
        }

        private static BenefitsComboEnum GetBenefitsCombination(CurrentEntitlement response, string claimStatus)
        {
            var benefitsCombo = string.Empty;

            benefitsCombo += response.WeeklyHousingBenefitEntitlement == "0.00" && claimStatus == "Current" ? string.Empty : "[HB]";
            benefitsCombo += response.WeeklyCtaxBenefitEntitlement == "0.00" ? string.Empty : "[CTS]";

            switch (benefitsCombo)
            {
                case "[HB]":
                    return BenefitsComboEnum.HousingBenefitOnly;
                case "[CTS]":
                    return BenefitsComboEnum.CouncilTaxSupportOnly;
                case "[HB][CTS]":
                    return BenefitsComboEnum.AllBenefits;
                default:
                    return BenefitsComboEnum.HousingBenefitOnly;
            }
        }

        private void RetrieveAndAppendHistoryForAllClaims(Benefits claimDetails)
        {
            var parallelQuery = new ConcurrentDictionary<string, object>();

            var tasks = new List<Task>
            {
                Task.Run(() =>
                {
                    var history = _gateway.GetPaymentDetails("prp");
                    parallelQuery.TryAdd("HousingBenefitPaymentHistory", history.ToEntity<HousingBenefitsPaymentDetail>());
                }),
                Task.Run(() =>
                {
                    var history = _gateway.GetPaymentDetails("ctp");
                    parallelQuery.TryAdd("CtaxBenefitPaymentHistory", history.ToEntity<CtaxBenefitsPaymentDetail>());
                }),
                Task.Run(() =>
                {
                    var documents = _gateway.GetDocumentList();
                    parallelQuery.TryAdd("DocumentList", documents.ToEntity());
                })
            };

            Task.WaitAll(tasks.ToArray());

            claimDetails.HousingBenefitHistory = new HousingBenefitHistory
            {
                PaymentHistory = parallelQuery["HousingBenefitPaymentHistory"] as List<HousingBenefitsPaymentDetail>
            };

            var councilTaxSupportPayments = parallelQuery["CtaxBenefitPaymentHistory"] as List<CtaxBenefitsPaymentDetail>;
            claimDetails.CouncilTaxBenefitHistory = new CouncilTaxBenefitHistory
            {
                CurrentSummary = BuildCouncilTaxSupportSummary(councilTaxSupportPayments),
                PaymentHistory = councilTaxSupportPayments,
            };

            claimDetails.Documents = parallelQuery["DocumentList"] as List<Document>;
        }

        private CtaxBenefitsSummary BuildCouncilTaxSupportSummary(IEnumerable<CtaxBenefitsPaymentDetail> councilTaxSupportPayments)
        {
            var currentTaxYear = DateTime.Now.ToFinancialYear();

            var currentYearPayments =
                councilTaxSupportPayments
                    .Where(_ => _.PeriodStart.ToDate().ToFinancialYear() == currentTaxYear)
                    .ToList();

            var accountReference = currentYearPayments.FirstOrDefault() != null ? currentYearPayments.First().CouncilTaxReference : "N/a";

            var ctaxBenefitsSummaryEntity = new CtaxBenefitsSummary
            {
                TaxYear = currentTaxYear,
                AccountReference = accountReference,
            };

            try
            {
                var ctaxSummaryResponse = _gateway.GetCouncilTaxSummaryDetailForYear(accountReference, currentTaxYear);

                if (ctaxSummaryResponse.FinancialDetails == null || ctaxSummaryResponse.FinancialDetails.RecYrTotals == null)
                {
                    return ctaxBenefitsSummaryEntity;
                }

                var yearTotal = ctaxSummaryResponse.FinancialDetails.RecYrTotals;
                ctaxBenefitsSummaryEntity.TotalBill = yearTotal.TotalCharge == null ? "0.00" : yearTotal.TotalCharge.Trim();
                ctaxBenefitsSummaryEntity.TotalPayments = yearTotal.TotalPayments == null ? "0.00" : yearTotal.TotalPayments.Trim();
                ctaxBenefitsSummaryEntity.TotalBenefits = yearTotal.TotalBenefits == null ? "0.00" : yearTotal.TotalBenefits.Trim();
                ctaxBenefitsSummaryEntity.BalanceOutstanding = yearTotal.BalanceOutstanding == null ? "0.00" : yearTotal.BalanceOutstanding.Trim();

                ctaxBenefitsSummaryEntity.BenefitsBreakdown =
                    currentYearPayments.Select(
                        _ =>
                            new CouncilTaxBenefitsPayment
                            {
                                Amount = _.PayAmount,
                                PaymentDate = _.DatePaid,
                                PaymentPeriod = string.Concat(_.PeriodStart, " to ", _.PeriodEnd)
                            }).ToList();

                return ctaxBenefitsSummaryEntity;
            }
            catch (Exception ex)
            {
                _logger.WriteError(string.Format("Failed to Build Council Tax Support Summary: {0}", ex.Message));
                return ctaxBenefitsSummaryEntity;
            }
        }
    }
}
