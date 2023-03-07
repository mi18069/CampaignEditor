
namespace Database.DTOs.PricesDTO
{
    public class PricesDTO : BaseIdentityPricesDTO
    {
        public PricesDTO(int prcid, string plid, string dps, string dpe, float price, bool ispt) 
            : base(prcid, plid, dps, dpe, price, ispt)
        {
        }
    }
}
