namespace Database.Entities
{
    public class TermCoefs
    {
        public decimal Seccoef { get; set; }
        public decimal Seascoef { get; set; }
        public decimal Price { get; set; }
        public decimal? Cpp { get; set; }
        public decimal? Amrpsale { get; set; }

        public TermCoefs()
        {
            Cpp = null;
            Amrpsale = null;
        }

        public TermCoefs(decimal amrpsale, decimal cpp)
        {
            Amrpsale = amrpsale;
            Cpp = cpp;
        }
    }
}
