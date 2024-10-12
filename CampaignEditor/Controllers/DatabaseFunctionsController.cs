using Dapper;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Navigation;

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

        public async Task<bool> StartAMRCalculation(int cmpid, int minusTime, int plusTime, int xmpid=-1)
        {
            return await _repository.StartAMRCalculation(cmpid, minusTime, plusTime, xmpid);
        }

        public async Task<bool> CheckForecastPrerequisites(int cmpid)
        {
            return await _repository.CheckForecastPrerequisites(cmpid);
        }

        public async Task<bool> StartReachCalculation(int cmpid, int segins, int segbet, bool delete = true, bool expr = true, string path = null)
        {
            return await _repository.StartReachCalculation(cmpid, segins, segbet, delete, expr, path);
        }

        public async Task<bool> StartRealizationFunction(int cmpid, int brandid, string sdate, string edate)
        {
            return await _repository.StartRealizationFunction(cmpid, brandid, sdate, edate);
        }

        public async Task<DateOnly> GetLastDateImport()
        => await _repository.GetLastDateImport();

    }
}
