using Database.Entities;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IDGConfigRepository
    {
        Task<bool> CreateDGConfig(DGConfig dgConfig);
        Task<DGConfig> GetDGConfig(int usrid, int clid);
        Task<bool> UpdateDGConfig(DGConfig dgConfig);
        Task<bool> DeleteDGConfigByUsrid(int usrid);
        Task<bool> DeleteDGConfigByClid(int clid);
        Task<bool> DeleteDGConfig(int usrid, int clid);

    }
}
