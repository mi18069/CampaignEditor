using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IDatabaseFunctionsRepository
    {
        Task<bool> RunUpdateUnavailableDates();
        Task<IEnumerable<DateTime>> GetAllUnavailableDates();
        Task<bool> StartAMRCalculation(int cmpid, int minusTime, int plusTime);

    }
}
