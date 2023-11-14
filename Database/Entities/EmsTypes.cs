
namespace Database.Entities
{
    public class EmsTypes
    {
        public string typocode { get; set; }
        public string typoname { get; set; }

        public string Concatenated
        {
            get { return typocode + " " + typoname.Trim(); }
        }
    }
}
