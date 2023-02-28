
namespace Database.Entities
{
    public class Sectable
    {
        public int sctid { get; set; }
        public string sctname { get; set; }
        public bool sctlinear { get; set; }
        public bool sctactive { get; set; }
        public int ownedby { get; set; }
        
    }
}
