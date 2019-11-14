using System.Collections.Generic;
using System.Xml.Serialization;

namespace revs_bens_service.Services.Models
{
    public class ClaimsSummaryResponse
    {
        public ClaimList Claims { get; set; }
    }

    public class ClaimList
    {
        public List<ClaimSummary> Summary { get; set; }
    }

    public class ClaimSummary
    {
        public string Number { get; set; }

        public string PlaceRef { get; set; }

        public string Status { get; set; }

        public string PersonType { get; set; }

        public string Address { get; set; }
    }
}
