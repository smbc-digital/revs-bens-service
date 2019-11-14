using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using revs_bens_service.Services.Dashboard;
using revs_bens_service.Services.HousingBenefits;
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
        private readonly IBenefitsService _benefitsService;

        public PeopleController(IPeopleService peopleService, IBenefitsService benefitsService)
        {
            _peopleService = peopleService;
            _benefitsService = benefitsService;
        }

        [HttpGet]
        [Route("{personReference}/is-benefits-claimant")]
        public async Task<IActionResult> IsBenefitsClaimant([FromRoute][Required] string personReference)
        {
            var model = await _peopleService.IsBenefitsClaimant(personReference);

            return StatusCode(StatusCodes.Status200OK, model);
        }

        [HttpGet]
        [Route("{personReference}/benefits")]
        public async Task<IActionResult> GetBenefits([FromRoute][Required] string personReference)
        {
            var model = await _benefitsService.GetBenefitsDetails(personReference);

            return StatusCode(StatusCodes.Status200OK, model);
        }
    }
}
