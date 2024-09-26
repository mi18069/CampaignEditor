using Database.DTOs.ChannelDTO;
using System.Collections.Generic;
using System.Linq;

namespace CampaignEditor.Entities
{
    public class ChannelSpotModel
    {
        public ChannelDTO Channel { get; set; }
        public Dictionary<char, decimal> SpotCoefficients { get; set; }
        /*
         * Status
         * -1 - don't do anything, default at initialization
         * 0 - don't do anything, marking when Initializing to show that we have this element in database
         * 1 - create in database
         * 2 - update in database
         * 3 - delete from database
        */
        private Dictionary<char, int> _statuses;
        public Dictionary<char, int> Statuses { get { return _statuses; } }

        public ChannelSpotModel(ChannelDTO channel, List<char> spotcodes)
        {
            Channel = channel;
            SpotCoefficients = spotcodes.ToDictionary(sc => sc, sc => 1.0m); // Default coefficient is 1.0
            _statuses = spotcodes.ToDictionary(sc => sc, sc => -1);
        }

        public void InitializeCoef(char spotcode, decimal value)
        {
            SpotCoefficients[spotcode] = value;
            _statuses[spotcode] = 0;
        }
        public void SetCoef(char spotcode, decimal value)
        {
            SpotCoefficients[spotcode] = value;
            
            if (value != 1.0M)
            {
                switch (_statuses[spotcode])
                {
                    case -1: // not in database
                        _statuses[spotcode] = 1; break; // create
                    case 0: // already in database
                        _statuses[spotcode] = 2; break; // update
                    default: break;
                }
            }
            else
            {
                switch (_statuses[spotcode])
                {
                    case -1: // not in database
                        break; // pass
                    case 0: // already in database
                        _statuses[spotcode] = 3; break; // delete
                    case 1: // not in database
                        _statuses[spotcode] = -1; break; // mark as initial
                    case 2: // in database
                        _statuses[spotcode] = 3; break; // delete
                    default: break;
                }
            }

        }
    }
}
