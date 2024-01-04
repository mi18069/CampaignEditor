using Database.DTOs.CampaignDTO;
using CampaignEditor.Entities;
using AutoMapper;
using Dapper;
using Database.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class CampaignRepository : ICampaignRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public CampaignRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateCampaign(CreateCampaignDTO campaignDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO tblcampaigns (cmprev, cmpown, cmpname, clid, cmpsdate, cmpedate, cmpstime, cmpetime, " +
                    "cmpstatus, sostring, activity, cmpaddedon, cmpaddedat, active, forcec)" +
                    " VALUES (@cmprev, @cmpown, @cmpname, @clid, @cmpsdate, @cmpedate, @cmpstime, @cmpetime, " +
                        "@cmpstatus, @sostring, @activity, @cmpaddedon, @cmpaddedat, @active, @forcec)",
            new
            {
                campaignDTO.cmprev,
                campaignDTO.cmpown,
                campaignDTO.cmpname,
                campaignDTO.clid,
                campaignDTO.cmpsdate,
                campaignDTO.cmpedate,
                campaignDTO.cmpstime,
                campaignDTO.cmpetime,
                campaignDTO.cmpstatus,
                campaignDTO.sostring,
                campaignDTO.activity,
                campaignDTO.cmpaddedon,
                campaignDTO.cmpaddedat,
                campaignDTO.active,
                campaignDTO.forcec
            });

            return affected != 0;
        }

        public async Task<CampaignDTO> GetCampaignById(int cmpid)
        {
            using var connection = _context.GetConnection();

            var campaign = await connection.QueryFirstOrDefaultAsync<Campaign>(
                "SELECT * FROM tblcampaigns WHERE cmpid = @cmpid", new { cmpid = cmpid });

            return _mapper.Map<CampaignDTO>(campaign);
        }

        public async Task<CampaignDTO> GetCampaignByName(string cmpname)
        {
            using var connection = _context.GetConnection();

            var campaign = await connection.QueryFirstOrDefaultAsync<Campaign>(
                "SELECT * FROM tblcampaigns WHERE cmpname = @cmpname", new { cmpname = cmpname });

            return _mapper.Map<CampaignDTO>(campaign);
        }

        public async Task<IEnumerable<CampaignDTO>> GetAllCampaigns()
        {
            using var connection = _context.GetConnection();

            var allCampaigns = await connection.QueryAsync<Campaign>("SELECT * FROM tblcampaigns");

            return _mapper.Map<IEnumerable<CampaignDTO>>(allCampaigns);
        }

        public async Task<IEnumerable<CampaignDTO>> GetCampaignsByClientId(int clid)
        {
            using var connection = _context.GetConnection();

            var allCampaigns = await connection.QueryAsync<Campaign>("SELECT * FROM tblcampaigns " +
                                                                 "WHERE clid = @clid", new { clid = clid });

            return _mapper.Map<IEnumerable<CampaignDTO>>(allCampaigns);
        }

        public async Task<bool> UpdateCampaign(UpdateCampaignDTO campaignDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblcampaigns SET cmprev = @cmprev, cmpown = @cmpown, cmpname = @cmpname, clid = @clid, cmpsdate = @cmpsdate, " +
                    "cmpedate = @cmpedate, cmpstime = @cmpstime, cmpetime = @cmpetime, cmpstatus = @cmpstatus, sostring = @sostring, " +
                    "activity = @activity, cmpaddedon = @cmpaddedon, cmpaddedat = @cmpaddedat, active = @active, forcec = @forcec " +
                    "WHERE cmpid = @cmpid",
            new
            {
                campaignDTO.cmpid,
                campaignDTO.cmprev,
                campaignDTO.cmpown,
                campaignDTO.cmpname,
                campaignDTO.clid,
                campaignDTO.cmpsdate,
                campaignDTO.cmpedate,
                campaignDTO.cmpstime,
                campaignDTO.cmpetime,
                campaignDTO.cmpstatus,
                campaignDTO.sostring,
                campaignDTO.activity,
                campaignDTO.cmpaddedon,
                campaignDTO.cmpaddedat,
                campaignDTO.active,
                campaignDTO.forcec
            });

            return affected != 0;
        }

        public async Task<bool> DeleteCampaignById(int cmpid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblcampaigns WHERE cmpid = @cmpid", new { cmpid = cmpid });

            return affected != 0;
        }

        public async Task<bool> DeleteCampaignsByUserId(int userid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblcampaigns WHERE clid = @clid", new { clid = userid });

            return affected != 0;
        }

    }
}
