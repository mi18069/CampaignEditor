using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.SectablesDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class SectablesRepository : ISectablesRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        public SectablesRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<bool> CreateSectables(CreateSectablesDTO sectablesDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblsectables (sctid, sec, coef)" +
                    "VALUES (@Sctid, @Sec, @Coef)",
            new
            {
                Sctid = sectablesDTO.sctid,
                Sec = sectablesDTO.sec,
                Coef = sectablesDTO.coef
            });

            return affected != 0;
        }
        public async Task<IEnumerable<SectablesDTO>> GetSectablesById(int id)
        {
            using var connection = _context.GetConnection();

            var sectables = await connection.QueryAsync<Sectables>(
                "SELECT * FROM tblsectables WHERE sctid = @Id", new { Id = id });

            return _mapper.Map<IEnumerable<SectablesDTO>>(sectables);
        }
        public async Task<SectablesDTO> GetSectablesByIdAndSec(int id, int sec)
        {
            using var connection = _context.GetConnection();

            var sectables = await connection.QueryAsync<Sectables>(
                "SELECT * FROM tblsectables WHERE sctid = @Id AND sec = @Sec", 
                new { Id = id, Sec = sec });

            return _mapper.Map<SectablesDTO>(sectables);
        }

        public async Task<IEnumerable<SectablesDTO>> GetAllSectables()
        {
            using var connection = _context.GetConnection();

            var allSectables = await connection.QueryAsync<Sectables>("SELECT * FROM tblsectables");

            return _mapper.Map<IEnumerable<SectablesDTO>>(allSectables);
        }

        public async Task<bool> UpdateSectables(UpdateSectablesDTO sectablesDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblsectables SET sctid = @Sctid, sec = @Sec, coef = @Coef" +
                "WHERE sctid = @Sctid",
                new
                {
                    Sctid = sectablesDTO.sctid,
                    Sec = sectablesDTO.sec,
                    Coef = sectablesDTO.coef
                });

            return affected != 0;
        }
        public async Task<bool> DeleteSectablesById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblsectables WHERE sctid = @Sctid", new { Sctid = id });

            return affected != 0;
        }

    }
}
