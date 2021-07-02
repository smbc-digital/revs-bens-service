using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using revs_bens_service.Services.Benefits;
using revs_bens_service.Services.CouncilTax;
using StockportGovUK.AspNetCore.Attributes.TokenAuthentication;

namespace revs_bens_service.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[Controller]")]
    [ApiController]
    [TokenAuthentication]
    public class PeopleController : ControllerBase
    {
        private readonly IBenefitsService _benefitsService;
        private readonly ICouncilTaxService _councilTaxService;

        public PeopleController(
            IBenefitsService benefitsService, 
            ICouncilTaxService councilTaxService)
        {
            _benefitsService = benefitsService;
            _councilTaxService = councilTaxService;
        }

        [HttpGet]
        [Route("{personReference}/is-benefits-claimant")]
        public async Task<IActionResult> IsBenefitsClaimant([FromRoute][Required]string personReference) =>
            Ok(await _benefitsService.IsBenefitsClaimant(personReference));

        [HttpGet]
        [Route("{personReference}/benefits")]
        public async Task<IActionResult> GetBenefits([FromRoute][Required]string personReference) =>
            Ok(await _benefitsService.GetBenefits(personReference));

        [HttpGet]
        [Route("{personReference}/council-tax")]
        public async Task<IActionResult> GetBaseCouncilTaxAccount([FromRoute][Required]string personReference) =>
            Ok(await _councilTaxService.GetBaseCouncilTaxAccount(personReference));

        [HttpGet]
        [Route("{personReference}/council-tax/{accountReference}/{year}")]
        public async Task<IActionResult> GetCouncilTaxDetails(
            [FromRoute][Required]string personReference,
            [FromRoute][Required]string accountReference,
            [FromRoute][Required]int year) =>
                Ok(await _councilTaxService.GetCouncilTaxDetails(personReference, accountReference.Trim(), year));

        [HttpGet]
        [Route("{personReference}/council-tax/{accountReference}/documents/{documentId}")]
        public async Task<IActionResult> GetDocumentForAccount(
            [FromRoute][Required]string personReference,
            [FromRoute][Required]string accountReference,
            [FromRoute][Required]string documentId)
        {
            var document = await _councilTaxService.GetDocumentForAccount(personReference, accountReference.Trim(), documentId);

            if (document == null)
                return NotFound();

            if (document.Length == 0)
                return NoContent();
            
            return File(document, "application/pdf", "download.pdf");
        }
    }
}
