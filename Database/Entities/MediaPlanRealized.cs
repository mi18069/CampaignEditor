using Database.DTOs.ChannelDTO;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Database.Entities
{
    public class MediaPlanRealized : INotifyPropertyChanged
    {
        private decimal? _price;
        private int? _status;
        public MediaPlan? MediaPlan { get; set; }
        public ChannelDTO Channel { get; set; } 

        public int? id { get; set; }
        public int? cmpid { get; set; }
        public string name { get; set; }
        public int? stime { get; set; }
        public int? etime { get; set; }
        public string stimestr { get; set; }
        public string etimestr { get; set; }
        public int? chid { get; set; }
        public int? dure { get; set; }
        public int? durf { get; set; }
        public string date { get; set; }
        public int? emsnum { get; set; }
        public int? posinbr { get; set; }
        public int? totalspotnum { get; set; }
        public int? breaktype { get; set; }
        public int? spotnum { get; set; }
        public int? brandnum { get; set; }
        public decimal amrp1 { get; set; }
        public decimal amrp2 { get; set; }
        public decimal amrp3 { get; set; }
        public decimal amrpsale { get; set; }
        public decimal? cpp { get; set; }
        public decimal? dpcoef { get; set; }
        public decimal? seascoef { get; set; }
        public decimal? seccoef { get; set; }
        public decimal? progcoef { get; set; }
        public decimal? chcoef { get; set; }
        public decimal? coefA { get; set; }
        public decimal? coefB { get; set; }

        public string Date { get { return date; } }
        public decimal? price {
            get { return _price; }
            set { 
                _price = value;
                OnPropertyChanged();
            } 
        }
        public int? status { 
            get { return _status; } 
            set { 
                _status = value; 
                OnPropertyChanged(); 
            } 
        }

        public decimal? Cpp
        {
            get {  return status == -1 ? null : cpp; }
        }
        public decimal? Amrp1
        {
            get { return status == -1 ? null : amrp1; }
        }
        public decimal? Amrp2
        {
            get { return status == -1 ? null : amrp2; }
        }
        public decimal? Amrp3
        {
            get { return status == -1 ? null : amrp3; }
        }
        public decimal? Amrpsale
        {
            get { return status == -1 ? null : amrpsale; }
        }

        public string? spotname { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
