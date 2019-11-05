using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using revs_bens_service.Services.BenefitsService;
using StockportGovUK.AspNetCore.Attributes.TokenAuthentication;

namespace revs_bens_service.Controllers
{

    [Produces("application/json")]
    [Route("api/v1/[Controller]")]
    [ApiController]
    [TokenAuthentication]

    public class BenefitsController : Controller
    {
        private readonly IBenefitsService _benefitsService;
        private readonly ILogger _logger;

        public BenefitsController(IBenefitsService benefitsService, ILogger logger)
        {
            _benefitsService = benefitsService;
            _logger = logger;
        }


        [HttpGet]
        [Route("benefits/{personReference}/details")]
        public async Task<IActionResult> GetBenefitDetails (string personReference)
        {
            var model = await _benefitsService.GetBenefitDetails(personReference);

            return StatusCode(StatusCodes.Status200OK, model);
        }
    }
}