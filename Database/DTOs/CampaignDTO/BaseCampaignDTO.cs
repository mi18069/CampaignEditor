using System;

namespace CampaignEditor.DTOs.CampaignDTO
{

    public class BaseCampaignDTO
    {
        public BaseCampaignDTO(int cmprev, int cmpown, string cmpname, int clid, string cmpsdate, string cmpedate, 
            string cmpstime, string cmpetime, int cmpstatus, string sostring, int activity, int cmpaddedon, 
            int cmpaddedat, bool active, bool forcec)
        {
            this.cmprev = cmprev;
            this.cmpown = cmpown;
            this.cmpname = cmpname ?? throw new ArgumentNullException(nameof(cmpname));
            this.clid = clid;
            this.cmpsdate = cmpsdate ?? throw new ArgumentNullException(nameof(cmpsdate));
            this.cmpedate = cmpedate ?? throw new ArgumentNullException(nameof(cmpedate));
            this.cmpstime = cmpstime ?? throw new ArgumentNullException(nameof(cmpstime));
            this.cmpetime = cmpetime ?? throw new ArgumentNullException(nameof(cmpetime));
            this.cmpstatus = cmpstatus;
            this.sostring = sostring ?? throw new ArgumentNullException(nameof(sostring));
            this.activity = activity;
            this.cmpaddedon = cmpaddedon;
            this.cmpaddedat = cmpaddedat;
            this.active = active;
            this.forcec = forcec;
        }

        public int cmprev { get; set; }
        public int cmpown { get; set; }
        public string cmpname { get; set; }
        public int clid { get; set; }
        public string cmpsdate { get; set; }
        public string cmpedate { get; set; }
        public string cmpstime { get; set; }
        public string cmpetime { get; set; }
        public int cmpstatus { get; set; }
        public string sostring { get; set; }
        public int activity { get; set; }
        public int cmpaddedon { get; set; }
        public int cmpaddedat { get; set; }
        public bool active { get; set; }
        public bool forcec { get; set; }
    }

       
}
