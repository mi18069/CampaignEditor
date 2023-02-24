
using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.PricelistChannels;
using Database.DTOs.SectableChannels;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class SectableChannelsRepository : ISectableChannelsRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public SectableChannelsRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateSectableChannels(CreateSectableChannelsDTO sectableDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblsectablechn (sctid, chid)" +
                    "VALUES (@Sctid, @Chid)",
                new
                {
                    Sctid = sectableDTO.sctid,
                    Chid = sectableDTO.chid
                }) ;

            return affected != 0;
        }

        public async Task<SectableChannelsDTO> GetSectableChannelsByIds(int sctid, int chid)
        {
            using var connection = _context.GetConnection();

            var sectableChannels = await connection.QueryFirstOrDefaultAsync<SectableChannels>(
                "SELECT * FROM tblsectablechn WHERE sctid = @Sctid AND chid = @Chid",
                new
                {
                    Sctid = sctid,
                    Chid = chid
                });

            return _mapper.Map<SectableChannelsDTO>(sectableChannels);
        }
        public async Task<IEnumerable<SectableChannelsDTO>> GetAllSectableChannelsBySctid(int sctid)
        {
            using var connection = _context.GetConnection();

            var sectableChannels = await connection.QueryAsync<SectableChannels>
                ("SELECT * FROM tblsectablechn WHERE sctid = @Sctid", new { Sctid = sctid });

            return _mapper.Map<IEnumerable<SectableChannelsDTO>>(sectableChannels);
        }

        public async Task<IEnumerable<SectableChannelsDTO>> GetAllSectableChannelsByChid(int chid)
        {
            using var connection = _context.GetConnection();

            var sectableChannels = await connection.QueryAsync<PricelistChannels>
                ("SELECT * FROM tblsectablechn WHERE chid = @Chid", new { Chid = chid });

            return _mapper.Map<IEnumerable<SectableChannelsDTO>>(sectableChannels);
        }

        
        public async Task<IEnumerable<SectableChannelsDTO>> GetAllSectableChannels()
        {
            using var connection = _context.GetConnection();

            var sectableChannels = await connection.QueryAsync<PricelistChannels>
                ("SELECT * FROM tblsectablechn ");

            return _mapper.Map<IEnumerable<SectableChannelsDTO>>(sectableChannels);
        }

        public async Task<bool> UpdateSectableChannels(UpdateSectableChannelsDTO sectableChannelsDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblsectablechn SET sctid = @Sctid, chid = @Chid" +
                "WHERE sctid = @Sctid AND chid = @Chid",
                new
                {
                    Sctid = sectableChannelsDTO.sctid,
                    Chid = sectableChannelsDTO.chid
                });

            return affected != 0;
        }

        public async Task<bool> DeleteSectableChannelsByIds(int sctid, int chid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblsectablechn WHERE sctid = @Sctid AND chid = @Chid",
                new
                {
                    Sctid = sctid,
                    Chid = chid
                });

            return affected != 0;
        }
    }
}
