
namespace Database.Entities
{
    public class Spot
    {
        public int cmpid { get; set; }
        public string spotcode { get; set; }
        public string spotname { get; set; }

        public int spotlength { get; set; }
        public bool ignore { get; set; }
    }
}
