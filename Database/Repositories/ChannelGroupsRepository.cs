using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.ChannelGroupsDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class ChannelGroupsRepository : IChannelGroupsRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public ChannelGroupsRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateChannelGroups(CreateChannelGroupsDTO channelGroupsDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblchgrchn (chgrid, chid)" +
                    "VALUES (@Chgrid, @Chid)",
            new
            {
                Chgrid = channelGroupsDTO.chgrid,
                Chid = channelGroupsDTO.chid
            });

            return affected != 0;
        }

        public async Task<IEnumerable<ChannelGroupsDTO>> GetAllChannelGroupsByChgrid(int chgrid)
        {
            using var connection = _context.GetConnection();

            var channelGroups = await connection.QueryAsync<ChannelGroups>
                ("SELECT * FROM tblchgrchn WHERE chgrid = @Chgrid", new { Chgrid = chgrid });

            return _mapper.Map<IEnumerable<ChannelGroupsDTO>>(channelGroups);
        }

        public async Task<IEnumerable<ChannelGroupsDTO>> GetAllChannelGroupsByChid(int chid)
        {
            using var connection = _context.GetConnection();

            var channelGroups = await connection.QueryAsync<ChannelGroups>
                ("SELECT * FROM tblchgrchn WHERE chid = @Chid", new { Chid = chid });

            return _mapper.Map<IEnumerable<ChannelGroupsDTO>>(channelGroups);
        }

        public async Task<ChannelGroupsDTO> GetChannelGroupsByIds(int chgrid, int chid)
        {
            using var connection = _context.GetConnection();

            var channelGroups = await connection.QueryFirstOrDefaultAsync<ChannelGroups>(
                "SELECT * FROM tblchgrchn WHERE chgrid = @Chgrid AND chid = @Chid",
                new
                {
                    Chgrid = chgrid,
                    Chid = chid
                });

            return _mapper.Map<ChannelGroupsDTO>(channelGroups);
        }

        public async Task<IEnumerable<ChannelGroupsDTO>> GetAllChannelGroups()
        {
            using var connection = _context.GetConnection();

            var channelGroups = await connection.QueryAsync<ChannelGroups>
                ("SELECT * FROM tblchgrchn ");

            return _mapper.Map<IEnumerable<ChannelGroupsDTO>>(channelGroups);
        }

        public async Task<bool> UpdateChannelGroups(UpdateChannelGroupsDTO channelGroupsDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblchgrchn SET chgrid = @Chgrid, chid = @Chid" +
                "WHERE chgrid = @Chgrid AND chid = @Chid",
                new
                {
                    Chgrid = channelGroupsDTO.chgrid,
                    Chid = channelGroupsDTO.chid
                });

            return affected != 0;
        }

        public async Task<bool> DeleteChannelGroupsByChgrid(int chgrid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblchgrchn WHERE chgrid = @Chgrid",
                new
                {
                    Chgrid = chgrid
                });

            return affected != 0;
        }

        public async Task<bool> DeleteChannelGroupsByChid(int chid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblchgrchn WHERE chid = @Chid",
                new
                {
                    Chid = chid
                });

            return affected != 0;
        }

        public async Task<bool> DeleteChannelGroupsByIds(int chgrid, int chid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblchgrchn WHERE chgrid = @Chgrid AND chid = @Chid",
            new
            {
                    Chgrid = chgrid,
                    Chid = chid
                });

            return affected != 0;
        }
    }
}
