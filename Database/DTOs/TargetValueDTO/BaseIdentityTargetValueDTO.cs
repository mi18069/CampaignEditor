
namespace Database.DTOs.TargetValueDTO
{
    public class BaseIdentityTargetValueDTO : BaseTargetValueDTO
    {
        public BaseIdentityTargetValueDTO(int id, string name, string value) 
            : base(name, value)
        {
            this.id = id;
        }

        public int id { get; set; }
    }
}
