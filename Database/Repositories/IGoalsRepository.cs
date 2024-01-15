using Database.DTOs.GoalsDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IGoalsRepository
    {
        Task<bool> CreateGoals(CreateGoalsDTO goalsDTO);
        Task<GoalsDTO> GetGoalsByCmpid(int id);
        Task<IEnumerable<GoalsDTO>> GetAllGoals();
        Task<bool> UpdateGoals(UpdateGoalsDTO goalsDTO);
        Task<bool> DeleteGoalsByCmpid(int id);
        Task<bool> DuplicateGoals(int oldCmpid, int newCmpid);
    }
}
