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
        private decimal? cpp  = 1.0M;
        private decimal? dpcoef = 1.0M;
        private decimal? seascoef  = 1.0M;
        private decimal? seccoef  = 1.0M;
        private decimal? progcoef  = 1.0M;
        private decimal? chcoef  = 1.0M;
        private decimal? coefA  = 1.0M;
        private decimal? coefB  = 1.0M;
        private bool? accept = false;

        public bool? Accept
        {
            get { return status == -1 ? null : accept; }
            set 
            { 
                accept = value;
                OnPropertyChanged();
            }
        }

        public string Date { get { return date; } }
        public decimal? price {
            get { return status == -1 ? null : _price; }
            set { 
                _price = value;
                OnPropertyChanged();
            } 
        }
        public int? Status { 
            get { return status == -1 ? null : status; }
            set
            {
                status = value;
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
            set
            {
                cpp = value;
                OnPropertyChanged();
            }
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

        public decimal? Dpcoef { 
            get { return status == -1 ? null : dpcoef; }
            set
            {
                dpcoef = value;
                OnPropertyChanged();
            }
        }
        public decimal? Seascoef { 
            get { return status == -1 ? null : seascoef; }
            set
            {
                seascoef = value;
                OnPropertyChanged();
            }
        }
        public decimal? Seccoef { 
            get { return status == -1 ? null : seccoef; }
            set
            {
                seccoef = value;
                OnPropertyChanged();
            }
        }
        public decimal? Progcoef { 
            get { return status == -1 ? null : progcoef; }
            set
            {
                progcoef = value;
                OnPropertyChanged();
            }
        }
        public decimal? Chcoef { 
            get { return status == -1 ? null : chcoef; }
            set
            {
                chcoef = value;
                OnPropertyChanged();
            }
        }
        public decimal? CoefA { 
            get { return status == -1 ? null : coefA; }
            set
            {
                coefA = value;
                OnPropertyChanged();
            }
        }
        public decimal? CoefB { 
            get { return status == -1 ? null : coefB; }
            set
            {
                coefB = value;
                OnPropertyChanged();
            }
        }

        public string? spotname { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
