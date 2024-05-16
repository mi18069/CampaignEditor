﻿
namespace Database.DTOs.PricesDTO
{
    public class PricesDTO : BaseIdentityPricesDTO
    {
        public PricesDTO(int prcid, int plid, string dps, string dpe, decimal price, bool ispt, string days) 
            : base(prcid, plid, dps, dpe, price, ispt, days)
        {
        }
    }
}
