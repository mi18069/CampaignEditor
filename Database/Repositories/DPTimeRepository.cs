using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.DayPartDTO;
using Database.DTOs.DPTimeDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class DPTimeRepository : IDPTimeRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        public DPTimeRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<int> CreateDPTime(CreateDPTimeDTO dpTimeDTO)
        {
            using var connection = _context.GetConnection();

            var newId = await connection.QueryFirstOrDefaultAsync<int>(
                "INSERT INTO tbldptime (dpid, stime, etime) " +
                "VALUES (@Dpid, @Stime, @Etime) " +
                "RETURNING dptimeid",
            new
            {
                Dpid = dpTimeDTO.dpid,
                Stime = dpTimeDTO.stime,
                Etime = dpTimeDTO.etime
            });

            return newId;
        }

        public async Task<DPTimeDTO> GetDPTimeById(int id)
        {
            using var connection = _context.GetConnection();

            var dpTime = await connection.QueryFirstOrDefaultAsync<DPTime>(
                "SELECT * FROM tbldptime WHERE dptimeid = @Id", new { Id = id });

            return _mapper.Map<DPTimeDTO>(dpTime);
        }

        public async Task<IEnumerable<DPTimeDTO>> GetAllDPTimesByDPId(int dpId)
        {
            using var connection = _context.GetConnection();

            var allDpTimes = await connection.QueryAsync<DPTime>(
                "SELECT * FROM tbldptime WHERE dpid = @Dpid",
                new { DpId = dpId });

            return _mapper.Map<IEnumerable<DPTimeDTO>>(allDpTimes);
        }



        public async Task<bool> UpdateDPTime(UpdateDPTimeDTO dpTimeDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "Update tbldptime SET dptimeid = @Dptimeid, dpid = @Dpid, " +
                "stime = @Stime, etime = @Etime " +
                "WHERE dptimeid = @Dptimeid",
            new
            {
                Dptimeid = dpTimeDTO.dptimeid,
                Dpid = dpTimeDTO.dpid,
                Stime = dpTimeDTO.stime,
                Etime = dpTimeDTO.etime
            });

            return affected != 0;
        }

        public async Task<bool> DeleteDPTime(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tbldptime WHERE dptimeid = @Id", new { Id = id });

            return affected != 0;
        }

        public async Task<bool> DeleteDPTimeByDPId(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tbldptime WHERE dpid = @Id", new { Id = id });

            return affected != 0;
        }
    }
}
