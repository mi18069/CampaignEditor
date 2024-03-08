using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.ReachDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class ReachRepository : IReachRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public ReachRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<ReachDTO>> GetReachByCmpid(int cmpid)
        {
            using var connection = _context.GetConnection();

            var reach = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xobrrch WHERE cmpid = @Id", new { Id = cmpid });

            reach = reach.Select(item => new Reach()
            {
                id = (int)item.id,
                cmpid = (int)item.cmpid,
                date = item.datum,
                xmpid = (int)item.xmpid,
                channel = (int)item.kanal,
                time = item.vreme,
                universe = item.universe,
                demostr1 = item.demostr1,
                demostr2 = item.demostr2,
                num1 = (int)item.broj1,
                universe1 = item.universe1,
                num2 = (int)item.broj2,
                universe2 = item.universe2,
                ogrp1 = item.ogrp1,
                grp1 = item.grp1,
                rchn1 = item.rchn1,
                rch1 = item.rch1,
                rch12 = item.rch12,
                rch13 = item.rch13,
                rch14 = item.rch14,
                rch15 = item.rch15,
                rch16 = item.rch16,
                rch17 = item.rch17,
                rch18 = item.rch18,
                rch19 = item.rch19,
                ogrp2 = item.ogrp2,
                grp2 = item.grp2,
                rchn2 = item.rchn2,
                rch2 = item.rch2,
                rch22 = item.rch22,
                rch23 = item.rch23,
                rch24 = item.rch24,
                rch25 = item.rch25,
                rch26 = item.rch26,
                rch27 = item.rch27,
                rch28 = item.rch28,
                rch29 = item.rch29,
                live = (int)item.live,
                adi = (int)item.adi,
                mdi = (int)item.mdi,
                adi2 = (int)item.adi2,
                mdi2 = (int)item.mdi2,
                redbrems = (int)item.redbrems
            });

            return _mapper.Map<IEnumerable<ReachDTO>>(reach);
        }

        public async Task<ReachDTO?> GetFinalReachByCmpid(int cmpid)
        {
            using var connection = _context.GetConnection();

            var reach = await connection.QueryAsync<dynamic>(
                "SELECT * FROM xobrrch WHERE cmpid = @Id AND xmpid = 0", new { Id = cmpid });

            try
            {
                reach = reach.Select(item => new Reach()
                {
                    id = (int)item.id,
                    cmpid = (int)item.cmpid,
                    date = item.datum,
                    xmpid = (int)item.xmpid,
                    channel = (int)item.kanal,
                    time = (int)item.vreme,
                    universe = item.universe,
                    demostr1 = item.demostr1,
                    demostr2 = item.demostr2,
                    num1 = (int)item.broj1,
                    universe1 = item.universe1,
                    num2 = (int)item.broj2,
                    universe2 = item.universe2,
                    ogrp1 = item.ogrp1,
                    grp1 = item.grp1,
                    rchn1 = item.rchn1,
                    rch1 = item.rch1,
                    rch12 = item.rch12,
                    rch13 = item.rch13,
                    rch14 = item.rch14,
                    rch15 = item.rch15,
                    rch16 = item.rch16,
                    rch17 = item.rch17,
                    rch18 = item.rch18,
                    rch19 = item.rch19,
                    ogrp2 = item.ogrp2,
                    grp2 = item.grp2,
                    rchn2 = item.rchn2,
                    rch2 = item.rch2,
                    rch22 = item.rch22,
                    rch23 = item.rch23,
                    rch24 = item.rch24,
                    rch25 = item.rch25,
                    rch26 = item.rch26,
                    rch27 = item.rch27,
                    rch28 = item.rch28,
                    rch29 = item.rch29,
                    live = (int)item.live,
                    adi = (int)item.adi,
                    mdi = (int)item.mdi,
                    adi2 = (int)item.adi2,
                    mdi2 = (int)item.mdi2,
                    redbrems = (int?)item.redbrems
                });

                return _mapper.Map<ReachDTO>(reach.FirstOrDefault());
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> DeleteReachByCmpid(int cmpid)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM xmp WHERE cmpid = @Id", new { Id = cmpid });

            return affected != 0;
        }
    }
}
