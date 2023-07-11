namespace Database.DTOs.BrandDTO
{
    public class UpdateBrandDTO : BaseIdentityBrandDTO
    {
        public UpdateBrandDTO(string brand, bool? active, int brgrbrand, int brclass) 
            : base(brand, active, brgrbrand, brclass)
        {
        }
    }
}
