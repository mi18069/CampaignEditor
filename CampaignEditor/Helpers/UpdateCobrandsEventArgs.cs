using Database.DTOs.CobrandDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignEditor.Helpers
{
    public class UpdateCobrandsEventArgs
    {
        public IEnumerable<CobrandDTO> Cobrands { get; private set; }

        public UpdateCobrandsEventArgs(IEnumerable<CobrandDTO> cobrands)
        {
            Cobrands = cobrands;
        }
    }
}
