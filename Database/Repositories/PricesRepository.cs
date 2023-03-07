using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.PricesDTO;
using Database.DTOs.SectableDTO;
using Database.DTOs.SectablesDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class PricesRepository : IPricesRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        public PricesRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreatePrices(CreatePricesDTO pricesDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblprices (plid, dps, dpe, price, ispt)" +
                    "VALUES (@Plid, @Dps, @Dpe, @Price, @Ispt)",
            new
            {
                Plid = pricesDTO.plid,
                Dps = pricesDTO.dps,
                Dpe = pricesDTO.dpe,
                Price = pricesDTO.price,
                Ispt = pricesDTO.ispt
            });

            return affected != 0;
        }
        public async Task<PricesDTO> GetPricesById(int id)
        {
            using var connection = _context.GetConnection();

            var prices = await connection.QueryFirstOrDefaultAsync<Prices>(
                "SELECT * FROM tblprices WHERE prcid = @Id", new { Id = id });

            return _mapper.Map<PricesDTO>(prices);
        }
        public async Task<IEnumerable<PricesDTO>> GetAllPricesByPlId(int id)
        {
            using var connection = _context.GetConnection();

            var allPrices = await connection.QueryAsync<Prices>
                ("SELECT * FROM tblprices WHERE plid = @Id OR plid = 0", new { Id = id });

            return _mapper.Map<IEnumerable<PricesDTO>>(allPrices);
        }
        public async Task<IEnumerable<PricesDTO>> GetAllPrices()
        {
            using var connection = _context.GetConnection();

            var allPrices = await connection.QueryAsync<Prices>
                ("SELECT * FROM tblprices");

            return _mapper.Map<IEnumerable<PricesDTO>>(allPrices);
        }
        public async Task<bool> UpdatePrices(UpdatePricesDTO pricesDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblprices SET prcid = @Prcid, plid = @Plid, " +
                "dps = @Dps, dpe = @Dpe, price = @Price, ispt = @Ispt " +
                "WHERE sctid = @Sctid",
            new
            {
                Prcid = pricesDTO.prcid,
                Plid = pricesDTO.plid,
                Dps = pricesDTO.dps,
                Dpe = pricesDTO.dpe,
                Price = pricesDTO.price,
                Ispt = pricesDTO.ispt
            });

            return affected != 0;
        }
        public async Task<bool> DeletePricesById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblprices WHERE prcid = @Prcid", new { Prcid = id });

            return affected != 0;
        }
        public async Task<bool> DeletePricesByPlid(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblprices WHERE plid = @Plid", new { Plid = id });

            return affected != 0;
        }
    }
}
