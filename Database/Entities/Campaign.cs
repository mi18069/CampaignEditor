namespace CampaignEditor.Entities
{
    public class Campaign
    {
        public int cmpid { get; set; }
        public int cmprev { get; set; }
        public int cmpown { get; set; }
        public string cmpname { get; set; }
        public int clid { get; set; }
        public string cmpsdate { get; set; }
        public string cmpedate { get; set; }
        public string cmpstime { get; set; }
        public string cmpetime { get; set; }
        public string cmpstatus { get; set; }
        public string sostring { get; set; }
        public int activity { get; set; }
        public int cmpaddedon { get; set; }
        public int cmpaddedat { get; set; }
        public bool active { get; set; }
        public bool forcec { get; set; }

    }
}
