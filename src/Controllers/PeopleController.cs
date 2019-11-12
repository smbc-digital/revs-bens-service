using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using revs_bens_service.Services.CouncilTax;
using revs_bens_service.Services.Dashboard;
using StockportGovUK.AspNetCore.Attributes.TokenAuthentication;

namespace revs_bens_service.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[Controller]")]
    [ApiController]
    [TokenAuthentication]
    public class PeopleController : ControllerBase
    {
        private readonly IPeopleService _peopleService;
        private readonly ICouncilTaxService _councilTaxService;
        public PeopleController(IPeopleService peopleService, ICouncilTaxService councilTaxService)
        {
            _peopleService = peopleService;
            _councilTaxService = councilTaxService;
        }

        [HttpGet]
        [Route("{personReference}/is-benefits-claimant")]
        public async Task<IActionResult> IsBenefitsClaimant([FromRoute][Required]string personReference)
        {
            var model = await _peopleService.IsBenefitsClaimant(personReference);

            return StatusCode(StatusCodes.Status200OK, model);
        }

        [HttpGet]
        [Route("{personReference}/details/{accountReference}/transactions/{year}")]

        public async Task<IActionResult> GetAllTransactionsForYear([FromRoute][Required]string personReference, [FromRoute][Required]string accountReference, [FromRoute][Required] int year)
        {
            var model = await _councilTaxService.GetAllTransactionsForYear(personReference, accountReference, year);
            return Ok(model);
        }
    }
}
