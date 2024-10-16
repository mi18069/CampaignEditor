﻿using Database.DTOs.TargetDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class TargetController
    {
        private readonly ITargetRepository _repository;
        public TargetController(ITargetRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<TargetDTO> CreateTarget(CreateTargetDTO targetDTO)
        {
            await _repository.CreateTarget(targetDTO);
            var target = await _repository.GetTargetByName(targetDTO.targname);
            return target;
        }

        public async Task<TargetDTO> GetTargetById(int id)
        {
            var target = await _repository.GetTargetById(id);
            return target;
        }
        public async Task<TargetDTO> GetTargetByName(string targname)
        {
            var target = await _repository.GetTargetByName(targname);
            return target;
        }

        public async Task<bool> UpdateTarget(UpdateTargetDTO targetDTO)
        {
            return await _repository.UpdateTarget(targetDTO);
        }

        public async Task<bool> DeleteTargetById(int id)
        {
            return await _repository.DeleteTargetById(id);
        }
        public async Task<bool> DeleteTargetByName(string targname)
        {
            return await _repository.DeleteTargetByName(targname);
        }
        public async Task<IEnumerable<TargetDTO>> GetAllTargets()
        {
            return await _repository.GetAllTargets();
        }
        public async Task<IEnumerable<TargetDTO>> GetAllClientTargets(int clientId)
        {
            return await _repository.GetAllClientTargets(clientId);
        }

        // For checking if some client have access to some target by name
        // For fixing bug where client can't name target because name is already taken
        // Returns true if there is collision problems
        public async Task<bool> CheckClientTargetName(string targname, int clid)
        {
            var clientTargets = await _repository.GetAllClientTargets(clid);
            foreach (var clientTarget in clientTargets)
            {
                if (clientTarget.targname.Trim() == targname.Trim())
                    return true;
            }
            return false;
        }
    }
}
