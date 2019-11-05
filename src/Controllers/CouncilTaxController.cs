using Microsoft.AspNetCore.Mvc;
using StockportGovUK.AspNetCore.Attributes.TokenAuthentication;
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

        public CouncilTaxController(ICouncilTaxService councilTaxService)
        {
            _councilTaxService = councilTaxService;
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
