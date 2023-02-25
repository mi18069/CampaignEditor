using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.SectableDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class SectableRepository : ISectableRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        public SectableRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateSectable(CreateSectableDTO sectableDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblsectable (sctname, sctlinear, sctactive, channel, ownedby)" +
                    "VALUES (@Sctname, @Sctlinear, @Sctactive, @Channel, @Ownedby)",
            new
            {
                Sctname = sectableDTO.sctname,
                Sctlinear = sectableDTO.sctlinear,
                Sctactive = sectableDTO.sctactive,
                Channel = sectableDTO.channel,
                Ownedby = sectableDTO.ownedby
            });

            return affected != 0;
        }

        public async Task<SectableDTO> GetSectableById(int id)
        {
            using var connection = _context.GetConnection();

            var sectable = await connection.QueryFirstOrDefaultAsync<Sectable>(
                "SELECT * FROM tblsectable WHERE sctid = @Id", new { Id = id });

            return _mapper.Map<SectableDTO>(sectable);
        }
        public async Task<SectableDTO> GetSectableByName(string sectablename)
        {
            using var connection = _context.GetConnection();

            var sectable = await connection.QueryFirstOrDefaultAsync<Sectable>(
                "SELECT * FROM tblsectable WHERE sctname = @Sctname", new { Sctname = sectablename });

            return _mapper.Map<SectableDTO>(sectable);
        }
        public async Task<IEnumerable<SectableDTO>> GetAllSectablesByOwnerId(int id)
        {
            using var connection = _context.GetConnection();

            var allSectables = await connection.QueryAsync<Sectable>
                ("SELECT * FROM tblsectable WHERE ownedby = @Id OR ownedby = 0", new { Id = id });

            return _mapper.Map<IEnumerable<SectableDTO>>(allSectables);
        }
        public async Task<IEnumerable<SectableDTO>> GetAllSectables()
        {
            using var connection = _context.GetConnection();

            var allSectables = await connection.QueryAsync<Sectable>("SELECT * FROM tblsectable");

            return _mapper.Map<IEnumerable<SectableDTO>>(allSectables);
        }

        public async Task<bool> UpdateSectable(UpdateSectableDTO sectableDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblsectable SET sctid = @Sctid, sctname = @Sctname, " +
                "sctlinear = @Sctlinear, channel = @Channel, ownedby = @Ownedby" +
                "WHERE sctid = @Sctid",
                new
                {
                    Sctid = sectableDTO.sctid,
                    Sctname = sectableDTO.sctname,
                    Sctlinear = sectableDTO.sctlinear,
                    Channel = sectableDTO.channel,
                    Ownedby = sectableDTO.ownedby
                });

            return affected != 0;
        }
        public async Task<bool> DeleteSectableById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblsectable WHERE sctid = @Sctid", new { Sctid = id });

            return affected != 0;
        }

    }
}
