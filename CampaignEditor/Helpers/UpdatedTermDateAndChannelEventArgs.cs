using Database.DTOs.ChannelDTO;
using System;

namespace CampaignEditor.Helpers
{
    public class UpdatedTermDateAndChannelEventArgs
    {
        public DateOnly Date { get; private set; }
        public ChannelDTO Channel { get; private set; }

        public UpdatedTermDateAndChannelEventArgs(DateOnly date, ChannelDTO channel)
        {
            Date = date;
            Channel = channel;
        }
    }
}
