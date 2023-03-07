
namespace Database.DTOs.PricesDTO
{
    public class UpdatePricesDTO : BaseIdentityPricesDTO
    {
        public UpdatePricesDTO(int prcid, string plid, string dps, string dpe, float price, bool ispt) 
            : base(prcid, plid, dps, dpe, price, ispt)
        {
        }
    }
}
