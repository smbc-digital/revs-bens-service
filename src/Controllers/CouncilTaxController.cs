using Microsoft.AspNetCore.Mvc;
using StockportGovUK.AspNetCore.Attributes.TokenAuthentication;
using Microsoft.Extensions.Logging;
using revs_bens_service.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace revs_bens_service.Controllers
{
    [Produces("application/json")]
    [Route("api/v2/council-tax")]
    [ApiController]
    [TokenAuthentication]
    public class CouncilTaxController : ControllerBase
    {
        private readonly ICouncilTaxService _councilTaxService;
        private readonly ILogger<CouncilTaxController> _logger;

        public CouncilTaxController(ICouncilTaxService councilTaxService, ILogger<CouncilTaxController> logger)
        {
            _councilTaxService = councilTaxService;
            _logger = logger;
        }

        [HttpGet]
        [Route("{personReference}/details/{accountReference}/transactions/{year}")]

        public async Task<IActionResult> GetAllTransactionsForYear([FromRoute][Required]string personReference, [FromRoute][Required]string accountReference, [FromRoute][Required] int year)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Received an invalid council tax request with the following error messages: {0}");

                return BadRequest(ModelState);
            }

            var model = await _councilTaxService.GetAllTransactionsForYear(personReference, accountReference, year);

            if (model == null)
            {
                _logger.LogWarning("Could not match the Council request with ref: {0}");

                return NotFound();
            }

            return Ok(model);
        }
    }
}
