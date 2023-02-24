
namespace Database.Entities
{
    public class Seasonality
    {
        public int seasid { get; set; }
        public string seasname { get; set; }
        public bool seasactive { get; set; }
        public string channel { get; set; }
        public int ownedby { get; set; }
    }
}
