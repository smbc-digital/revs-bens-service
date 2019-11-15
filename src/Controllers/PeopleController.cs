using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading;
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
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            Console.WriteLine("RunTime " + elapsedTime);

            return Ok(model);
        }
    }
}
