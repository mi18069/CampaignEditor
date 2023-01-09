using System;

namespace CampaignEditor.DTOs.UserDTO
{
    public class BaseUserDTO
    {
        public BaseUserDTO(string usrname, string usrpass, int usrlevel, string email, string telefon, int enabled, int father, bool buy)
        {
            this.usrname = usrname ?? throw new ArgumentNullException(nameof(usrname));
            this.usrpass = usrpass ?? throw new ArgumentNullException(nameof(usrpass));
            this.usrlevel = usrlevel;
            this.email = email ?? throw new ArgumentNullException(nameof(email));
            this.telefon = telefon ?? throw new ArgumentNullException(nameof(telefon));
            this.enabled = enabled;
            this.father = father;
            this.buy = buy;
        }

        public string usrname { get; set; }
        public string usrpass { get; set; }
        public int usrlevel { get; set; }
        public string email { get; set; }
        public string telefon { get; set; }
        public int enabled { get; set; }
        public int father { get; set; }
        public bool buy { get; set; }
    }
}
