
namespace Database.DTOs.TargetClassDTO
{
    public class UpdateTargetClassDTO : BaseIdentityTargetClassDTO
    {
        public UpdateTargetClassDTO(string name, string type, string position) 
            : base(name, type, position)
        {
        }
    }
}
