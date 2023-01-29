
namespace Database.DTOs.TargetClassDTO
{
    public class UpdateTargetClassDTO : BaseIdentityTargetClassDTO
    {
        public UpdateTargetClassDTO(string name, int type, string position) 
            : base(name, type, position)
        {
        }
    }
}
