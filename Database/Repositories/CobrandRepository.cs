using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.CobrandDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class CobrandRepository : ICobrandRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public CobrandRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateCobrand(CreateCobrandDTO cobrandDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblcobrands (cmpid, chid, spotcode, coef) " +
                    "VALUES (@Cmpid, @Chid, @Spotcode, @Coef)",
            new
            {
                Cmpid = cobrandDTO.cmpid,
                Chid = cobrandDTO.chid,
                Spotcode = cobrandDTO.spotcode.ToString(),
                Coef = cobrandDTO.coef
            });

            return affected != 0;
        }
        public async Task<IEnumerable<CobrandDTO>> GetAllCampaignCobrands(int cmpid)
        {
            using var connection = _context.GetConnection();

            var allCobrands = await connection.QueryAsync<Cobrand>
                ("SELECT * FROM tblcobrands WHERE cmpid=@Cmpid",
                new {Cmpid = cmpid});


            return _mapper.Map<IEnumerable<CobrandDTO>>(allCobrands);
        }

        public async Task<IEnumerable<CobrandDTO>> GetAllChannelCobrands(int cmpid, int chid)
        {
            using var connection = _context.GetConnection();

            var allCobrands = await connection.QueryAsync<Cobrand>
                ("SELECT * FROM tblcobrands WHERE cmpid = @Cmpid AND chid = @Chid",
                new {Cmpid = cmpid, Chid = chid});


            return _mapper.Map<IEnumerable<CobrandDTO>>(allCobrands);
        }

        public async Task<bool> UpdateCobrand(UpdateCobrandDTO cobrandDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblcobrands SET cmpid = @Cmpid, chid = @Chid, spotcode = @Spotcode, coef = @Coef" +
                " WHERE cmpid = @Cmpid AND chid = @Chid AND spotcode = @Spotcode",
                new
                {
                    Cmpid = cobrandDTO.cmpid,
                    Chid = cobrandDTO.chid,
                    Spotcode = cobrandDTO.spotcode.ToString(),
                    Coef = cobrandDTO.coef
                });

            return affected != 0;
        }

        public async Task<bool> DeleteCobrand(int cmpid, int chid, char spotcode)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblcobrands WHERE cmpid = @Cmpid AND chid = @Chid AND spotcode = @Spotcode"
                , new { Cmpid = cmpid, Chid = chid, Spotcode = spotcode });

            return affected != 0;
        }

        public async Task<bool> DeleteCobrandsForChannel(int cmpid, int chid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblcobrands WHERE cmpid = @Cmpid AND chid = @Chid"
                , new { Cmpid = cmpid, Chid = chid});

            return affected != 0;
        }

        public async Task<bool> DeleteCampaignCobrands(int cmpid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblcobrands WHERE cmpid = @Cmpid"
                , new { Cmpid = cmpid });

            return affected != 0;
        }



    }
}
