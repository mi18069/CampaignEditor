
namespace Database.DTOs.TargetValueDTO
{
    public class UpdateTargetValueDTO : BaseIdentityTargetValueDTO
    {
        public UpdateTargetValueDTO(int id, string name, string value) 
            : base(id, name, value)
        {
        }
    }
}
