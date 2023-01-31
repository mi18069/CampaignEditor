using System;

namespace Database.DTOs.TargetClassDTO
{
    public class BaseTargetClassDTO
    {
        public BaseTargetClassDTO(string name, string type, string position)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.type = type;
            this.position = position ?? throw new ArgumentNullException(nameof(position));
        }

        public string name { get; set; }
        public string type { get; set; }
        public string position { get; set; }
    }
}
