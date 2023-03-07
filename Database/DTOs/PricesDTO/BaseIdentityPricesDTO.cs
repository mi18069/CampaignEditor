
namespace Database.DTOs.PricesDTO
{
    public class BaseIdentityPricesDTO : BasePricesDTO
    {
        public BaseIdentityPricesDTO(int prcid, string plid, string dps, string dpe, float price, bool ispt) 
            : base(plid, dps, dpe, price, ispt)
        {
            this.prcid = prcid;
        }

        public int prcid { get; set; }
    }
}
