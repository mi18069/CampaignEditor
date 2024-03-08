namespace Database.DTOs.EmsTypesDTO
{
    public class BaseEmsTypesDTO
    {
        public BaseEmsTypesDTO(string typocode, string typoname)
        {
            this.typocode = typocode;
            this.typoname = typoname;
        }

        public string typocode { get; set; }
        public string typoname { get; set; }
    }
}
