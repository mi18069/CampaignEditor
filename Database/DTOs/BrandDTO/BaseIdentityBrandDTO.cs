namespace Database.DTOs.BrandDTO
{
    public class BaseIdentityBrandDTO : BaseBrandDTO
    {
        public BaseIdentityBrandDTO(string brand, bool? active, int brgrbrand, int brclass) 
            : base(brand, active, brgrbrand, brclass)
        {
        }

        public int brbrand { get; set; }

    }
}
