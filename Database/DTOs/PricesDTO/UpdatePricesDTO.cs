﻿
namespace Database.DTOs.PricesDTO
{
    public class UpdatePricesDTO : BaseIdentityPricesDTO
    {
        public UpdatePricesDTO(int prcid, int plid, string dps, string dpe, decimal price, bool ispt, string days) 
            : base(prcid, plid, dps, dpe, price, ispt, days)
        {
        }
    }
}
