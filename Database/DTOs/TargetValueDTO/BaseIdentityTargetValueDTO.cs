
namespace Database.DTOs.TargetValueDTO
{
    public class BaseIdentityTargetValueDTO : BaseTargetValueDTO
    {
        public BaseIdentityTargetValueDTO(string name, int value) 
            : base(name, value)
        {
        }

        public int id { get; set; }
    }
}
