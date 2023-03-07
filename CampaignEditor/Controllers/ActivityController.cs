using Database.DTOs.ActivityDTO;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class ActivityController : ControllerBase
    {
        private readonly IActivityRepository _repository;
        public ActivityController(IActivityRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<ActivityDTO> CreateActivity(CreateActivityDTO activityDTO)
        {
            await _repository.CreateActivity(activityDTO);
            var activity = await _repository.GetActivityByName(activityDTO.act);
            return activity;
        }

        public async Task<ActivityDTO> GetActivityById(int id)
        {
            var activity = await _repository.GetActivityById(id);
            return activity;
        }
        public async Task<ActivityDTO> GetActivityByName(string name)
        {
            var activity = await _repository.GetActivityByName(name);
            return activity;
        }

        public async Task<IEnumerable<ActivityDTO>> GetAllActivities()
        {
            return await _repository.GetAllActivities();
        }

        public async Task<bool> UpdateActivity(UpdateActivityDTO activityDTO)
        {
            return await _repository.UpdateActivity(activityDTO);
        }

        public async Task<bool> DeleteActivityById(int id)
        {
            return await _repository.DeleteActivityById(id);
        }
    }
}
