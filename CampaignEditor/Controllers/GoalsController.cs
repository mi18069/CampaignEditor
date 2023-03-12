using Database.DTOs.GoalsDTO;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class GoalsController : ControllerBase
    {
        private readonly IGoalsRepository _repository;
        public GoalsController(IGoalsRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<GoalsDTO> CreateGoals(CreateGoalsDTO goalsDTO)
        {
            await _repository.CreateGoals(goalsDTO);
            var goals = await _repository.GetGoalsByCmpid(goalsDTO.cmpid);
            return goals;
        }

        public async Task<GoalsDTO> GetGoalsByCmpid(int id)
        {
            return await _repository.GetGoalsByCmpid(id);
        }

        public async Task<IEnumerable<GoalsDTO>> GetAllGoals()
        {
            var goals = await _repository.GetAllGoals();
            return goals;
        }

        public async Task<bool> UpdateGoals(UpdateGoalsDTO goalsDTO)
        {
            var goals = await _repository.UpdateGoals(goalsDTO);
            return goals;
        }

        public async Task<bool> DeleteGoalsByCmpid(int id)
        {
            return await _repository.DeleteGoalsByCmpid(id);
        }
    }
}
