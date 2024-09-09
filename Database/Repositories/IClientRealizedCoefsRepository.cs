using Database.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IClientRealizedCoefsRepository
    {
        Task<bool> CreateRealizedCoefs(ClientRealizedCoefs coefs);
        Task<ClientRealizedCoefs> GetRealizedCoefs(int clid, int emsnum);
        Task<IEnumerable<ClientRealizedCoefs>> GetAllClientRealizedCoefs(int clid);
        Task<bool> UpdateRealizedCoefs(ClientRealizedCoefs coefs);
        Task<bool> DeleteRealizedCoefs(int clid, int emsnum);
    }
}
