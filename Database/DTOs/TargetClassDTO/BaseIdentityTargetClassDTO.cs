
namespace Database.DTOs.TargetClassDTO
{
    public class BaseIdentityTargetClassDTO : BaseTargetClassDTO
    {
        public BaseIdentityTargetClassDTO(string name, int type, string position) 
            : base(name, type, position)
        {
        }

        public int classid { get; set; }
    }
}
