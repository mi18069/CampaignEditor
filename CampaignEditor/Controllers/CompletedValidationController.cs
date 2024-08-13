using Database.Entities;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class CompletedValidationController
    {
        private readonly ICompletedValidationRepository _repository;
        public CompletedValidationController(ICompletedValidationRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<bool> CreateCompValidation(CompletedValidation compValidation)
         => await _repository.CreateCompValidation(compValidation); 

        public async Task<CompletedValidation> GetCompValidation(int cmpid, string date)
         => await _repository.GetCompValidation(cmpid, date);

        public async Task<IEnumerable<CompletedValidation>> GetCompValidations(int cmpid)
         => await _repository.GetCompValidations(cmpid);

        public async Task<bool> UpdateCompValidation(CompletedValidation compValidation)
         => await _repository.UpdateCompValidation(compValidation);

        public async Task<bool> DeleteCompValidation(int cmpid)
         => await _repository.DeleteCompValidation(cmpid);

        public async Task<bool> DeleteCompValidation(int cmpid, string date)
         => await _repository.DeleteCompValidation(cmpid, date);
    }
}
