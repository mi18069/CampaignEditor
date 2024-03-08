using AutoMapper;
using Dapper;
using Database.Data;
using Database.DTOs.ChannelCmpDTO;
using Database.DTOs.EmsTypesDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class EmsTypesRepository : IEmsTypesRepository
    {

        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public EmsTypesRepository(IDataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<EmsTypesDTO> GetEmsTypesByCode(string code)
        {
            using var connection = _context.GetConnection();

            var emsTypes = await connection.QueryFirstOrDefaultAsync<EmsTypes>(
                "SELECT * FROM tbltypo WHERE typocode = @Typocode",
                new { Typocode = code });

            return _mapper.Map<EmsTypesDTO>(emsTypes);
        }

        public async Task<IEnumerable<EmsTypesDTO>> GetAllEmsTypes()
        {
            using var connection = _context.GetConnection();

            var allEmsTypes = await connection.QueryAsync<EmsTypes>("SELECT * FROM tbltypo ");

            return _mapper.Map<IEnumerable<EmsTypesDTO>>(allEmsTypes);
        }

        public async Task<IEnumerable<EmsTypes>> GetAllEmsTypesEntities()
        {
            using var connection = _context.GetConnection();

            var allEmsTypes = await connection.QueryAsync<EmsTypes>("SELECT * FROM tbltypo ");

            return allEmsTypes;
        }
    }
}
