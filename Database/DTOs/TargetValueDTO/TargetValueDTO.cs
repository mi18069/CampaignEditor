

namespace Database.DTOs.TargetValueDTO
{
    public class TargetValueDTO : BaseIdentityTargetValueDTO
    {
        public TargetValueDTO(int id, string name, string value) 
            : base(id, name, value)
        {
        }
    }
}
