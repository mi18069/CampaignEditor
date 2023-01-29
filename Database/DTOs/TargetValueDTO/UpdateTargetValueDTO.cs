
namespace Database.DTOs.TargetValueDTO
{
    public class UpdateTargetValueDTO : BaseIdentityTargetValueDTO
    {
        public UpdateTargetValueDTO(string name, int value) 
            : base(name, value)
        {
        }
    }
}
