using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.DayPartDTO;
using Database.DTOs.SpotPairDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class SpotPairRepository : ISpotPairRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        public SpotPairRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<bool> CreateSpotPair(CreateSpotPairDTO spotPairDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                @"INSERT INTO tblspotpairs (cmpid, spotcode, spotnum) 
                  VALUES (@Cmpid, @Spotcode, @Spotnum) ", new
                {
                    Cmpid = spotPairDTO.cmpid,
                    Spotcode = spotPairDTO.spotcode,
                    Spotnum = spotPairDTO.spotnum
                });

            return affected != 0;
        }
        public async Task<SpotPairDTO> GetSpotPairBySpotnum(int cmpid, int spotnum)
        {
            using var connection = _context.GetConnection();

            var spotPair = await connection.QueryFirstOrDefaultAsync<SpotPair>(
                "SELECT * FROM tblspotpairs WHERE cmpid = @Cmpid AND spotnum = @Spotnum", 
                new { Cmpid = cmpid, Spotnum = spotnum });

            return _mapper.Map<SpotPairDTO>(spotPair);
        }

        public async Task<IEnumerable<SpotPairDTO>> GetAllCampaignSpotPairs(int cmpid)
        {
            using var connection = _context.GetConnection();

            var allSpotPairs = await connection.QueryAsync<SpotPair>(
                "SELECT * FROM tblspotpairs WHERE cmpid = @Cmpid",
                new { Cmpid = cmpid });

            return _mapper.Map<IEnumerable<SpotPairDTO>>(allSpotPairs);
        }

        public async Task<bool> UpdateSpotPair(UpdateSpotPairDTO spotPairDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "Update tblspotpairs SET cmpid = @Cmpid, " +
                "spotcode = @Spotcode, spotnum = @Spotnum " +
                "WHERE cmpid = @Cmpid AND spotnum = @Spotnum",
            new
            {
                Cmpid = spotPairDTO.cmpid,
                Spotcode = spotPairDTO.spotcode,
                Spotnum = spotPairDTO.spotnum
            });

            return affected != 0;
        }
        public async Task<bool> DeleteSpotcodeFromSpotPairs(int cmpid, string spotcode)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "Update tblspotpairs SET spotcode = NULL " +
                "WHERE cmpid = @Cmpid AND spotcode = @Spotcode",
            new
            {
                Cmpid = cmpid,
                Spotcode = spotcode
            });

            return affected != 0;
        }

        public async Task<bool> DeleteAllCampaignSpotPairs(int cmpid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblspotpairs WHERE cmpid = @Cmpid", new { Cmpid = cmpid });

            return affected != 0;
        }
    }
}
