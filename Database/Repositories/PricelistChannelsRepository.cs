using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.PricelistChannels;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class PricelistChannelsRepository : IPricelistChannelsRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public PricelistChannelsRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreatePricelistChannels(CreatePricelistChannelsDTO pricelistChannelsDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblpricelistchn (plid, chid)" +
                    "VALUES (@Plid, @Chid)",
                new
                {
                    Plid = pricelistChannelsDTO.plid,
                    Chid = pricelistChannelsDTO.chid,
                });

            return affected != 0;
        }

        public async Task<PricelistChannelsDTO> GetPricelistChannelsByIds(int plid, int chid)
        {
            using var connection = _context.GetConnection();

            var pricelistChannels = await connection.QueryFirstOrDefaultAsync<PricelistChannels>(
                "SELECT * FROM tblpricelistchn WHERE plid = @Plid AND chid = @Chid", 
                new { 
                    PlId = plid,
                    Chid = chid
                });

            return _mapper.Map<PricelistChannelsDTO>(pricelistChannels);
        }

        public async Task<IEnumerable<PricelistChannelsDTO>> GetAllPricelistChannelsByPlid(int plid)
        {
            using var connection = _context.GetConnection();

            var pricelistChannels = await connection.QueryAsync<PricelistChannels>
                ("SELECT * FROM tblpricelistchn WHERE plid = @Plid", new { Plid = plid });

            return _mapper.Map<IEnumerable<PricelistChannelsDTO>>(pricelistChannels);
        }

        public async Task<IEnumerable<PricelistChannelsDTO>> GetAllPricelistChannelsByChid(int chid)
        {
            using var connection = _context.GetConnection();

            var pricelistChannels = await connection.QueryAsync<PricelistChannels>
                ("SELECT * FROM tblpricelistchn WHERE chid = @Chid", new { Chid = chid });

            return _mapper.Map<IEnumerable<PricelistChannelsDTO>>(pricelistChannels);
        }

        public async Task<IEnumerable<PricelistChannelsDTO>> GetAllPricelistChannels()
        {
            using var connection = _context.GetConnection();

            var pricelistChannels = await connection.QueryAsync<PricelistChannels>
                ("SELECT * FROM tblpricelistchn ");

            return _mapper.Map<IEnumerable<PricelistChannelsDTO>>(pricelistChannels);
        }

        public async Task<bool> UpdatePricelistChannels(UpdatePricelistChannelsDTO pricelistChannelsDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblpricelistchn SET plid = @Plid, chid = @Chid" +
                "WHERE plid = @Plid AND chid = @Chid",
                new
                {
                    Plid = pricelistChannelsDTO.plid,
                    Chid = pricelistChannelsDTO.chid
                });

            return affected != 0;
        }

        public async Task<bool> DeletePricelistChannelsByIds(int plid, int chid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblpricelistchn WHERE plid = @Plid AND chid = @Chid", 
                new { 
                    Plid = plid, 
                    Chid = chid
                });

            return affected != 0;
        }

    }
}
