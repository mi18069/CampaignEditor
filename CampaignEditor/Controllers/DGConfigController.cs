using Database.Entities;
using Database.Repositories;
using System;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class DGConfigController
    {

        private readonly IDGConfigRepository _repository;

        public DGConfigController(IDGConfigRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<bool> CreateDGConfig(DGConfig dgConfig) => 
            await _repository.CreateDGConfig(dgConfig);

        public async Task<DGConfig?> GetDGConfig(int usrid, int clid) => 
            await _repository.GetDGConfig(usrid, clid);

        public async Task<bool> UpdateDGConfig(DGConfig dgConfig) => 
            await _repository.UpdateDGConfig(dgConfig);
        public async Task<bool> UpdateDGConfigFor(int usrid, int clid, string mask) =>
            await _repository.UpdateDGConfigFor(usrid, clid, mask);
        public async Task<bool> UpdateDGConfigExp(int usrid, int clid, string mask) =>
            await _repository.UpdateDGConfigExp(usrid, clid, mask);
        public async Task<bool> UpdateDGConfigReal(int usrid, int clid, string mask) =>
            await _repository.UpdateDGConfigReal(usrid, clid, mask);

        public async Task<bool> DeleteDGConfig(int usrid, int clid) =>
            await _repository.DeleteDGConfig(usrid, clid);

        public async Task<bool> DeleteDGConfigByClid(int clid) =>
            await _repository.DeleteDGConfigByClid(clid);

        public async Task<bool> DeleteDGConfigByUsrid(int usrid) =>
            await _repository.DeleteDGConfigByUsrid(usrid);
    }
}
