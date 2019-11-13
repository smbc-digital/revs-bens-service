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
    public class PersonController : ControllerBase
    {
        private readonly IPersonService _personService;
        private readonly IBenefitsService _benefitsService;

        public PersonController(IPersonService personService, IBenefitsService benefitsService)
        {
            _personService = personService;
            _benefitsService = benefitsService;
        }

        [HttpGet]
        [Route("{personReference}/is-benefits-claimant")]
        public async Task<IActionResult> IsBenefitsClaimant([FromRoute][Required]string personReference)
        {
            var model = await _personService.IsBenefitsClaimant(personReference);

            return StatusCode(StatusCodes.Status200OK, model);
        }

        [HttpGet]
        [Route("{personReference}/benefits")]
        public async Task<IActionResult> GetBenefits(string personReference)
        {
            var model = await _benefitsService.GetBenefits(personReference);

            return StatusCode(StatusCodes.Status200OK, model);
        }
    }
}
