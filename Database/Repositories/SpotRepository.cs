﻿using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.SpotDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class SpotRepository : ISpotRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public SpotRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<bool> CreateSpot(CreateSpotDTO spotDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblcmpspot2 (cmpid, spotcode, spotname, spotlength, ignore) " +
                    "VALUES (@Cmpid, @Spotcode, @Spotname, @Spotlength, @Ignore)",
            new
            {
                Cmpid = spotDTO.cmpid,
                Spotcode = spotDTO.spotcode,
                Spotname = spotDTO.spotname,
                Spotlength = spotDTO.spotlength,
                Shid = spotDTO.ignore
            });

            return affected != 0;
        }
        public async Task<SpotDTO> GetSpotByCmpidAndCode(int id, string code)
        {
            using var connection = _context.GetConnection();

            var spot = await connection.QueryFirstOrDefaultAsync<Spot>(
                "SELECT * FROM tblcmpspot2 WHERE cmpid = @Cmpid AND spotcode = @Spotcode", 
                new { Cmpid = id, Spotcode = code });

            return _mapper.Map<SpotDTO>(spot);
        }

        public async Task<IEnumerable<SpotDTO>> GetSpotsByCmpid(int id)
        {
            using var connection = _context.GetConnection();

            var spots = await connection.QueryAsync<Spot>(
                "SELECT * FROM tblcmpspot2 WHERE cmpid = @Id", new { Id = id });

            return _mapper.Map<IEnumerable<SpotDTO>>(spots);
        }

        public async Task<SpotDTO> GetSpotByName(string spotname)
        {
            using var connection = _context.GetConnection();

            var spot = await connection.QueryFirstOrDefaultAsync<Spot>(
                "SELECT * FROM tblcmpspot2 WHERE spotname = @Spotname", new { Spotname = spotname });

            return _mapper.Map<SpotDTO>(spot);
        }

        public async Task<IEnumerable<SpotDTO>> GetAllSpots()
        {
            using var connection = _context.GetConnection();

            var allSpots = await connection.QueryAsync<Spot>("SELECT * FROM tblcmpspot2");

            return _mapper.Map<IEnumerable<SpotDTO>>(allSpots);
        }

        public async Task<bool> UpdateSpot(UpdateSpotDTO spotDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblcmpspot2 SET cmpid = @Cmpid, spotcode = @Spotcode, spotname = @Spotname, " +
                "spotlength = @Spotlength, ignore = @Ignore " +
                "WHERE spotname = @Spotname",
                new
                {
                    Cmpid = spotDTO.cmpid,
                    Spotcode = spotDTO.spotcode,
                    Spotname = spotDTO.spotname,
                    Spotlength = spotDTO.spotlength,
                    Shid = spotDTO.ignore
                });

            return affected != 0;
        }

        public async Task<bool> DeleteSpotsByCmpid(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblcmpspot2 WHERE cmpid = @Id", new { Id = id });

            return affected != 0;
        }

        public async Task<bool> DeleteSpotByCmpidAndCode(int id, string code)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblcmpspot2 WHERE cmpid = @Id AND spotcode = @Spotcode", 
                new { Id = id, Spotcode = code });

            return affected != 0;
        }
    }
}
