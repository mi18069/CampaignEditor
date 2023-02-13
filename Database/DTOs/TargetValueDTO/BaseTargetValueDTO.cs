using System;

namespace Database.DTOs.TargetValueDTO
{
    public class BaseTargetValueDTO
    {
        public BaseTargetValueDTO(string name, string value)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.value = value;
        }

        public string name { get; set; }
        public string value { get; set; }
    }
}
