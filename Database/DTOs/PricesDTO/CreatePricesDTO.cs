
namespace Database.DTOs.PricesDTO
{
    public class CreatePricesDTO : BasePricesDTO
    {
        public CreatePricesDTO(string plid, string dps, string dpe, float price, bool ispt) 
            : base(plid, dps, dpe, price, ispt)
        {
        }
    }
}
