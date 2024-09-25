namespace Database.DTOs.CobrandDTO
{
    public class UpdateCobrandDTO : BaseCobrandDTO
    {
        public UpdateCobrandDTO(int cmpid, int chid, char spotcode, decimal coef) 
            : base(cmpid, chid, spotcode, coef)
        {
        }
    }
}
