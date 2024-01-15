using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.CmpBrndDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class CmpBrndRepository : ICmpBrndRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public CmpBrndRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateCmpBrnd(CmpBrndDTO cmpbrndDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblcmpbrnd (cmpid, brbrand) " +
                    "VALUES (@Cmpid, @Brbrand)",
            new
            {
                Cmpid = cmpbrndDTO.cmpid,
                Brbrand = cmpbrndDTO.brbrand
            });

            return affected != 0;
        }

        public async Task<IEnumerable<CmpBrndDTO>> GetCmpBrndsByCmpId(int id)
        {
            using var connection = _context.GetConnection();

            var brand = await connection.QueryAsync<CmpBrnd>(
                "SELECT * FROM tblcmpbrnd WHERE cmpid = @Id", new { Id = id });

            return _mapper.Map<IEnumerable<CmpBrndDTO>>(brand);
        }

        public async Task<bool> UpdateBrand(CmpBrndDTO cmpbrndDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblcmpbrnd SET cmpid = @Cmpid, brbrand = @Brbrand" +
                " WHERE cmpid = @Cmpid",
                new
                {
                    Cmpid = cmpbrndDTO.cmpid,
                    Brbrand = cmpbrndDTO.brbrand
                });

            return affected != 0;
        }

        public async Task<bool> DeleteBrandByCmpId(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblcmpbrnd WHERE cmpid = @Cmpid", new { Cmpid = id });

            return affected != 0;
        }

        public async Task<bool> DuplicateCmpBrnd(int oldCmpid, int newCmpid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                @"INSERT INTO tblcmpbrnd (cmpid, brbrand)
                  SELECT @NewCmpid, brbrand
                  FROM tblcmpbrnd WHERE cmpid = @OldCmpid;",
                new { OldCmpid = oldCmpid, NewCmpid = newCmpid });

            return affected != 0;
        }
    }
}
