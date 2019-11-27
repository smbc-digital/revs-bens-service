using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using StockportGovUK.NetStandard.Models.Models.Civica.CouncilTax;
using StockportGovUK.NetStandard.Models.RevsAndBens;

namespace revs_bens_service.Services.CouncilTax.Mappers
{
    public static class DocumentMapper
    {
        public static CouncilTaxDetailsModel DocumentsMapper(this List<CouncilTaxDocumentReference> documentResponse, CouncilTaxDetailsModel model, int taxYear)
        {
            model.Documents = documentResponse.Select(_ => new CouncilTaxDocument
            {
                AccountReference = _.AccountReference,
                DateCreated = _.DateCreated,
                DocumentId = _.DocumentId,
                DocumentName = _.DocumentName,
                DocumentType = _.DocumentType,
                Downloaded = _.Downloaded
            }).ToList();
            
            if(model.Documents != null )
            {
                var documentsForCurrentTaxYear = model.Documents.Where(_ =>
                {
                    DateTime.TryParse(_.DateCreated, new CultureInfo("en-GB"), DateTimeStyles.AssumeLocal, out var dateCreated);

                    var year = dateCreated.Month < 3 ? dateCreated.Year - 1 : dateCreated.Year;

                    return year == taxYear;
                });

                var latestDocument = documentsForCurrentTaxYear.OrderByDescending(_ => _.DateCreated).FirstOrDefault();

                model.LatestBillId = latestDocument?.DocumentId ?? string.Empty;
            }
            else
            {
                model.LatestBillId = string.Empty;
            }

            return model;
        }
    }
}
