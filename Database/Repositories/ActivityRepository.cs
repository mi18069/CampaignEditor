using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.ActivityDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        public ActivityRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateActivity(CreateActivityDTO activityDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblactivity (act)" +
                    "VALUES (@Act)",
            new
            {
                Act = activityDTO.act
            });

            return affected != 0;
        }


        public async Task<ActivityDTO> GetActivityById(int id)
        {
            using var connection = _context.GetConnection();

            var activity = await connection.QueryFirstOrDefaultAsync<Activity>(
                "SELECT * FROM tblactivity WHERE actid = @Id", new { Id = id });

            return _mapper.Map<ActivityDTO>(activity);
        }

        public async Task<ActivityDTO> GetActivityByName(string name)
        {
            using var connection = _context.GetConnection();

            var activity = await connection.QueryFirstOrDefaultAsync<Activity>(
                "SELECT * FROM tblactivity WHERE actname = @Actname", new { Actname = name });

            return _mapper.Map<ActivityDTO>(activity);
        }

        public async Task<IEnumerable<ActivityDTO>> GetAllActivities()
        {
            using var connection = _context.GetConnection();

            var allActivities = await connection.QueryAsync<Activity>("SELECT * FROM tblactivity");

            return _mapper.Map<IEnumerable<ActivityDTO>>(allActivities);
        }

        public async Task<bool> UpdateActivity(UpdateActivityDTO activityDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblactivity SET actid = @Actid, act = @Act " +
                "WHERE actid = @Actid",
            new
            {
                    Actid = activityDTO.actid,
                    Sctname = activityDTO.act
                });

            return affected != 0;
        }
        public async Task<bool> DeleteActivityById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblactivity WHERE actid = @Actid", new { Actid = id });

            return affected != 0;
        }
    }
}
