using CampaignEditor.DTOs.UserDTO;
using System;
using System.Runtime.CompilerServices;

namespace CampaignEditor.Entities
{
    public class User
    {
        public int usrid { get; set; }
        public string usrname { get; set; }
        public string usrpass { get; set; }
        public int usrlevel { get; set; }
        public string email { get; set; }
        public string telefon { get; set; }
        public int enabled { get; set; }
        public int father { get; set; }
        public int buy { get; set; }

        public bool isAdministrator { get { return usrlevel == 0; }  }
        public bool isReadWrite { get { return usrlevel == 1 ? true : isAdministrator; }  }

    }
}
