
namespace Database.DTOs.TargetValueDTO
{
    public class UpdateTargetValueDTO : BaseIdentityTargetValueDTO
    {
        public UpdateTargetValueDTO(string name, string value) 
            : base(name, value)
        {
        }
    }
}
