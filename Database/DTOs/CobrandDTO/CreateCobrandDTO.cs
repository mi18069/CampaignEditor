namespace Database.DTOs.CobrandDTO
{
    public class CreateCobrandDTO : BaseCobrandDTO
    {
        public CreateCobrandDTO(int cmpid, int chid, char spotcode, decimal coef) 
            : base(cmpid, chid, spotcode, coef)
        {
        }
    }
}
