using Dapper;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class DatabaseFunctionsController : ControllerBase
    {
        private readonly IDatabaseFunctionsRepository _repository;
        public DatabaseFunctionsController(IDatabaseFunctionsRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        public async Task<bool> RunUpdateUnavailableDates()
        {
            return await _repository.RunUpdateUnavailableDates();
        }

        public async Task<IEnumerable<DateTime>> GetAllUnavailableDates()
        {
            return await _repository.GetAllUnavailableDates();
        }

        public async Task<bool> StartAMRCalculation(int cmpid, int minusTime, int plusTime)
        {
            return await _repository.StartAMRCalculation(cmpid, minusTime, plusTime);
        }

    }
}
