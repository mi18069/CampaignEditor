
namespace Database.DTOs.TargetValueDTO
{
    public class BaseIdentityTargetValueDTO : BaseTargetValueDTO
    {
        public BaseIdentityTargetValueDTO(string name, string value) 
            : base(name, value)
        {
        }

        public int id { get; set; }
    }
}
