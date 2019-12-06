using System.Threading.Tasks;

namespace revs_bens_service.Services.Availability
{
    public interface IAvailabilityService
    {
        Task<bool> GetCivicaAvailability();
    }
}