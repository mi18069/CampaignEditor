using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.BrandDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class BrandRepository : IBrandRepository
    {

        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public BrandRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateBrand(CreateBrandDTO brandDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "INSERT INTO brand (brand, aktivan, brgrbrand, brclass) " +
                    "VALUES (@Brand, @Active, @Brgrbrand, @Brclass)",
            new
            {
                Brand = brandDTO.brand,
                Active = brandDTO.active,
                Brgrbrand = brandDTO.brgrbrand,
                Brclass = brandDTO.brclass
            });

            return affected != 0;
        }

        public async Task<BrandDTO> GetBrandById(int id)
        {
            using var connection = _context.GetConnection();

            var brand = await connection.QueryAsync<dynamic>(
                "SELECT * FROM brand WHERE brbrand = @Id", new { Id = id });

            brand = brand.Select(item => new Brand()
            {
                brbrand = item.brbrand,
                brand = item.brand,
                active = item.aktivan,
                brgrbrand = item.brgrbrand,
                brclass = item.brclass
            });

            return _mapper.Map<BrandDTO>(brand.FirstOrDefault());
        }

        public async Task<BrandDTO> GetBrandByName(string brandname)
        {
            using var connection = _context.GetConnection();

            var brand = await connection.QueryAsync<dynamic>(
                "SELECT * FROM brand WHERE brand = @Brand", new { Brand = brandname });

            brand = brand.Select(item => new Brand()
            {
                brbrand = item.brbrand,
                brand = item.brand,
                active = item.aktivan,
                brgrbrand = item.brgrbrand,
                brclass = item.brclass
            });

            return _mapper.Map<BrandDTO>(brand.FirstOrDefault());
        }

        public async Task<IEnumerable<BrandDTO>> GetAllBrands()
        {
            using var connection = _context.GetConnection();

            var allBrands = await connection.QueryAsync<dynamic>("SELECT * FROM brand");

            allBrands = allBrands.Select(item => new Brand()
            {
                brbrand = item.brbrand,
                brand = item.brand,
                active = item.aktivan,
                brgrbrand = item.brgrbrand,
                brclass = item.brclass
            });

            return _mapper.Map<IEnumerable<BrandDTO>>(allBrands);
        }

        public async Task<bool> UpdateBrand(UpdateBrandDTO brandDTO)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "UPDATE brand SET brbrand = @Brbrand, brand = @Brand, aktivan = @Active" +
                " brgrbrand = @Brgrbrand, brclass = @Brclass" +
                " WHERE brbrand = @Brbrand",
                new
                {
                    Brbrand = brandDTO.brbrand,
                    Brand = brandDTO.brand,
                    Active = brandDTO.active,
                    Brgrbrand = brandDTO.brgrbrand,
                    Brclass = brandDTO.brclass
                });

            return affected != 0;
        }

        public async Task<bool> DeleteBrandById(int id)
        {
            using var connection = _context.GetConnection();

            var affected = await connection.ExecuteAsync(
                "DELETE FROM brand WHERE brbrand = @Brbrand", new { Brbrand = id });

            return affected != 0;
        }
    }
}
