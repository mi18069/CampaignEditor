
namespace Database.DTOs.TargetClassDTO
{
    public class BaseIdentityTargetClassDTO : BaseTargetClassDTO
    {
        public BaseIdentityTargetClassDTO(string name, string type, string position) 
            : base(name, type, position)
        {
        }

        public int demoid { get; set; }
    }
}
