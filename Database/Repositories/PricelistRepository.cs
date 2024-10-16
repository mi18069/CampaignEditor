﻿using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.MediaPlanDTO;
using Database.DTOs.PricelistDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class PricelistRepository : IPricelistRepository
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public PricelistRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<int> CreatePricelist(CreatePricelistDTO pricelistDTO)
        {
            using var connection = _context.GetConnection();

            var result = await connection.ExecuteScalarAsync<int>(
                "INSERT INTO tblpricelist (clid, plname, pltype, sectbid, seastbid, plactive, price, minprice, " +
                "prgcoef, pltarg, use2, sectbid2, sectb2st, sectb2en, valfrom, valto, mgtype, fixprice) " +
                    "VALUES (@Clid, @Plname, @Pltype, @Sectbid, @Seastbid, @Plactive, @Price, @Minprice, " +
                    "@Prgcoef, @Pltarg, @Use2, @Sectbid2, @Sectb2st, @Sectb2en, @Valfrom, @Valto, @Mgtype, @Fixprice) " +
                    "RETURNING plid",
            new
            {
                Clid = pricelistDTO.clid,
                Plname = pricelistDTO.plname,
                Pltype = pricelistDTO.pltype,
                Sectbid = pricelistDTO.sectbid,
                Seastbid = pricelistDTO.seastbid,
                Plactive = pricelistDTO.plactive,
                Price = pricelistDTO.price,
                Minprice = pricelistDTO.minprice,
                Prgcoef = pricelistDTO.prgcoef,
                Pltarg = pricelistDTO.pltarg,
                Use2 = pricelistDTO.use2,
                Sectbid2 = pricelistDTO.sectbid2,
                Sectb2st = pricelistDTO.sectb2st,
                Sectb2en = pricelistDTO.sectb2en,
                Valfrom = pricelistDTO.valfrom,
                Valto = pricelistDTO.valto,
                Mgtype = pricelistDTO.mgtype,
                Fixprice = pricelistDTO.fixprice
            });


            if (result != null)
            {
                // Successfully inserted, 'result' contains the newly inserted MediaPlan with Id
                return result;
            }
            else
            {
                // Insertion failed, handle accordingly (e.g., throw an exception)
                return -1;
            }
        }

        public async Task<bool> IsPricelistInUse(int plid)
        {
            using var connection = _context.GetConnection();

            string query = "";


            var row = await connection.QueryAsync<int>(
            @"
            SELECT COUNT(*) 
            FROM tblcmpchn 
            WHERE plid = @Plid
            ", new { Plid = plid });

            // Check if any rows were returned
            bool hasRows = row.FirstOrDefault() > 0;

            return hasRows;
        }

        public async Task<PricelistDTO> GetPricelistById(int id)
        {
            using var connection = _context.GetConnection();

            var pricelist = await connection.QueryFirstOrDefaultAsync<Pricelist>(
                "SELECT * FROM tblpricelist WHERE plid = @Id", new { Id = id });

            return _mapper.Map<PricelistDTO>(pricelist);
        }
        public async Task<PricelistDTO> GetPricelistByName(string pricelistname)
        {
            using var connection = _context.GetConnection();

            var pricelist = await connection.QueryFirstOrDefaultAsync<Pricelist>(
                "SELECT * FROM tblpricelist WHERE plname = @Plname", new { Plname = pricelistname });

            return _mapper.Map<PricelistDTO>(pricelist);
        }
        public async Task<PricelistDTO> GetClientPricelistByName(int clid, string pricelistname)
        {
            using var connection = _context.GetConnection();

            var pricelist = await connection.QueryFirstOrDefaultAsync<Pricelist>(
                "SELECT * FROM tblpricelist WHERE plname = @Plname AND (clid = @Clid OR clid = 0)", 
                new { Plname = pricelistname, Clid = clid});

            return _mapper.Map<PricelistDTO>(pricelist);
        }
        public async Task<IEnumerable<PricelistDTO>> GetAllPricelists()
        {
            using var connection = _context.GetConnection();

            var allPricelists = await connection.QueryAsync<Pricelist>
                ("SELECT * FROM tblpricelist");

            return _mapper.Map<IEnumerable<PricelistDTO>>(allPricelists);
        }
        public async Task<IEnumerable<PricelistDTO>> GetAllClientPricelists(int clid)
        {
            using var connection = _context.GetConnection();

            var allPricelists = await connection.QueryAsync<Pricelist>
                ("SELECT * FROM tblpricelist WHERE clid = @Clid OR clid = 0", new { Clid = clid});

            return _mapper.Map<IEnumerable<PricelistDTO>>(allPricelists);
        }

        public async Task<bool> UpdatePricelist(UpdatePricelistDTO pricelistDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE tblpricelist SET clid = @Clid, plname = @Plname, pltype = @Pltype, " +
                "sectbid = @Sectbid, seastbid = @Seastbid, plactive = @Plactive, price = @Price, minprice = @Minprice, " +
                "prgcoef = @Prgcoef, pltarg = @Pltarg, use2 = @Use2, sectbid2 = @Sectbid2, " +
                "sectb2st = @Sectb2st, sectb2en = @Sectb2en, valfrom = @Valfrom, valto = @Valto, mgtype = @Mgtype, fixprice = @Fixprice " +
                "WHERE plid = @Plid",
                new
                {
                    Plid = pricelistDTO.plid,
                    Clid = pricelistDTO.clid,
                    Plname = pricelistDTO.plname,
                    Pltype = pricelistDTO.pltype,
                    Sectbid = pricelistDTO.sectbid,
                    Seastbid = pricelistDTO.seastbid,
                    Plactive = pricelistDTO.plactive,
                    Price = pricelistDTO.price,
                    Minprice = pricelistDTO.minprice,
                    Prgcoef = pricelistDTO.prgcoef,
                    Pltarg = pricelistDTO.pltarg,
                    Use2 = pricelistDTO.use2,
                    Sectbid2 = pricelistDTO.sectbid2,
                    Sectb2st = pricelistDTO.sectb2st,
                    Sectb2en = pricelistDTO.sectb2en,
                    Valfrom = pricelistDTO.valfrom,
                    Valto = pricelistDTO.valto,
                    Mgtype = pricelistDTO.mgtype,
                    Fixprice = pricelistDTO.fixprice
                });

            return affected != 0;
        }
        public async Task<bool> DeletePricelistById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM tblpricelist WHERE plid = @Plid", new { Plid = id });

            return affected != 0;
        }
    }
}
