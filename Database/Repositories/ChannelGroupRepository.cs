using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.ChannelGroupDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class ChannelGroupRepository : IChannelGroupRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        public ChannelGroupRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateChannelGroup(CreateChannelGroupDTO channelGroupDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblchannelgroups (chgrname, chgrown)" +
                    "VALUES (@Chgrname, @Chgrown)",
            new
            {
                Chgrname = channelGroupDTO.chgrname,
                Chgrown = channelGroupDTO.chgrown
            });

            return affected != 0;
        }

        public async Task<ChannelGroupDTO> GetChannelGroupById(int id)
        {
            using var connection = _context.GetConnection();

            var channelGroup = await connection.QueryFirstOrDefaultAsync<ChannelGroup>(
                "SELECT * FROM tblchannelgroups WHERE chgrid = @Id", new { Id = id });

            return _mapper.Map<ChannelGroupDTO>(channelGroup);
        }

        public async Task<ChannelGroupDTO> GetChannelGroupByName(string name)
        {
            using var connection = _context.GetConnection();

            var channelGroup = await connection.QueryFirstOrDefaultAsync<ChannelGroup>(
                "SELECT * FROM tblchannelgroups WHERE chgrname = @Chgrname", new { Chgrname = name });

            return _mapper.Map<ChannelGroupDTO>(channelGroup);
        }

        public async Task<ChannelGroupDTO> GetChannelGroupByNameAndOwner(string name, int owner)
        {
            using var connection = _context.GetConnection();

            var channelGroup = await connection.QueryFirstOrDefaultAsync<ChannelGroup>(
                "SELECT * FROM tblchannelgroups WHERE chgrown = @Chgrown AND chgrname = @Chgrname"
                , new { Chgrown = owner, Chgrname = name });

            return _mapper.Map<ChannelGroupDTO>(channelGroup);
        }


        public async Task<IEnumerable<ChannelGroupDTO>> GetAllOwnerChannelGroups(int ownerId)
        {
            using var connection = _context.GetConnection();

            var allChannelGroups = await connection.QueryAsync<ChannelGroup>
                ("SELECT * FROM tblchannelgroups WHERE chgrown = @Chgrown", new { Chgrown = ownerId});

            return _mapper.Map<IEnumerable<ChannelGroupDTO>>(allChannelGroups);
        }


        public async Task<IEnumerable<ChannelGroupDTO>> GetAllChannelGroups()
        {
            using var connection = _context.GetConnection();

            var allChannelGroups = await connection.QueryAsync<ChannelGroup>("SELECT * FROM tblchannelgroups");

            return _mapper.Map<IEnumerable<ChannelGroupDTO>>(allChannelGroups);
        }

        public async Task<bool> UpdateChannelGroup(UpdateChannelGroupDTO channelGroupDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblchannelgroups SET chgrid = @Chgrid, chgrname = @Chgrname, chgrown = @Chgrown " +
                "WHERE chgrid = @Chgrid",
            new
            {
                Chgrid = channelGroupDTO.chgrid,
                Chgrname = channelGroupDTO.chgrname,
                Chgrown = channelGroupDTO.chgrown
            });

            return affected != 0;
        }

        public async Task<bool> DeleteChannelGroupById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblchannelgroups WHERE chgrid = @Chgrid", new { Chgrid = id });

            return affected != 0;
        }
    }
}
