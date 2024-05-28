
using System.Runtime.CompilerServices;

namespace Database.Entities
{
    public class MediaPlanRealizedEmpty : MediaPlanRealized
    {
        public MediaPlanRealizedEmpty()
        {
            this.status = -1;
        }
    }
}
