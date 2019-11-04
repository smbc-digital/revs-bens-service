using System.Threading.Tasks;
using Amazon.Runtime.Internal.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Owin.Logging;
using revs_bens_service.Services.Benefits;
using StockportGovUK.AspNetCore.Attributes.TokenAuthentication;
using ILogger = Microsoft.Owin.Logging.ILogger;

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
        [Route("benefits-details{personReference}")]
        public Task<IActionResult> GetBenefitDetails (string personReference)
        {
            if (!ModelState.IsValid)
            {
                _logger.WriteWarning("");

                return BadRequest(ModelState);
            }

            var model = await _benefitsService.BenefitDetails();

            Return Ok();
        }
    }
}