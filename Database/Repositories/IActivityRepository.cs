using Database.DTOs.ActivityDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IActivityRepository
    {
        Task<bool> CreateActivity(CreateActivityDTO activityDTO);
        Task<ActivityDTO> GetActivityById(int id);
        Task<ActivityDTO> GetActivityByName(string name);
        Task<IEnumerable<ActivityDTO>> GetAllActivities();
        Task<bool> UpdateActivity(UpdateActivityDTO activityDTO);
        Task<bool> DeleteActivityById(int id);
    }
}
