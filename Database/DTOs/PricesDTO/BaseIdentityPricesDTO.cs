
namespace Database.DTOs.PricesDTO
{
    public class BaseIdentityPricesDTO : BasePricesDTO
    {
        public BaseIdentityPricesDTO(int prcid, int plid, string dps, string dpe, decimal price, bool ispt, string days) 
            : base(plid, dps, dpe, price, ispt, days)
        {
            this.prcid = prcid;
        }

        public int prcid { get; set; }
    }
}
