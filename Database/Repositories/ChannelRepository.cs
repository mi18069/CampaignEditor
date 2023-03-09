using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.ChannelDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class ChannelRepository : IChannelRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public ChannelRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateChannel(CreateChannelDTO channelDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblchannels (chactive, chname, chrsid, chsname, shid, chrfid) " +
                    "VALUES (@Chactive, @Chname, @Chrsid, @Chsname, @Shid, @Chrfid)",
            new
                {
                Chactive = channelDTO.chactive,
                Chname = channelDTO.chname,
                Chrsid = channelDTO.chrdsid,
                Chsname = channelDTO.chsname,
                Shid = channelDTO.shid,
                Chrfid = channelDTO.chrfid,
                });

            return affected != 0;
        }

        public async Task<ChannelDTO> GetChannelById(int id)
        {
            using var connection = _context.GetConnection();

            var channel = await connection.QueryFirstOrDefaultAsync<Channel>(
                "SELECT * FROM tblchannels WHERE chid = @Id", new { Id = id });

            return _mapper.Map<ChannelDTO>(channel);
        }

        public async Task<ChannelDTO> GetChannelByName(string channelname)
        {
            using var connection = _context.GetConnection();

            var channel = await connection.QueryFirstOrDefaultAsync<Channel>(
                "SELECT * FROM tblchannels WHERE chname = @Chname", new { Chname = channelname });

            return _mapper.Map<ChannelDTO>(channel);
        }

        public async Task<IEnumerable<ChannelDTO>> GetAllChannels()
        {
            using var connection = _context.GetConnection();

            var allChannels = await connection.QueryAsync<Channel>("SELECT * FROM tblchannels");

            return _mapper.Map<IEnumerable<ChannelDTO>>(allChannels);
        }

        public async Task<bool> UpdateChannel(UpdateChannelDTO channelDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblchannels SET chid = @Chid, chactive = @Chactive, chname = @Chname, " +
                "chrdsid = @Chrdsid, chsname = @Chsname, shid = @Shid, chrfid = @Chrfid " +
                "WHERE chid = @Chid",
                new
                {
                    Usrid = channelDTO.chid,
                    Usrname = channelDTO.chactive,
                    Usrpass = channelDTO.chname,
                    Usrlevel = channelDTO.chrdsid,
                    Email = channelDTO.chsname,
                    Telefon = channelDTO.shid,
                    Enabled = channelDTO.chrfid
                });

            return affected != 0;
        }

        public async Task<bool> DeleteChannelById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblchannels WHERE chid = @Chid", new { Chid = id });

            return affected != 0;
        }

    }
}
