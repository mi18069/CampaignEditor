using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.ChannelCmpDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace Database.Repositories
{
    public class ChannelCmpRepository : IChannelCmpRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public ChannelCmpRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateChannelCmp(CreateChannelCmpDTO channelCmpDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblcmpchn (cmpid, chid, plid, actid, plidbuy, actidbuy) " +
                    "VALUES (@Cmpid, @Chid, @Plid, @Actid, @Plidbuy, @Actidbuy)",
            new
            {
                Cmpid = channelCmpDTO.cmpid,
                Chid = channelCmpDTO.chid,
                Plid = channelCmpDTO.plid,
                Actid = channelCmpDTO.actid,
                Plidbuy = channelCmpDTO.plidbuy,
                Actidbuy = channelCmpDTO.actidbuy
            });

            return affected != 0;
        }

        public async Task<ChannelCmpDTO> GetChannelCmpByIds(int cmpid, int chid)
        {
            using var connection = _context.GetConnection();

            var channelCmp = await connection.QueryFirstOrDefaultAsync<ChannelCmp>(
                "SELECT * FROM tblcmpchn WHERE cmpid = @Cmpid AND chid = @Chid",
                new { Cmpid = cmpid, Chid = chid });

            return _mapper.Map<ChannelCmpDTO>(channelCmp);
        }

        public async Task<IEnumerable<ChannelCmpDTO>> GetChannelCmpsByCmpid(int id)
        {
            using var connection = _context.GetConnection();

            var channelsCmps = await connection.QueryAsync<ChannelCmp>(
                "SELECT * FROM tblcmpchn WHERE cmpid = @Id", new { Id = id });

            return _mapper.Map<IEnumerable<ChannelCmpDTO>>(channelsCmps);
        }

        public async Task<IEnumerable<ChannelCmpDTO>> GetAllChannelCmps()
        {
            using var connection = _context.GetConnection();

            var allChannelCmps = await connection.QueryAsync<ChannelCmp>("SELECT * FROM tblcmpchn");

            return _mapper.Map<IEnumerable<ChannelCmpDTO>>(allChannelCmps);
        }

        public async Task<bool> UpdateChannelCmp(UpdateChannelCmpDTO channelCmpDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblcmpchn SET cmpid = @Cmpid, chid = @Chid, plid = @Plid, " +
                "actid = @Actid, plidbuy = @Plidbuy, Actidbuy = @Actidbuy " +
                "WHERE cmpid = @Cmpid AND plid = @Plid",
                new
                {
                    Cmpid = channelCmpDTO.cmpid,
                    Chid = channelCmpDTO.chid,
                    Plid = channelCmpDTO.plid,
                    Actid = channelCmpDTO.actid,
                    Plidbuy = channelCmpDTO.plidbuy,
                    Actidbuy = channelCmpDTO.actidbuy
                });

            return affected != 0;
        }

        public async Task<bool> DeleteChannelCmpByCmpid(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblcmpchn WHERE cmpid = @Id", new { Id = id });

            return affected != 0;
        }

        public async Task<bool> DeleteChannelCmpByIds(int cmpid, int plid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblcmpchn WHERE cmpid = @Id AND plid = @Plid",
                new { Id = cmpid, Plid = plid });

            return affected != 0;
        }

        public async Task<bool> DuplicateChannelCmp(int oldCmpid, int newCmpid)
        {

            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                @"INSERT INTO tblcmpchn (cmpid, chid, plid, actid, plidbuy, actidbuy)
                  SELECT @NewCmpid, chid, plid, actid, plidbuy, actidbuy
                  FROM tblcmpchn WHERE cmpid = @OldCmpid;",
                new { OldCmpid = oldCmpid, NewCmpid = newCmpid });

            return affected != 0;

        }
    }
}
