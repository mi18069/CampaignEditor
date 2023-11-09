using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{

    public class OnStartupContoller : ControllerBase
    {
        private readonly IDatabaseFunctionsRepository _databaseFunctionsRepository;
        public OnStartupContoller(IDatabaseFunctionsRepository databaseFunctionsRepository)
        {
            _databaseFunctionsRepository = databaseFunctionsRepository ?? throw new ArgumentNullException(nameof(databaseFunctionsRepository));
        }

        public async Task RunUpdateUnavailableDates()
        {
            await _databaseFunctionsRepository.RunUpdateUnavailableDates();
        }
    }
}
