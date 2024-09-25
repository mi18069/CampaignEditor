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
        private int _status = -1;
        public int Status { get { return _status; } }

        public ChannelSpotModel(ChannelDTO channel, List<char> spotcodes)
        {
            Channel = channel;
            SpotCoefficients = spotcodes.ToDictionary(sc => sc, sc => 1.0m); // Default coefficient is 1.0
        }

        public void InitializeCoef(char spotcode, decimal value)
        {
            SpotCoefficients[spotcode] = value;
            _status = 0;
        }
        public void SetCoef(char spotcode, decimal value)
        {
            SpotCoefficients[spotcode] = value;
            
            if (value != 1.0M)
            {
                switch (_status)
                {
                    case -1: // not in database
                        _status = 1; break; // create
                    case 0: // already in database
                        _status = 2; break; // update
                    default: break;
                }
            }
            else
            {
                switch (_status)
                {
                    case -1: // not in database
                        break; // pass
                    case 0: // already in database
                        _status = 3; break; // delete
                    case 1: // not in database
                        _status = -1; break; // mark as initial
                    case 2: // in database
                        _status = 3; break; // delete
                    default: break;
                }
            }

        }
    }
}
