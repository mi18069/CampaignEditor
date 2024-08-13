using Database.DTOs.CampaignDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface ICompletedValidationRepository
    {
        Task<bool> CreateCompValidation(CompletedValidation compValidation);
        Task<CompletedValidation> GetCompValidation(int cmpid, string date);
        Task<IEnumerable<CompletedValidation>> GetCompValidations(int cmpid);
        Task<bool> UpdateCompValidation(CompletedValidation compValidation);
        Task<bool> DeleteCompValidation(int cmpid);
        Task<bool> DeleteCompValidation(int cmpid, string date);
    }
}
