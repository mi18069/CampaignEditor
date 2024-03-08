namespace Database.DTOs.BrandDTO
{
    public class BaseBrandDTO
    {
        public BaseBrandDTO(string brand, bool? active, int brgrbrand, int brclass)
        {
            this.brand = brand;
            this.active = active;
            this.brgrbrand = brgrbrand;
            this.brclass = brclass;
        }

        public string brand { get; set; }
        public bool? active { get; set; }
        public int brgrbrand { get; set; }
        public int brclass { get; set; }
    }
}
