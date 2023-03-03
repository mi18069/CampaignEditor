using System;

namespace Database.DTOs.ActivityDTO
{
    public class BaseActivityDTO
    {
        public BaseActivityDTO(string act)
        {
            this.act = act ?? throw new ArgumentNullException(nameof(act));
        }

        public string act { get; set; }
    }
}
