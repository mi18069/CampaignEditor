﻿
namespace Database.DTOs.PricesDTO
{
    public class CreatePricesDTO : BasePricesDTO
    {
        public CreatePricesDTO(int plid, string dps, string dpe, decimal price, bool ispt, string days) 
            : base(plid, dps, dpe, price, ispt, days)
        {
        }
    }
}
