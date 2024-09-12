using Database.DTOs.RealizedSpotDTO;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IRealizedSpotRepository
    {
        Task<RealizedSpotDTO> GetRealizedSpot(int spotnum);
    }
}
