using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> IsBenefitsClaimant([FromRoute][Required]string personReference)
        {
            var model = await _benefitsService.IsBenefitsClaimant(personReference);

            return StatusCode(StatusCodes.Status200OK, model);
        }

        [HttpGet]
        [Route("{personReference}/benefits")]
        public async Task<IActionResult> GetBenefits([FromRoute][Required]string personReference)
        {
            var model = await _benefitsService.GetBenefits(personReference);

            return StatusCode(StatusCodes.Status200OK, model);
        }

        //TODO: Make better than 1:10.22	
        [HttpGet]
        [Route("{personReference}/council-tax/{accountReference}/{year}")]
        public async Task<IActionResult> GetCouncilTaxDetails(
            [FromRoute][Required]string personReference,
            [FromRoute][Required]string accountReference,
            [FromRoute][Required]int year)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var model = await _councilTaxService.GetCouncilTaxDetails(personReference, accountReference, year);

            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.	
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.	
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
                
            return Ok(new { elapsedTime, model });
        }
    }
}
