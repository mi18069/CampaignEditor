
namespace Database.Entities
{
    public class Prices
    {
        public int prcid { get; set; }
        public int plid { get; set; }
        public string dps { get; set; }
        public string dpe { get; set; }
        public decimal price { get; set; }
        public bool ispt { get; set; }
        public string days { get; set; }
    }
}
