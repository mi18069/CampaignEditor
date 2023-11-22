using CampaignEditor.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignEditor
{
    public static class CampaignEventLinker
    {
        public static List<Tuple<int, Channels?, CampaignForecast?>> list = new List<Tuple<int, Channels?, CampaignForecast?>>();

        public static void AddCampaign(int cmpid)
        {
            list.Add(Tuple.Create<int, Channels?, CampaignForecast?>(cmpid, null, null));
        }
        public static void RemoveCampaign(int cmpid)
        {
            for (int i=0; i<list.Count; i++)
            {
                int listCmpid = list[i].Item1;
                if (listCmpid == cmpid)
                {
                    list.RemoveAt(i);
                }
            }
        }

        public static void AddChannels(int cmpid, Channels channels)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int listCmpid = list[i].Item1;
                if (listCmpid == cmpid)
                {
                    list[i] = Tuple.Create<int, Channels? ,CampaignForecast?>(cmpid, channels, list[i].Item3);
                    if (list[i].Item3 != null)
                    {
                        // Add events
                    }
                }
            }
        }

        public static void AddForecast(int cmpid, CampaignForecast forecast)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int listCmpid = list[i].Item1;
                if (listCmpid == cmpid)
                {
                    list[i] = Tuple.Create<int, Channels?, CampaignForecast?>(cmpid, list[i].Item2, forecast);
                    if (list[i].Item2 != null)
                    {
                        // Add events
                    }
                }
            }
        }
    }
}
