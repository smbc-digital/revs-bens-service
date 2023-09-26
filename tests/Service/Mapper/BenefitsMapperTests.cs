using revs_bens_service.Services.Benefits.Mappers;
using StockportGovUK.NetStandard.Gateways.Enums;
using StockportGovUK.NetStandard.Gateways.Models.RevsAndBens;
using Xunit;

namespace revs_bens_service_tests.Service.Mapper
{
    public class BenefitsMapperTests
    {
        private BenefitsClaim _benefitsClaim = new BenefitsClaim
        {
            PersonName = new PersonName
            {
                Forenames = "Test",
                Surname = "Test"
            },
            Number = "123",
            Status = "1",
            NextPayment = new CivicaNextPayment
            {
                Amount = "200.00",
                Method = "Direct debit",
                PaidUpToAmount = "50.00",
                Payee = "test",
                PaymentDueDate = "12/12/2019",
                Schedule = "fortnightly"
            },
            BenefitEntitlement = new BenefitEntitlement
            {
                PrivateRent = new HousingBenefitEntitlement
                {
                    WeeklyBenefit = "100.00"
                },
                CouncilTax = new CouncilTaxEntitlement
                {
                    WeeklyBenefit = "25.00"
                }
            },
            Address1 = "Address1",
            Address2 = "Address2",
            Address3 = "Address3",
            Address4 = "Address4",
            Postcode = "Postcode"
        };

        private List<CouncilTaxDocument> _documents = new List<CouncilTaxDocument>
        {
            new CouncilTaxDocument
            {
                AccountReference = "123",
                Downloaded = "true",
                DocumentId = "123",
                DateCreated = "12/12/2019",
                DocumentName = "Doc1",
                DocumentType = "Type1"
            },
            new CouncilTaxDocument
            {
                AccountReference = "456",
                Downloaded = "false",
                DocumentId = "456",
                DateCreated = "19/12/2019",
                DocumentName = "Doc2",
                DocumentType = "Type2"
            }
        };

        private List<PaymentDetail> _payments = new List<PaymentDetail>
        {
            new PaymentDetail
            {
                Payee = "Payee1",
                CouncilTaxReference = "123",
                DatePaid = "12/12/2019",
                OnAct = "OnAct1",
                PayAmount = "100.00",
                PayType = "Type1",
                PeriodEnd = "30/03/2020",
                PeriodStart = "01/04/2019"
            },
            new PaymentDetail
            {
                Payee = "Payee2",
                CouncilTaxReference = "456",
                DatePaid = "19/12/2019",
                OnAct = "OnAct2",
                PayAmount = "200.00",
                PayType = "Type2",
                PeriodEnd = "30/03/2019",
                PeriodStart = "01/04/2018"
            }
        };

        [Fact]
        public void MapToClaimDetails_ShouldMap()
        {
            // Arrange
            var expectedResult = new ClaimDetails
            {
                PersonName = "Test Test",
                Number = "123",
                Status = "Current",
                NextPayment = new ClaimNextPayment
                {
                    Amount = "200.00",
                    Method = "Direct debit",
                    PaidUpToAmount = "50.00",
                    Payee = "test",
                    DueDate = "12/12/2019",
                    Schedule = "fortnightly",
                    Status = EPaymentStatus.Expected
                },
                Address = "Address1, Address2, Address3, Address4, Postcode",
                BenefitsCombination = BenefitsCombinationEnum.AllBenefits,
                CurrentEntitlement = new CurrentEntitlement
                {
                    WeeklyCtaxBenefitEntitlement = "25.00",
                    WeeklyHousingBenefitEntitlement = "100.00"
                }
            };

            // Act
            var result = _benefitsClaim.MapToClaimDetails();

            // Assert
            Assert.Equal(expectedResult.PersonName, result.PersonName);
            Assert.Equal(expectedResult.Number, result.Number);
            Assert.Equal(expectedResult.Status, result.Status);
            Assert.Equal(expectedResult.NextPayment.Amount, result.NextPayment.Amount);
            Assert.Equal(expectedResult.NextPayment.Method, result.NextPayment.Method);
            Assert.Equal(expectedResult.NextPayment.PaidUpToAmount, result.NextPayment.PaidUpToAmount);
            Assert.Equal(expectedResult.NextPayment.Payee, result.NextPayment.Payee);
            Assert.Equal(expectedResult.NextPayment.DueDate, result.NextPayment.DueDate);
            Assert.Equal(expectedResult.NextPayment.Schedule, result.NextPayment.Schedule);
            Assert.Equal(expectedResult.NextPayment.Status, result.NextPayment.Status);
            Assert.Equal(expectedResult.Address, result.Address);
            Assert.Equal(expectedResult.BenefitsCombination, result.BenefitsCombination);
            Assert.Equal(expectedResult.CurrentEntitlement.WeeklyHousingBenefitEntitlement, result.CurrentEntitlement.WeeklyHousingBenefitEntitlement);
            Assert.Equal(expectedResult.CurrentEntitlement.WeeklyCtaxBenefitEntitlement, result.CurrentEntitlement.WeeklyCtaxBenefitEntitlement);
        }

        [Fact]
        public void MapToDocuments_ShouldMap()
        {
            // Arrange & Act
            var result = _documents.MapToDocuments();

            // Assert
            Assert.Equal(_documents[0].AccountReference, result[0].AccountReference);
            Assert.Equal(_documents[0].DateCreated, result[0].DateCreated);
            Assert.Equal(_documents[0].DocumentId, result[0].Id);
            Assert.Equal(_documents[0].DocumentName, result[0].Name);
            Assert.Equal(_documents[0].DocumentType, result[0].Type);
            Assert.Equal(_documents[0].Downloaded, result[0].Downloaded);

            Assert.Equal(_documents[1].AccountReference, result[1].AccountReference);
            Assert.Equal(_documents[1].DateCreated, result[1].DateCreated);
            Assert.Equal(_documents[1].DocumentId, result[1].Id);
            Assert.Equal(_documents[1].DocumentName, result[1].Name);
            Assert.Equal(_documents[1].DocumentType, result[1].Type);
            Assert.Equal(_documents[1].Downloaded, result[1].Downloaded);
        }

        [Fact]
        public void MapToPayments_ShouldMap()
        {
            // Arrange & Act
            var result = _payments.MapToPayments();

            // Assert
            Assert.Equal(_payments[0].Payee, result[0].Payee);
            Assert.Equal(_payments[0].CouncilTaxReference, result[0].CouncilTaxReference);
            Assert.Equal(_payments[0].DatePaid, result[0].DatePaid);
            Assert.Equal(_payments[0].OnAct, result[0].OnAct);
            Assert.Equal(_payments[0].PayAmount, result[0].Amount);
            Assert.Equal(_payments[0].PayType, result[0].Type);
            Assert.Equal(_payments[0].PeriodEnd, result[0].PeriodEnd);
            Assert.Equal(_payments[0].PeriodStart, result[0].PeriodStart);
            Assert.Equal(_payments[1].Payee, result[1].Payee);
            Assert.Equal(_payments[1].CouncilTaxReference, result[1].CouncilTaxReference);
            Assert.Equal(_payments[1].DatePaid, result[1].DatePaid);
            Assert.Equal(_payments[1].OnAct, result[1].OnAct);
            Assert.Equal(_payments[1].PayAmount, result[1].Amount);
            Assert.Equal(_payments[1].PayType, result[1].Type);
            Assert.Equal(_payments[1].PeriodEnd, result[1].PeriodEnd);
            Assert.Equal(_payments[1].PeriodStart, result[1].PeriodStart);
        }
    }
}
