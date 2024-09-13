using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.RealizedSpotDTO;
using Database.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class RealizedSpotRepository : IRealizedSpotRepository
    {

        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public RealizedSpotRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<RealizedSpotDTO> GetRealizedSpot(int spotnum)
        {
            using var connection = _context.GetConnection();

            var realizedSpot = await connection.QueryAsync<dynamic>(
                "SELECT * FROM spotovi WHERE brreklame = @Spotnum", new { Spotnum = spotnum });

            realizedSpot = realizedSpot.Select(item => new RealizedSpot()
            {
                spotnum = item.brreklame,
                brandnum = item.brbrand,
                row = item.redreklame,
                spotname = item.nazreklame.Trim(),
                spotlength = decimal.ToInt32(Math.Floor(item.duzina)),
                active = item.aktivan,
                variant = item.varijanta,
                firstdate = item.prvidatum
            });

            return _mapper.Map<RealizedSpotDTO>(realizedSpot.FirstOrDefault());
        }
    }
}
