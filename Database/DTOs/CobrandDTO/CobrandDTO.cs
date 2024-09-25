namespace Database.DTOs.CobrandDTO
{
    public class CobrandDTO : BaseCobrandDTO
    {
        public CobrandDTO(int cmpid, int chid, char spotcode, decimal coef) 
            : base(cmpid, chid, spotcode, coef)
        {
        }
    }
}
