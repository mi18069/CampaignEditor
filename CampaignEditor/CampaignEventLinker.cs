using CampaignEditor.UserControls;
using System;
using System.Collections.Generic;

namespace CampaignEditor
{
    public static class CampaignEventLinker
    {

        // This class is used to connect classes, so when something is changed within overview,
        // it is delegated to forecast

        public static List<CampaignLinker> _linkers = new List<CampaignLinker>();
        public static void AddCampaign(int cmpid)
        {
            var linker = new CampaignLinker();
            linker.cmpid = cmpid;
            _linkers.Add(linker);
        }
        public static void RemoveCampaign(int cmpid)
        {
            for (int i=0; i< _linkers.Count; i++)
            {
                var linker = _linkers[i];
                if (linker.cmpid == cmpid)
                {
                    _linkers.RemoveAt(i);
                    return;
                }
            }
        }

        public static void AddChannels(int cmpid, Channels channels)
        {
            for (int i = 0; i < _linkers.Count; i++)
            {
                int linkerCmpid = _linkers[i].cmpid;
                if (linkerCmpid == cmpid)
                {
                    _linkers[i].channels = channels;
                    if (_linkers[i].forecast != null)
                    {
                        // Add trigger
                    }

                }
            }
        }

        public static void AddInfo(int cmpid, CmpInfo cmpInfo)
        {
            for (int i = 0; i < _linkers.Count; i++)
            {
                int linkerCmpid = _linkers[i].cmpid;
                if (linkerCmpid == cmpid)
                {
                    _linkers[i].cmpInfo = cmpInfo;
                    if (_linkers[i].forecast != null)
                    {
                        // Add trigger
                    }

                }
            }
        }

        public static void AddGoals(int cmpid, Goals goals)
        {
            /*for (int i = 0; i < _linkers.Count; i++)
            {
                int linkerCmpid = _linkers[i].cmpid;
                if (linkerCmpid == cmpid)
                {
                    _linkers[i].goals = goals;
                    if (_linkers[i].forecast != null)
                    {
                        _linkers[i].goals.GoalsChanged += _linkers[i].forecast.GoalsChanged;
                    }

                }
            }*/
        }

        public static void AddSpots(int cmpid, Spots spots)
        {
            for (int i = 0; i < _linkers.Count; i++)
            {
                int linkerCmpid = _linkers[i].cmpid;
                if (linkerCmpid == cmpid)
                {
                    _linkers[i].spots = spots;
                    if (_linkers[i].forecast != null)
                    {
                        //_linkers[i].spots.SpotsChanged += _linkers[i].forecast.SpotsChanged;
                    }

                }
            }
        }

        public static void AddForecast(int cmpid, CampaignForecast forecast)
        {
            /*for (int i = 0; i < _linkers.Count; i++)
            {
                int linkerCmpid = _linkers[i].cmpid;
                if (linkerCmpid == cmpid)
                {
                    _linkers[i].forecast = forecast;
                    if (_linkers[i].cmpInfo != null)
                    {
                        // Add trigger
                        var a = 5;

                    }
                    if (_linkers[i].channels != null)
                    {
                        // Add trigger
                        var a = 5;
                    }
                    if (_linkers[i].goals != null)
                    {
                        _linkers[i].goals.GoalsChanged += _linkers[i].forecast.GoalsChanged;
                    }
                    if (_linkers[i].spots != null)
                    {
                        _linkers[i].spots.SpotsChanged += _linkers[i].forecast.SpotsChanged;
                    }

                }
            }*/
        }

        public class CampaignLinker
        {
            public int cmpid;
            public CmpInfo? cmpInfo;
            public Goals? goals;
            public Spots? spots;
            public Channels? channels;
            public CampaignForecast? forecast;

            public CampaignLinker()
            {
                cmpid = -1;
                cmpInfo = null;
                goals = null;
                spots = null;
                channels = null;
                forecast = null;
            }
        }
    }
}
