namespace Database.DTOs.BrandDTO
{
    public class BrandDTO : BaseIdentityBrandDTO
    {
        public BrandDTO(string brand, bool? active, int brgrbrand, int brclass) 
            : base(brand, active, brgrbrand, brclass)
        {
        }
    }
}
