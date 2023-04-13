using Database.DTOs.MediaPlanDTO;
using Database.DTOs.SchemaDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class SchemaController : ControllerBase
    {
        private readonly ISchemaRepository _repository;
        public SchemaController(ISchemaRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        public async Task<bool> CreateSchema(CreateSchemaDTO schemaDTO)
        {
            return await _repository.CreateSchema(schemaDTO);
        }

        public async Task<SchemaDTO> GetSchemaById(int id)
        {
            return await _repository.GetSchemaById(id);
        }

        public async Task<SchemaDTO> GetSchemaByName(string name)
        {
            return await _repository.GetSchemaByName(name);
        }

        public async Task<IEnumerable<SchemaDTO>> GetAllChannelSchemas(int chid)
        {
            return await _repository.GetAllChannelSchemas(chid);
        }

        public async Task<IEnumerable<SchemaDTO>> GetAllChannelSchemasWithinDate(int chid, DateOnly sdate, DateOnly edate)
        {
            return await _repository.GetAllChannelSchemasWithinDate(chid, sdate, edate);
        }

        public async Task<IEnumerable<SchemaDTO>> GetAllSchemas()
        {
            return await _repository.GetAllSchemas();
        }

        public async Task<bool> UpdateSchema(UpdateSchemaDTO schemaDTO)
        {
            return await _repository.UpdateSchema(schemaDTO);
        }

        public async Task<bool> DeleteSchemaById(int id)
        {
            return await _repository.DeleteSchemaById(id);
        }
    }
}
