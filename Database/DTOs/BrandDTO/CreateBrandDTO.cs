namespace Database.DTOs.BrandDTO
{
    public class CreateBrandDTO : BaseBrandDTO
    {
        public CreateBrandDTO(string brand, bool? active, int brgrbrand, int brclass) 
            : base(brand, active, brgrbrand, brclass)
        {
        }
    }
}
