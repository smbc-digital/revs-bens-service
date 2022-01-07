using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using revs_bens_service.Services.Availability;
using StockportGovUK.AspNetCore.Attributes.TokenAuthentication;

namespace revs_bens_service.Controllers
{
    [Route("api/v1/[Controller]")]
    [Produces("application/json")]
    [ApiController]
    [TokenAuthentication]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;

        public AvailabilityController(IAvailabilityService availabilityService) => _availabilityService = availabilityService;

        [HttpGet]
        [Route("civica")]
        public async Task<IActionResult> GetCivicaAvailability() =>
            await _availabilityService.GetCivicaAvailability()
                ? Ok()
                : StatusCode(StatusCodes.Status424FailedDependency);

        [HttpGet]
        [Route("civica-brokers")]
        public async Task<IActionResult> GetCivicaBrokersAvailability() =>
            await _availabilityService.GetCivicaBrokersAvailability()
                ? Ok()
                : StatusCode(StatusCodes.Status424FailedDependency);
    }
}